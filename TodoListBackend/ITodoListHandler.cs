namespace TodoListBackend;

public interface ITodoListHandler
{
	event Action<ITodoList>? CurrentChanged;
	Guid DefaultId { get; set; }
	bool            Save();
	bool            Load();
	bool            Export(Stream stream);
	bool            Import(Stream stream);
	ITodoList       Current();
	bool            SetCurrent(Guid id);
	List<ITodoList> GetAll();
	ITodoList?      GetById(Guid id);
	ITodoList?      CreateTodoList(string name);
	bool            RemoveTodoList(Guid id);
}