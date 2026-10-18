namespace TodoListBackend;

public class ActivityTypeHandler : IActivityTypeHandler
{
    private List<ActivityType> _activityTypes;
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
            if (type.Guid == activityTypeId)
            {
                return _activityTypes.Remove(type);
            }
        }
        return false;
    }
}