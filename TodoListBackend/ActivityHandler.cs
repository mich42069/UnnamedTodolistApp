using System.Collections.Concurrent;
using System.Text.Json;


namespace TodoListBackend;

public class ActivityHandler : IActivityHandler
{

    private readonly ConcurrentDictionary<Guid, Activity> _activities = new();
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
    
    
    public bool Save(string filepath, JsonSerializerOptions options)
    {
        try
        {
            File.WriteAllText(filepath, JsonSerializer.Serialize(_activities.Values, options));
            return true;
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }

    public bool Load(string filepath, JsonSerializerOptions options)
    {
        if (!File.Exists(filepath)) return false;
        try
        {
            var loaded = JsonSerializer.Deserialize<List<Activity>>(File.ReadAllText(filepath), options);
            if (loaded is null) return false;

            _activities.Clear();
            foreach (var activity in loaded) _activities.TryAdd(activity.Id, activity);
            return true;
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or JsonException)
        {
            return false;
        }
    }
    public bool Export(Stream stream, JsonSerializerOptions options)
    {
        try
        {
            JsonSerializer.Serialize(stream, _activities.Values, options);
            return true;
        }
        catch (Exception e) when (e is IOException or NotSupportedException) { return false; }
    }

    public bool Import(Stream stream, JsonSerializerOptions options)
    {
        try
        {
            var loaded = JsonSerializer.Deserialize<List<Activity>>(stream, options);
            if (loaded is null) return false;
            foreach (var activity in loaded) _activities.TryAdd(activity.Id, activity);
            return true;
        }
        catch (Exception e) when (e is IOException or JsonException) { return false; }
    }
}