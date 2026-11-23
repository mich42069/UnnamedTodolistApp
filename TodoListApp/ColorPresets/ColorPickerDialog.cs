using System.Globalization;
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
        var hex = new EditText(context) { Hint = "#RRGGBB" };
        hex.SetSingleLine();
        layout.AddView(swatch, new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, 120));
        layout.AddView(name);
        layout.AddView(hex);

        var rgb = new[] { (int)initial.R, initial.G, initial.B };
        var bars = new SeekBar[3];
        Color Current() => Color.Rgb(rgb[0], rgb[1], rgb[2]);

        void Apply(bool updateHex)
        {
            for (var i = 0; i < 3; i++) bars[i].Progress = rgb[i];
            swatch.SetBackgroundColor(Current());
            name.Text = NamedColors.NameOf(Current()) ?? "";
            if (updateHex) hex.Text = $"#{Current().ToArgb() & 0xFFFFFF:X6}";
        }

        for (var i = 0; i < 3; i++)
        {
            var channel = i;
            bars[i] = new SeekBar(context) { Max = 255 };
            bars[i].ProgressTintList = ColorStateList.ValueOf(Tints[i]);
            bars[i].ThumbTintList = ColorStateList.ValueOf(Tints[i]);
            bars[i].ProgressChanged += (_, e) =>
            {
                if (!e.FromUser) return;
                rgb[channel] = e.Progress;
                Apply(true);
            };
            layout.AddView(bars[i]);
        }

        hex.TextChanged += (_, _) =>
        {
            if (!hex.HasFocus || !TryParseHex(hex.Text, out var c)) return;
            rgb[0] = c.R; rgb[1] = c.G; rgb[2] = c.B;
            Apply(false);
        };

        var named = new Button(context) { Text = "Named colors" };
        named.Click += (_, _) => new AlertDialog.Builder(context)!
            .SetTitle("Color names")!
            .SetItems(NamedColors.All.Select(n => n.Name).ToArray(), (_, e) =>
            {
                var c = NamedColors.All[e.Which].Value;
                rgb[0] = c.R; rgb[1] = c.G; rgb[2] = c.B;
                Apply(true);
            })!
            .Show();
        layout.AddView(named);
        Apply(true);

        new AlertDialog.Builder(context)!
            .SetTitle("Pick color")!
            .SetView(layout)!
            .SetPositiveButton("OK", (_, _) => onPicked(Current()))!
            .SetNegativeButton("Cancel", (_, _) => { })!
            .Show();
    }

    private static bool TryParseHex(string? text, out Color color)
    {
        color = default;
        var h = (text ?? "").Trim().TrimStart('#');
        if (h.Length == 3) h = string.Concat(h.Select(c => $"{c}{c}"));
        if (h.Length != 6 || !int.TryParse(h, NumberStyles.HexNumber, null, out var v)) return false;
        color = Color.Rgb((v >> 16) & 0xFF, (v >> 8) & 0xFF, v & 0xFF);
        return true;
    }
}