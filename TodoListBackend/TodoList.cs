using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace TodoListBackend;

public partial class TodoList : ITodoList
{
	public string Name { get; set; }
	public Guid Guid { get; set; } = Guid.NewGuid();
	private readonly string _filePath;
	private static readonly JsonSerializerOptions Options = new() { IncludeFields = true, WriteIndented = true };
	private ActivityHandler _activityHandler = new();
	private ActivityTypeHandler _activityTypeHandler = new();
	public JsonObject Settings { get; set; } = new();

	public IActivityTypeHandler GetActivityTypeHandler()
	{
		return _activityTypeHandler;
	}

	public IActivityHandler GetActivityHandler()
	{
		return _activityHandler;
	}
}