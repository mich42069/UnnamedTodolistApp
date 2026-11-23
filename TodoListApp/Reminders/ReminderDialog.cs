using TodoListBackend;

namespace TodoListApp;

public class ReminderDialog(Context context, SettingsStore store, ITodoListHandler handler)
{
	private static readonly (string Label, TimeSpan Offset)[] Choices =
	[
		("1 week before", TimeSpan.FromDays(7)),
		("2 days before", TimeSpan.FromDays(2)),
		("1 day before", TimeSpan.FromDays(1)),
		("3 hours before", TimeSpan.FromHours(3)),
		("1 hour before", TimeSpan.FromHours(1)),
		("At the deadline", TimeSpan.Zero)
	];

	public void Show()
	{
		var settings = store.LoadReminderSettings();
		var on = Choices.Select(c => settings.Offsets.Contains(c.Offset)).ToArray();
		new AlertDialog.Builder(context)!
			.SetTitle("Remind me")!
			.SetMultiChoiceItems(Choices.Select(c => c.Label).ToArray(), on, (_, e) => on[e.Which] = e.IsChecked)!
			.SetPositiveButton("Save", (_, _) =>
			{
				ReminderService.Process(context, handler);
				settings.Offsets = Choices.Where((_, i) => on[i]).Select(c => c.Offset).ToList();
				store.SaveReminderSettings(settings);
				ReminderService.Process(context, handler);
			})!
			.SetNegativeButton("Cancel", (_, _) => { })!
			.Show();
	}
}