using System.Text.Json;
using System.Text.Json.Serialization;

namespace TodoListApp;

public class ColorProfile
{
    public const string CustomName = "Custom";

    public string Selected { get; set; } = "Classic";
    public DeadlineColorSettings Custom { get; set; } = new();
    public List<NamedStyle> Saved { get; set; } = [];

    [JsonIgnore]
    public DeadlineColorSettings Current => Resolve(Selected);

    public DeadlineColorSettings Resolve(string name) =>
        name == CustomName ? Custom
        : Saved.FirstOrDefault(s => s.Name == name)?.Style
          ?? BuiltInPresets.All.FirstOrDefault(p => p.Name == name)?.Style
          ?? BuiltInPresets.All[0].Style;

    public IDeadlineColorizer ToColorizer() => Current.ToColorizer();

    public bool SaveStyle(string name, DeadlineColorSettings style)
    {
        name = name.Trim();
        if (name.Length == 0 || name == CustomName || BuiltInPresets.All.Any(p => p.Name == name)) return false;
        Saved.RemoveAll(s => s.Name == name);
        Saved.Add(new NamedStyle(name, style.Clone()));
        Selected = name;
        return true;
    }

    public void Normalize()
    {
        Custom.Normalize();
        foreach (var s in Saved) s.Style.Normalize();
    }
}

public class SettingsStore(Context context)
{
    private ISharedPreferences Prefs => context.GetSharedPreferences("settings", FileCreationMode.Private)!;

    public string? AutoBackupFolder
    {
        get => Prefs.GetString("auto_backup_folder", null);
        set => Prefs.Edit()!.PutString("auto_backup_folder", value)!.Apply();
    }

    public ColorProfile LoadProfile()
    {
        try
        {
            if (Prefs.GetString("color_profile", null) is { } json)
            {
                var p = JsonSerializer.Deserialize<ColorProfile>(json) ?? new();
                p.Normalize();
                return p;
            }
            // your current saved colors become the Custom preset and stay selected
            if (Prefs.GetString("deadline_colors", null) is { } legacy
                && JsonSerializer.Deserialize<DeadlineColorSettings>(legacy) is { } old)
            {
                old.Normalize();
                return new ColorProfile { Selected = ColorProfile.CustomName, Custom = old };
            }
        }
        catch (JsonException) { }
        return new();
    }

    public void SaveProfile(ColorProfile p) =>
        Prefs.Edit()!.PutString("color_profile", JsonSerializer.Serialize(p))!.Apply();
}