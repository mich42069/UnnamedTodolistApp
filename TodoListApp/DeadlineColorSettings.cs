using System.Text.Json;

namespace TodoListApp;

public class DeadlineColorSettings
{
    public int Days { get; set; } = 14;
    public List<int> Colors { get; set; } = [Color.ForestGreen.ToArgb(), Color.DarkRed.ToArgb()];
    public int Steps { get; set; } // 0 = smooth
    public int MaxSteps => 3 * Days;

    public void Normalize()
    {
        Days = Math.Max(Days, (Colors.Count + 2) / 3); // keeps Colors.Count <= MaxSteps
        if (Steps > 0) Steps = Math.Clamp(Steps, Colors.Count, MaxSteps);
    }

    public DeadlineColorSettings Clone() => new() { Days = Days, Colors = [..Colors], Steps = Steps };

    public IDeadlineColorizer ToColorizer() => new GradientDeadlineColorizer(
        Colors.Select(c => new Color(c)).ToList(), TimeSpan.FromDays(Days), new LinearCurve(), Steps);
}

public class SettingsStore(Context context)
{
    private ISharedPreferences Prefs => context.GetSharedPreferences("settings", FileCreationMode.Private)!;

    public DeadlineColorSettings Load()
    {
        try
        {
            var s = JsonSerializer.Deserialize<DeadlineColorSettings>(Prefs.GetString("deadline_colors", "")!);
            s?.Normalize();
            return s ?? new();
        }
        catch { return new(); }
    }

    public void Save(DeadlineColorSettings s) =>
        Prefs.Edit()!.PutString("deadline_colors", JsonSerializer.Serialize(s))!.Apply();
}