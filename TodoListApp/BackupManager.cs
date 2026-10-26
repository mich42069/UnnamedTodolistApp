namespace TodoListApp;

public class BackupManager(Android.App.Activity host, IBackend backend,
    Func<DeadlineColorSettings> getColors, Action<DeadlineColorSettings?> onImported)
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
            ok = BackupFormat.Import(backend, stream, out var colors);
            onImported(colors);
        }

        Toast.MakeText(host, ok ? "Done" : "Failed", ToastLength.Short)!.Show();
    }

    private bool ExportToFolder(Android.Net.Uri treeUri)
    {
        var old = FindChild(treeUri, "activities_old.json");
        if (old is not null) DocumentsContract.DeleteDocument(host.ContentResolver!, old);

        var current = FindChild(treeUri, "activities.json");
        if (current is not null) DocumentsContract.RenameDocument(host.ContentResolver!, current, "activities_old.json");

        var dir = DocumentsContract.BuildDocumentUriUsingTree(treeUri, DocumentsContract.GetTreeDocumentId(treeUri));
        var file = DocumentsContract.CreateDocument(host.ContentResolver!, dir!, "application/json", "activities.json");
        if (file is null) return false;

        using var stream = host.ContentResolver!.OpenOutputStream(file, "wt")!;
        return BackupFormat.Export(backend, getColors(), stream);
    }

    private Android.Net.Uri? FindChild(Android.Net.Uri treeUri, string name)
    {
        var children = DocumentsContract.BuildChildDocumentsUriUsingTree(
            treeUri, DocumentsContract.GetTreeDocumentId(treeUri));
        using var cursor = host.ContentResolver!.Query(children!,
            [DocumentsContract.Document.ColumnDocumentId, DocumentsContract.Document.ColumnDisplayName],
            null, null, null);

        while (cursor is not null && cursor.MoveToNext())
            if (cursor.GetString(1) == name)
                return DocumentsContract.BuildDocumentUriUsingTree(treeUri, cursor.GetString(0));
        return null;
    }
}