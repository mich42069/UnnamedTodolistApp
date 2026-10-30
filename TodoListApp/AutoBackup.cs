namespace TodoListApp;

public class AutoBackup(Android.App.Activity host, IBackend backend, SettingsStore store,
    Func<ColorProfile> getProfile, Action<ColorProfile?> onRestored)
{
    private const int PickCode = 3;
    private const string FileName = "todo_autobackup.json";
    private const ActivityFlags Rw = ActivityFlags.GrantReadUriPermission | ActivityFlags.GrantWriteUriPermission;

    public void PickFolder() => host.StartActivityForResult(
        new Intent(Intent.ActionOpenDocumentTree).AddFlags(Rw | ActivityFlags.GrantPersistableUriPermission)!, PickCode);

    public void HandleResult(int requestCode, Result resultCode, Intent? data)
    {
        if (requestCode != PickCode || resultCode != Result.Ok || data?.Data is not { } tree) return;
        host.ContentResolver!.TakePersistableUriPermission(tree, Rw);
        store.AutoBackupFolder = tree.ToString();

        var hasData = backend.GetActivityHandler().GetActivities().Any();
        if (!hasData && TryRestore(tree, out var profile))
        {
            onRestored(profile);
            Toast.MakeText(host, "Restored from folder", ToastLength.Short)!.Show();
        }
        else Write();
    }

    public void Write()
    {
        // never overwrite a good backup with an empty app
        if (store.AutoBackupFolder is not { } folder
            || !backend.GetActivityHandler().GetActivities().Any()) return;
        try
        {
            var tree = Android.Net.Uri.Parse(folder)!;
            var resolver = host.ContentResolver!;
            using var buffer = new MemoryStream();
            if (!BackupFormat.Export(backend, getProfile(), buffer)) return;

            var file = DocumentTree.FindChild(resolver, tree, FileName)
                ?? DocumentsContract.CreateDocument(resolver,
                    DocumentsContract.BuildDocumentUriUsingTree(tree, DocumentsContract.GetTreeDocumentId(tree))!,
                    "application/json", FileName);
            if (file is null) return;
            using var stream = resolver.OpenOutputStream(file, "wt")!;
            buffer.WriteTo(stream);
        }
        catch (Exception) { } // folder removed or permission revoked
    }

    private bool TryRestore(Android.Net.Uri tree, out ColorProfile? profile)
    {
        profile = null;
        if (DocumentTree.FindChild(host.ContentResolver!, tree, FileName) is not { } file) return false;
        using var stream = host.ContentResolver!.OpenInputStream(file)!;
        return BackupFormat.Import(backend, stream, getProfile(), out profile);
    }
}