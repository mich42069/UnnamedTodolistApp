namespace TodoListApp;

public class BackupManager(Android.App.Activity host, IBackend backend,
    Func<ColorProfile> getProfile, Action<ColorProfile?> onImported)

{
    private const int ExportCode = 1, ImportCode = 2;

    public void ShowDialog()
    {
        new AlertDialog.Builder(host)!
            .SetItems(["Export to file", "Import from file"], (_, e) =>
            {
                if (e.Which == 0)
                    host.StartActivityForResult(new Intent(Intent.ActionOpenDocumentTree)
                            .AddFlags(ActivityFlags.GrantReadUriPermission | ActivityFlags.GrantWriteUriPermission)!,
                        ExportCode);
                else
                    host.StartActivityForResult(new Intent(Intent.ActionOpenDocument)
                        .AddCategory(Intent.CategoryOpenable)!
                        .SetType("*/*")!, ImportCode);
            })!
            .Show();
    }

    public void HandleResult(int requestCode, Result resultCode, Intent? data)
    {
        if (requestCode is not (ExportCode or ImportCode)
            || resultCode != Result.Ok || data?.Data is not { } uri) return;

        bool ok;
        if (requestCode == ExportCode)
            ok = ExportToFolder(uri);
        else
        {
            using var stream = host.ContentResolver!.OpenInputStream(uri)!;
            ok = BackupFormat.Import(backend, stream, getProfile(), out var profile);
            onImported(profile);
        }

        Toast.MakeText(host, ok ? "Done" : "Failed", ToastLength.Short)!.Show();
    }

    private bool ExportToFolder(Android.Net.Uri treeUri)
    {
        var old = DocumentTree.FindChild(host.ContentResolver!, treeUri, "activities_old.json");
        var current = DocumentTree.FindChild(host.ContentResolver!, treeUri, "activities.json");
        var dir = DocumentsContract.BuildDocumentUriUsingTree(treeUri, DocumentsContract.GetTreeDocumentId(treeUri));
        var file = DocumentsContract.CreateDocument(host.ContentResolver!, dir!, "application/json", "activities.json");
        if (file is null) return false;

        using var stream = host.ContentResolver!.OpenOutputStream(file, "wt")!;
        return BackupFormat.Export(backend, getProfile(), stream);
    }
}