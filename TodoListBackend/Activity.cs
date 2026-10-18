namespace TodoListBackend;

public struct Activity(string name, DateTime deadline, ActivityType type, string description = "no-description")
    : IComparable<Activity>
{
    public Guid Id = Guid.NewGuid();
    public string Name = name;
    public ActivityType Type = type;
    public string? Description = description;
    public DateTime Deadline = deadline;

    public int CompareTo(Activity other)
    {
        return Deadline.CompareTo(other.Deadline);
    }
}