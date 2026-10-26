namespace TodoListApp;

public static class ColorPickerDialog
{
    public static void Show(Context context, Color initial, Action<Color> onPicked)
    {
        var layout = new LinearLayout(context) { Orientation = Orientation.Vertical };
        layout.SetPadding(48, 24, 48, 0);
        var swatch = new View(context);
        layout.AddView(swatch, new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, 120));

        var rgb = new[] { (int)initial.R, initial.G, initial.B };
        void Update() => swatch.SetBackgroundColor(Color.Rgb(rgb[0], rgb[1], rgb[2]));
        for (var i = 0; i < 3; i++)
        {
            var channel = i;
            var bar = new SeekBar(context) { Max = 255, Progress = rgb[i] };
            bar.ProgressChanged += (_, e) => { rgb[channel] = e.Progress; Update(); };
            layout.AddView(bar);
        }
        Update();

        new AlertDialog.Builder(context)!
            .SetTitle("Pick color")!
            .SetView(layout)!
            .SetPositiveButton("OK", (_, _) => onPicked(Color.Rgb(rgb[0], rgb[1], rgb[2])))!
            .SetNegativeButton("Cancel", (_, _) => { })!
            .Show();
    }
}