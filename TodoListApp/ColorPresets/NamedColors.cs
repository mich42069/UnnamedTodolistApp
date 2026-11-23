namespace TodoListApp;

public static class NamedColors
{
    public static readonly IReadOnlyList<(string Name, Color Value)> All =
        Enum.GetValues<System.Drawing.KnownColor>()
            .Select(System.Drawing.Color.FromKnownColor)
            .Where(c => !c.IsSystemColor && c.A == 255)
            .Select(c => (Name: c.Name, Value: Color.Rgb(c.R, c.G, c.B)))
            .OrderBy(c => c.Name)
            .ToList();

    public static string? NameOf(Color color) =>
        All.Where(n => n.Value.ToArgb() == color.ToArgb()).Select(n => n.Name).FirstOrDefault();
}