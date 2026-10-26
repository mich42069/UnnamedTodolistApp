using ColorStateList = Android.Content.Res.ColorStateList;

namespace TodoListApp;

public static class ColorPickerDialog
{
    private static readonly Color[] Tints = [Color.Red, Color.Green, Color.Blue];

    public static void Show(Context context, Color initial, Action<Color> onPicked)
    {
        var layout = new LinearLayout(context) { Orientation = Orientation.Vertical };
        layout.SetPadding(48, 24, 48, 0);
        var swatch = new View(context);
        var name = new TextView(context);
        layout.AddView(swatch, new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, 120));
        layout.AddView(name);

        var rgb = new[] { (int)initial.R, initial.G, initial.B };
        var bars = new SeekBar[3];
        Color Current() => Color.Rgb(rgb[0], rgb[1], rgb[2]);
        void Update()
        {
            swatch.SetBackgroundColor(Current());
            name.Text = NamedColors.NameOf(Current()) ?? $"#{Current().ToArgb() & 0xFFFFFF:X6}";
        }

        for (var i = 0; i < 3; i++)
        {
            var channel = i;
            bars[i] = new SeekBar(context) { Max = 255, Progress = rgb[i] };
            bars[i].ProgressTintList = ColorStateList.ValueOf(Tints[i]);
            bars[i].ThumbTintList = ColorStateList.ValueOf(Tints[i]);
            bars[i].ProgressChanged += (_, e) => { rgb[channel] = e.Progress; Update(); };
            layout.AddView(bars[i]);
        }

        var named = new Button(context) { Text = "Named colors" };
        named.Click += (_, _) => new AlertDialog.Builder(context)!
            .SetTitle("Color names")!
            .SetItems(NamedColors.All.Select(n => n.Name).ToArray(), (_, e) =>
            {
                var c = NamedColors.All[e.Which].Value;
                rgb[0] = c.R; rgb[1] = c.G; rgb[2] = c.B;
                for (var i = 0; i < 3; i++) bars[i].Progress = rgb[i];
                Update();
            })!
            .Show();
        layout.AddView(named);
        Update();

        new AlertDialog.Builder(context)!
            .SetTitle("Pick color")!
            .SetView(layout)!
            .SetPositiveButton("OK", (_, _) => onPicked(Current()))!
            .SetNegativeButton("Cancel", (_, _) => { })!
            .Show();
    }
}