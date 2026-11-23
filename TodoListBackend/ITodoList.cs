namespace TodoListBackend;

public interface ITodoList
{
    public bool Save();
    public bool Load();
    public bool Export(Stream stream);
    public bool Import(Stream stream);
    public IActivityTypeHandler GetActivityTypeHandler();
    public IActivityHandler GetActivityHandler();
}