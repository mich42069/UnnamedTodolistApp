using System.Text.Json;
using TodoListBackend;

namespace TodoListApp;

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
		catch (JsonException)
		{
		}

		return new();
	}

	public ReminderSettings LoadReminderSettings()
	{
		try
		{
			if (Prefs.GetString("reminder_settings", null) is { } json)
				return JsonSerializer.Deserialize<ReminderSettings>(json) ?? new();
		}
		catch (JsonException)
		{
		}

		return new();
	}

	public void SaveReminderSettings(ReminderSettings s) =>
		Prefs.Edit()!.PutString("reminder_settings", JsonSerializer.Serialize(s))!.Apply();

	public DateTime? LastReminderCheck
	{
		get
		{
			var t = Prefs.GetLong("reminder_check", 0);
			return t == 0 ? null : new DateTime(t);
		}
		set => Prefs.Edit()!.PutLong("reminder_check", value?.Ticks ?? 0)!.Apply();
	}
}