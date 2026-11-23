using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace TodoListApp;

public static class BackupFormat
{
    private const string Marker = "backupVersion";

    public static bool Export(IBackend backend, ColorProfile profile, Stream output)
    {
        using var raw = new MemoryStream();
        if (!backend.Export(raw)) return false;

        var root = new JsonObject
        {
            [Marker] = 3,
            ["activities"] = JsonNode.Parse(raw.ToArray()),
            ["colorProfile"] = JsonSerializer.SerializeToNode(profile)
        };
        using var writer = new Utf8JsonWriter(output);
        root.WriteTo(writer);
        return true;
    }

    public static bool Import(IBackend backend, Stream input, ColorProfile current, out ColorProfile? profile)
    {
        profile = null;
        using var buffer = new MemoryStream();
        input.CopyTo(buffer);
        var bytes = buffer.ToArray();

        try
        {
            if (JsonNode.Parse(bytes) is JsonObject root && root.ContainsKey(Marker))
            {
                if (root["colorProfile"] is { } p)
                    profile = JsonSerializer.Deserialize<ColorProfile>(p);
                else if (root["deadlineColors"] is { } l
                         && JsonSerializer.Deserialize<DeadlineColorSettings>(l) is { } legacy)
                    profile = new ColorProfile
                        { Selected = ColorProfile.CustomName, Custom = legacy, Saved = current.Saved };
                profile?.Normalize();
                bytes = Encoding.UTF8.GetBytes(root["activities"]!.ToJsonString());
            }
        }
        catch (JsonException) { }

        var ok = backend.Import(new MemoryStream(bytes));
        if (!ok) profile = null;
        return ok;
    }
}