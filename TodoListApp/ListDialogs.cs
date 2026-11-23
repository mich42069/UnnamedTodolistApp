using TodoListApp;
using TodoListBackend;

public class ListDialogs(Android.App.Activity context, ITodoListHandler handler, Action onEdited)
{
	public void ShowSwitcher()
	{
		var lists = handler.GetAll();
		var current = handler.Current().Guid;
		var titles = lists.Select(l => (l.Guid == current ? "✓ " : "") + l.Name).Append("+ New list").ToArray();

		var dialog = new AlertDialog.Builder(context)!
					 .SetTitle("Todo lists")!
					 .SetItems(titles, (_, e) =>
					 {
						 if (e.Which < lists.Count) handler.SetCurrent(lists[e.Which].Guid);
						 else ShowNewList();
					 })!
					 .Show()!;

		dialog.ListView!.ItemLongClick += (_, e) =>
		{
			e.Handled = true;
			if (e.Position >= lists.Count) return;
			dialog.Dismiss();
			ShowListOptions(lists[e.Position]);
		};
	}

	public void ShowStartOn()
	{
		var lists = handler.GetAll();
		new AlertDialog.Builder(context)!
			.SetTitle("Start on")!
			.SetSingleChoiceItems(lists.Select(l => l.Name).ToArray(),
				lists.FindIndex(l => l.Guid == handler.DefaultId), (sender, e) =>
				{
					handler.DefaultId = lists[e.Which].Guid;
					handler.Save();
					(sender as IDialogInterface)?.Dismiss();
				})!
			.Show();
	}

	private void ShowListOptions(IBackend list)
	{
		new AlertDialog.Builder(context)!
			.SetTitle(list.Name)!
			.SetItems(["Rename", "Deadline colors", $"Reminders: {(list.RemindersEnabled() ? "on" : "off")}", "Delete"],
				(_, e) =>
				{
					switch (e.Which)
					{
						case 0:
							ShowNameDialog("Rename list", list.Name, "Rename", name =>
							{
								list.Name = name;
								handler.Save();
								onEdited();
								ShowSwitcher();
							});
							break;
						case 1:
							ShowColors(list);
							break;
						case 2:
							list.SetRemindersEnabled(!list.RemindersEnabled());
							handler.Save();
							ShowSwitcher();
							break;
						default:
							ShowDelete(list);
							break;
					}
				})!
			.Show();
	}

	private void ShowColors(IBackend list)
	{
		var profile = list.GetColorProfile();
		new PresetPickerDialog(context, profile, () =>
		{
			list.SetColorProfile(profile);
			handler.Save();
			onEdited();
		}).Show();
	}

	private void ShowNewList() => ShowNameDialog("New list", "", "Create", name =>
	{
		if (handler.CreateTodoList(name) is not { } list) return;
		list.SetColorProfile(new ColorProfile());
		handler.SetCurrent(list.Guid);
		handler.Save();
	});

	private void ShowDelete(IBackend list)
	{
		if (handler.GetAll().Count <= 1)
		{
			Toast.MakeText(context, "Can't delete the last list", ToastLength.Short)!.Show();
			ShowSwitcher();
			return;
		}

		new AlertDialog.Builder(context)!
			.SetTitle($"Delete \"{list.Name}\" and all its activities?")!
			.SetPositiveButton("Delete", (_, _) =>
			{
				handler.RemoveTodoList(list.Guid);
				ShowSwitcher();
			})!
			.SetNegativeButton("Cancel", (_, _) => ShowSwitcher())!
			.Show();
	}

	private void ShowNameDialog(string title, string initial, string ok, Action<string> onName)
	{
		var input = new EditText(context) { Hint = "List name", Text = initial };
		new AlertDialog.Builder(context)!
			.SetTitle(title)!
			.SetView(input)!
			.SetPositiveButton(ok, (_, _) =>
			{
				if (string.IsNullOrWhiteSpace(input.Text)) ShowSwitcher();
				else onName(input.Text.Trim());
			})!
			.SetNegativeButton("Cancel", (_, _) => ShowSwitcher())!
			.Show();
	}
}