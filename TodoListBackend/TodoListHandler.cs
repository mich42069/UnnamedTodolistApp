using System.Text.Json;
using System.Text.Json.Nodes;

namespace TodoListBackend;

public class TodoListHandler : ITodoListHandler
{
    private const string Marker = "backupVersion";
    private const int Version = 4;
    private const string DefaultName = "Todo list";

    private record Entry(Guid Id, string Name, JsonObject Settings, byte[] Data);
    private record Parsed(Guid DefaultId, bool Migrated, List<Entry> Entries);

    private readonly string _path;
    private readonly List<ITodoList> _lists = [];
    private Guid _currentId;

    public Guid DefaultId { get; set; }
    public event Action<ITodoList>? CurrentChanged;

    public TodoListHandler(string savePath)
    {
        _path = savePath;
        var first = new TodoList { Name = DefaultName };
        _lists.Add(first);
        DefaultId = _currentId = first.Guid;
    }

    public ITodoList Current() => _lists.First(l => l.Guid == _currentId);
    public List<ITodoList> GetAll() => [.._lists];
    public ITodoList? GetById(Guid id) => _lists.FirstOrDefault(l => l.Guid == id);

    public bool SetCurrent(Guid id)
    {
        var list = GetById(id);
        if (list is null) return false;
        _currentId = id;
        CurrentChanged?.Invoke(list);
        return true;
    }

    public ITodoList? CreateTodoList(string name)
    {
        var list = new TodoList { Name = name };
        _lists.Add(list);
        Save();
        return list;
    }

    public bool RemoveTodoList(Guid id)
    {
        if (_lists.Count <= 1 || _lists.RemoveAll(l => l.Guid == id) == 0) return false;
        if (DefaultId == id) DefaultId = _lists[0].Guid;
        if (_currentId == id) SetCurrent(DefaultId);
        return Save();
    }

    public bool Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(_path))!);
            var tmp = _path + ".tmp";
            using (var stream = File.Create(tmp))
                if (!Export(stream)) return false;
            File.Move(tmp, _path, overwrite: true);
            return true;
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException) { return false; }
    }

    public bool Load()
    {
        if (!File.Exists(_path)) return false;
        try
        {
            var parsed = Parse(File.ReadAllBytes(_path));
            var built = Build(parsed.Entries);
            if (built is not { Count: > 0 }) throw new InvalidDataException();

            if (parsed.Migrated && !File.Exists(_path + ".legacy.bak")) File.Copy(_path, _path + ".legacy.bak");
            _lists.Clear();
            _lists.AddRange(built);
            DefaultId = built.Any(l => l.Guid == parsed.DefaultId) ? parsed.DefaultId : built[0].Guid;
            _currentId = DefaultId;
            return !parsed.Migrated || Save();
        }
        catch
        {
            try { File.Copy(_path, _path + ".failed.bak", overwrite: true); } catch { }
            return false;
        }
    }

    public bool Export(Stream stream)
    {
        try
        {
            var lists = new JsonArray();
            foreach (var l in _lists)
            {
                using var raw = new MemoryStream();
                if (!l.Export(raw)) return false;
                lists.Add(new JsonObject
                {
                    ["id"] = l.Guid.ToString(),
                    ["name"] = l.Name,
                    ["settings"] = l.Settings.DeepClone(),
                    ["data"] = JsonNode.Parse(raw.ToArray())
                });
            }
            var root = new JsonObject { [Marker] = Version, ["defaultId"] = DefaultId.ToString(), ["lists"] = lists };
            using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
            root.WriteTo(writer);
            return true;
        }
        catch (Exception e) when (e is IOException or JsonException) { return false; }
    }

    public bool Import(Stream stream)
    {
        try
        {
            using var buffer = new MemoryStream();
            stream.CopyTo(buffer);
            var built = Build(Parse(buffer.ToArray()).Entries);
            if (built is null) return false;

            foreach (var list in built)
            {
                var i = _lists.FindIndex(l => l.Guid == list.Guid);
                if (i >= 0) _lists[i] = list; else _lists.Add(list);
            }
            return Save();
        }
        catch { return false; }
    }

    private static Parsed Parse(byte[] bytes)
    {
        if (JsonNode.Parse(bytes) is not JsonObject root || !root.ContainsKey(Marker))
            return Single(bytes, new JsonObject());                 // raw payload: your current save file, activities-only backups

        if (root["lists"] is not JsonArray array)                   // backupVersion 1-3
        {
            var settings = new JsonObject();
            foreach (var key in new[] { "colorProfile", "deadlineColors" })
                if (root[key] is { } value) settings[key] = value.DeepClone();
            return Single(Bytes(root["activities"]), settings);
        }

        var entries = array.Select(n => new Entry(
            Guid.Parse((string)n!["id"]!), (string)n["name"]!,
            n["settings"]?.DeepClone() as JsonObject ?? new JsonObject(), Bytes(n["data"]))).ToList();
        return new Parsed(Guid.TryParse((string?)root["defaultId"], out var id) ? id : Guid.Empty, false, entries);
    }

    private static Parsed Single(byte[] data, JsonObject settings) =>
        new(Guid.Empty, true, [new(Guid.NewGuid(), DefaultName, settings, data)]);

    private static byte[] Bytes(JsonNode? node) => JsonSerializer.SerializeToUtf8Bytes(node);

    private static List<ITodoList>? Build(List<Entry> entries)
    {
        var result = new List<ITodoList>();
        foreach (var e in entries)
        {
            var list = new TodoList { Guid = e.Id, Name = e.Name, Settings = e.Settings };
            if (!list.Import(new MemoryStream(e.Data))) return null;
            result.Add(list);
        }
        return result;
    }
}