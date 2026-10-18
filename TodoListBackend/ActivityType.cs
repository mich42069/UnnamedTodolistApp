namespace TodoListBackend;

public readonly record struct ActivityType(string type)
{
    public Guid Guid { get; } = Guid.NewGuid();
    public string Type => type;
}