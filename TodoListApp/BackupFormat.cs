using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace TodoListApp;

public static class BackupFormat
{
    private const string Marker = "backupVersion";

    public static bool Export(IBackend backend, DeadlineColorSettings colors, Stream output)
    {
        using var raw = new MemoryStream();
        if (!backend.Export(raw)) return false;

        var root = new JsonObject
        {
            [Marker] = 2,
            ["activities"] = JsonNode.Parse(raw.ToArray()),
            ["deadlineColors"] = JsonSerializer.SerializeToNode(colors)
        };
        using var writer = new Utf8JsonWriter(output);
        root.WriteTo(writer);
        return true;
    }

    public static bool Import(IBackend backend, Stream input, out DeadlineColorSettings? colors)
    {
        colors = null;
        using var buffer = new MemoryStream();
        input.CopyTo(buffer);
        var bytes = buffer.ToArray();

        try
        {
            if (JsonNode.Parse(bytes) is JsonObject root && root.ContainsKey(Marker))
            {
                colors = JsonSerializer.Deserialize<DeadlineColorSettings>(root["deadlineColors"]);
                colors?.Normalize();
                bytes = Encoding.UTF8.GetBytes(root["activities"]!.ToJsonString());
            }
        }
        catch (JsonException) { } // not our wrapper: treat as an old backup

        var ok = backend.Import(new MemoryStream(bytes));
        if (!ok) colors = null;
        return ok;
    }
}