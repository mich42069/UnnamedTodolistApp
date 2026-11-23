namespace TodoListApp;

public class AutoBackup(
	Android.App.Activity host,
	ITodoListHandler handler,
	SettingsStore store,
	Action onRestored)
{
	private bool HasData => handler.GetAll().Any(l => l.GetActivityHandler().GetActivities().Any());
	private const int PickCode = 3;
	private const string FileName = "todo_autobackup.json";
	private const ActivityFlags Rw = ActivityFlags.GrantReadUriPermission | ActivityFlags.GrantWriteUriPermission;

	public void PickFolder() => host.StartActivityForResult(
		new Intent(Intent.ActionOpenDocumentTree).AddFlags(Rw | ActivityFlags.GrantPersistableUriPermission)!,
		PickCode);

	public void HandleResult(int requestCode, Result resultCode, Intent? data)
	{
		if (requestCode != PickCode || resultCode != Result.Ok || data?.Data is not { } tree) return;
		host.ContentResolver!.TakePersistableUriPermission(tree, Rw);

		store.AutoBackupFolder = tree.ToString();
		if (!HasData && TryRestore(tree))
		{
			onRestored();
			Toast.MakeText(host, "Restored from folder", ToastLength.Short)!.Show();
		}
		else Write();
	}

	public void Write()
	{
		// never overwrite a good backup with an empty app
		if (store.AutoBackupFolder is not { } folder || !HasData) return;
		try
		{
			var tree = Android.Net.Uri.Parse(folder)!;
			var resolver = host.ContentResolver!;
			using var buffer = new MemoryStream();
			if (!handler.Export(buffer)) return;

			var file = DocumentTree.FindChild(resolver, tree, FileName)
					   ?? DocumentsContract.CreateDocument(resolver,
						   DocumentsContract.BuildDocumentUriUsingTree(tree,
							   DocumentsContract.GetTreeDocumentId(tree))!,
						   "application/json", FileName);
			if (file is null) return;
			using var stream = resolver.OpenOutputStream(file, "wt")!;
			buffer.WriteTo(stream);
		}
		catch (Exception)
		{
		} // folder removed or permission revoked
	}

	private bool TryRestore(Android.Net.Uri tree)
	{
		if (DocumentTree.FindChild(host.ContentResolver!, tree, FileName) is not { } file) return false;
		var empty = handler.GetAll().Select(l => l.Guid).ToList();
		using var stream = host.ContentResolver!.OpenInputStream(file)!;
		if (!handler.Import(stream)) return false;
		foreach (var id in empty) handler.RemoveTodoList(id);
		return true;
	}
}