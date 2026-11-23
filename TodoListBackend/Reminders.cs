using System.Text.Json.Nodes;

namespace TodoListBackend;

public record Reminder(string ListName, Guid ActivityId, string ActivityName, DateTime Deadline, DateTime FireAt);

public class ReminderSettings
{
	public List<TimeSpan> Offsets { get; set; } = [TimeSpan.FromDays(1), TimeSpan.Zero];
	public TimeSpan DateOnlyTime { get; set; } = TimeSpan.FromHours(9);
}

public static class ReminderListSettings
{
	private const string Key = "remindersEnabled";

	public static bool RemindersEnabled(this ITodoList list) => list.Settings[Key]?.GetValue<bool>() ?? true;
	public static void SetRemindersEnabled(this ITodoList list, bool on) => list.Settings[Key] = on;
}

public static class ReminderPlanner
{
	public static List<Reminder> Plan(ITodoListHandler handler, ReminderSettings settings)
	{
		var result = new List<Reminder>();
		foreach (var list in handler.GetAll().Where(l => l.RemindersEnabled()))
			foreach (var a in list.GetActivityHandler().GetActivities())
			{
				var due = a.Deadline.TimeOfDay == TimeSpan.Zero ? a.Deadline.Date + settings.DateOnlyTime : a.Deadline;
				result.AddRange(settings.Offsets.Select(o => new Reminder(list.Name, a.Id, a.Name, a.Deadline, due - o)));
			}
		return result;
	}

	public static List<Reminder> Due(IEnumerable<Reminder> all, DateTime after, DateTime upTo) =>
		all.Where(r => r.FireAt > after && r.FireAt <= upTo).OrderBy(r => r.FireAt).ToList();

	public static DateTime? Next(IEnumerable<Reminder> all, DateTime after) =>
		all.Where(r => r.FireAt > after).Select(r => (DateTime?)r.FireAt).Min();
}