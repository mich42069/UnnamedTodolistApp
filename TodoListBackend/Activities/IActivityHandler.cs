namespace TodoListBackend;

public interface IActivityHandler
{
    public Activity GetActivity(Guid activityId);
    public bool TryAddActivity(Guid activityId, Activity activity);
    public bool DeleteActivity(Guid activityId);
    public bool CreateActivity(Activity activity);
    public IEnumerable<Activity> GetActivities();
    public IEnumerable<Guid> GetActivityIds();
}