namespace TodoListBackend;

public class ActivityTypeHandler : IActivityTypeHandler
{
    private List<ActivityType> _activityTypes = new();
    public IEnumerable<ActivityType> GetActivityTypes()
    {
        return _activityTypes;
    }

    public bool AddActivityType(string typeName)
    {
        var activityType = new ActivityType(typeName);
        if (_activityTypes.Contains(activityType))
            return false;
        _activityTypes.Add(activityType);
        return true;
    }

    public bool RemoveActivityType(Guid activityTypeId)
    {
        foreach (var type in _activityTypes)
        {
            if (type.Id == activityTypeId)
            {
                return _activityTypes.Remove(type);
            }
        }
        return false;
    }
    
    public void Clear() => _activityTypes.Clear();

    public bool TryAdd(ActivityType type)
    {
        if (_activityTypes.Any(t => t.Id == type.Id)) return false;
        _activityTypes.Add(type);
        return true;
    }
}