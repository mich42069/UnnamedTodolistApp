using System.Collections.Concurrent;
using System.Text.Json;

namespace TodoListBackend;

public partial class TodoList : ITodoList
{
    
    private readonly string _filePath;
    private static readonly JsonSerializerOptions Options = new() { IncludeFields = true, WriteIndented = true };
    private ActivityHandler _activityHandler = new();
    private ActivityTypeHandler _activityTypeHandler = new ();
    public TodoList(string filePath) => _filePath = filePath;

    public IActivityTypeHandler GetActivityTypeHandler()
    {
        return _activityTypeHandler;
    }

    public IActivityHandler GetActivityHandler()
    {
        return _activityHandler;
    }
}