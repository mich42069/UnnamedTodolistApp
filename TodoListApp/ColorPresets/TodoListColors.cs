using System.Text.Json;
using TodoListBackend;

namespace TodoListApp;

public static class TodoListColors
{
	private const string Key = "colorProfile", LegacyKey = "deadlineColors";

	public static bool HasColorProfile(this ITodoList list) =>
		list.Settings.ContainsKey(Key) || list.Settings.ContainsKey(LegacyKey);

	public static ColorProfile GetColorProfile(this ITodoList list)
	{
		try
		{
			if (list.Settings[Key] is { } p && JsonSerializer.Deserialize<ColorProfile>(p) is { } profile)
			{
				profile.Normalize();
				return profile;
			}
			if (list.Settings[LegacyKey] is { } d && JsonSerializer.Deserialize<DeadlineColorSettings>(d) is { } old)
			{
				old.Normalize();
				return new ColorProfile { Selected = ColorProfile.CustomName, Custom = old };
			}
		}
		catch (JsonException) { }
		return new();
	}

	public static void SetColorProfile(this ITodoList list, ColorProfile profile) =>
		list.Settings[Key] = JsonSerializer.SerializeToNode(profile);

	public static void EnsureColorProfiles(this ITodoListHandler handler, ColorProfile fallback)
	{
		foreach (var l in handler.GetAll().Where(l => !l.HasColorProfile())) l.SetColorProfile(fallback);
	}
}