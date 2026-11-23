using System.Text.Json;

namespace TodoListBackend;

public partial class TodoList
{
	private sealed class BackupData
	{
		public List<ActivityType> Types { get; set; } = [];
		public List<Activity> Activities { get; set; } = [];
	}

	public bool Export(Stream stream)
	{
		try
		{
			JsonSerializer.Serialize(stream, new BackupData
			{
				Types = _activityTypeHandler.GetActivityTypes().ToList(),
				Activities = _activityHandler.GetActivities().ToList()
			}, Options);
			return true;
		}
		catch (Exception e) when (e is IOException or NotSupportedException)
		{
			return false;
		}
	}

	public bool Import(Stream stream) => Read(stream);

	private bool Read(Stream stream)
	{
		try
		{
			var data = JsonSerializer.Deserialize<BackupData>(stream, Options);
			if (data is null) return false;

			foreach (var type in data.Types) _activityTypeHandler.TryAdd(type);
			foreach (var activity in data.Activities)
			{
				_activityTypeHandler.TryAdd(activity.Type);
				_activityHandler.TryAddActivity(activity.Id, activity);
			}

			return true;
		}
		catch (Exception e) when (e is IOException or JsonException)
		{
			return false;
		}
	}
}