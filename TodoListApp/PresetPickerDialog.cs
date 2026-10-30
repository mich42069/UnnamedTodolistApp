namespace TodoListApp;

public record PresetEntry(string Name, DeadlineColorSettings Style, bool IsSaved);

public class PresetPickerDialog(Context context, ColorProfile profile, Action onChanged)
{
    public void Show()
    {
        var entries = new List<PresetEntry> { new(ColorProfile.CustomName, profile.Custom, false) };
        entries.AddRange(profile.Saved.Select(s => new PresetEntry(s.Name, s.Style, true)));
        entries.AddRange(BuiltInPresets.All.Select(p => new PresetEntry(p.Name, p.Style, false)));

        var list = new ListView(context) { Adapter = new PresetAdapter(context, entries, profile.Selected) };
        var dialog = new AlertDialog.Builder(context)!
            .SetTitle("Deadline colors")!
            .SetView(list)!
            .SetNegativeButton("Close", (_, _) => { })!
            .Show()!;

        list.ItemClick += (_, e) =>
        {
            profile.Selected = entries[e.Position].Name;
            onChanged();
            dialog.Dismiss();
        };
        list.ItemLongClick += (_, e) => ShowActions(entries[e.Position], dialog);
    }

    private void ShowActions(PresetEntry entry, AlertDialog parent)
    {
        var actions = new List<(string Title, Action Run)>
        {
            ("Edit as custom", () =>
            {
                parent.Dismiss();
                new ColorCurveDialog(context, profile, entry.Style, onChanged).Show();
            })
        };
        if (entry.IsSaved)
            actions.Add(("Delete", () =>
            {
                profile.Saved.RemoveAll(s => s.Name == entry.Name);
                if (profile.Selected == entry.Name) profile.Selected = BuiltInPresets.All[0].Name;
                onChanged();
                parent.Dismiss();
                Show();
            }));

        new AlertDialog.Builder(context)!
            .SetTitle(entry.Name)!
            .SetItems(actions.Select(a => a.Title).ToArray(), (_, e) => actions[e.Which].Run())!
            .Show();
    }
}

public class PresetAdapter(Context context, IReadOnlyList<PresetEntry> entries, string selected) : BaseAdapter
{
    public override int Count => entries.Count;
    public override Java.Lang.Object? GetItem(int position) => null;
    public override long GetItemId(int position) => position;

    public override View GetView(int position, View? convertView, ViewGroup? parent)
    {
        var entry = entries[position];
        var row = new LinearLayout(context) { Orientation = Orientation.Vertical };
        row.SetPadding(48, 16, 48, 16);
        row.AddView(new TextView(context) { Text = entry.Name == selected ? $"✓ {entry.Name}" : entry.Name });
        row.AddView(new CurvePreview(context) { Colorizer = entry.Style.ToColorizer() },
            new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, 48));
        return row;
    }
}