namespace TodoListBackend;

public interface ITodoListBackend
{
    public Activity GetActivity(Guid activityId);
    public bool DeleteActivity(Guid activityId);
    public bool CreateActivity(Activity activity);
    public IEnumerable<Activity> GetActivities();
    public IEnumerable<Guid> GetActivityIds();
    public bool Save();
    public bool Load();
    public bool Export(Stream stream);
    public bool Import(Stream stream);
}