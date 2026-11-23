using System.Collections.Concurrent;
using System.Text.Json;


namespace TodoListBackend;

public class ActivityHandler : IActivityHandler
{
	private readonly ConcurrentDictionary<Guid, Activity> _activities = new();

	public bool SetCompleted(Guid activityId, DateTime? completedAt)
	{
		if (!_activities.TryGetValue(activityId, out var activity)) return false;
		activity.CompletedAt = completedAt;
		_activities[activityId] = activity;
		return true;
	}

	public Activity GetActivity(Guid activityId) =>
		_activities.TryGetValue(activityId, out var activity)
			? activity
			: throw new KeyNotFoundException($"Activity {activityId} not found.");

	public bool TryAddActivity(Guid activityId, Activity activity)
	{
		return _activities.TryAdd(activityId, activity);
	}

	public bool DeleteActivity(Guid activityId) => _activities.TryRemove(activityId, out _);

	public bool CreateActivity(Activity activity) => _activities.TryAdd(activity.Id, activity);

	public IEnumerable<Activity> GetActivities() => _activities.Values;

	public IEnumerable<Guid> GetActivityIds() => _activities.Keys;

	public void Clear() => _activities.Clear();
}