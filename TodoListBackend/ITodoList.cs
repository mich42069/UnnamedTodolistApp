using System.Text.Json.Nodes;

namespace TodoListBackend;

public interface ITodoList
{
	public bool                 Export(Stream stream);
	public bool                 Import(Stream stream);
	public IActivityTypeHandler GetActivityTypeHandler();
	public IActivityHandler     GetActivityHandler();
	public JsonObject Settings { get; set; }
	public Guid Guid { get; set; }
	public string Name { get; set; }
}