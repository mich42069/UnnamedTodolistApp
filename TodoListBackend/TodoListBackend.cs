using System.Collections.Concurrent;
using System.Text.Json;

namespace TodoListBackend;

public class TodoListBackend : ITodoListBackend
{
    
    private readonly string _filePath;
    private ActivityTypeHandler _activityTypeHandler = new ();
    private static readonly JsonSerializerOptions Options = new() { IncludeFields = true, WriteIndented = true };
    private ActivityHandler _activityHandler = new();
    public TodoListBackend(string filePath) => _filePath = filePath;
    
    public bool Save()
    {
        return _activityHandler.Save(_filePath, Options);
    }

    public bool Load()
    {
        return _activityHandler.Load(_filePath, Options);
    }
    public bool Export(Stream stream)
    {
        return _activityHandler.Export(stream, Options);
    }

    public bool Import(Stream stream)
    {
        return _activityHandler.Import(stream, Options);
    }

    public IActivityTypeHandler GetActivityTypeHandler()
    {
        return _activityTypeHandler;
    }

    public IActivityHandler GetActivityHandler()
    {
        return _activityHandler;
    }
}