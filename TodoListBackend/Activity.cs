namespace TodoListBackend;

public struct Activity(string name, DateTime deadline, string description = "no-description")
    : IComparable<Activity>
{
    public Guid Id = Guid.NewGuid();
    public string Name = name;
    public string? Description = description;
    public DateTime Deadline = deadline;

    public int CompareTo(Activity other)
    {
        return Deadline.CompareTo(other.Deadline);
    }
}