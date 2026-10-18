namespace TodoListBackend;

public interface IActivityTypeHandler
{
    public IEnumerable<ActivityType> GetActivityTypes();
    public bool AddActivityType(string typeName);
    public bool RemoveActivityType(Guid activityTypeId);
}