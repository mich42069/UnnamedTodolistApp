namespace TodoListBackend;

public readonly record struct ActivityType(Guid Id, string Name)
{
    public ActivityType(string name) : this(Guid.NewGuid(), name) { }
}