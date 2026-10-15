using System.Collections.Concurrent;
using System.Text.Json;

namespace TodoListBackend;

public class TodoListBackend : ITodoListBackend
{
    private readonly ConcurrentDictionary<Guid, Activity> _activities = new();
    
    private readonly string _filePath;
    private static readonly JsonSerializerOptions Options = new() { IncludeFields = true, WriteIndented = true };
    
    public TodoListBackend(string filePath) => _filePath = filePath;
    
    public Activity GetActivity(Guid activityId) =>
        _activities.TryGetValue(activityId, out var activity)
            ? activity
            : throw new KeyNotFoundException($"Activity {activityId} not found.");

    public bool DeleteActivity(Guid activityId) => _activities.TryRemove(activityId, out _);

    public bool CreateActivity(Activity activity) => _activities.TryAdd(activity.Id, activity);

    public IEnumerable<Activity> GetActivities() => _activities.Values;

    public IEnumerable<Guid> GetActivityIds() => _activities.Keys;
    
    public bool Save()
    {
        try
        {
            File.WriteAllText(_filePath, JsonSerializer.Serialize(_activities.Values, Options));
            return true;
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }

    public bool Load()
    {
        if (!File.Exists(_filePath)) return false;
        try
        {
            var loaded = JsonSerializer.Deserialize<List<Activity>>(File.ReadAllText(_filePath), Options);
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
    public bool Export(Stream stream)
    {
        try
        {
            JsonSerializer.Serialize(stream, _activities.Values, Options);
            return true;
        }
        catch (Exception e) when (e is IOException or NotSupportedException) { return false; }
    }

    public bool Import(Stream stream)
    {
        try
        {
            var loaded = JsonSerializer.Deserialize<List<Activity>>(stream, Options);
            if (loaded is null) return false;
            foreach (var activity in loaded) _activities.TryAdd(activity.Id, activity);
            return true;
        }
        catch (Exception e) when (e is IOException or JsonException) { return false; }
    }
}