namespace TodoListApp;

public class OptionsMenu(Context context, IReadOnlyList<(string Title, Action Run)> entries)
{
    public void Show() => new AlertDialog.Builder(context)!
        .SetTitle("Options")!
        .SetItems(entries.Select(e => e.Title).ToArray(), (_, e) => entries[e.Which].Run())!
        .Show();
}