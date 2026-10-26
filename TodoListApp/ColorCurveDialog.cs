namespace TodoListApp;

public class ColorCurveDialog(Context context, DeadlineColorSettings current, Action<DeadlineColorSettings> onSaved)
{
    private readonly DeadlineColorSettings _s = current.Clone();
    private CurvePreview _preview = null!;
    private TextView _farLabel = null!, _daysLabel = null!, _stepsLabel = null!;
    private SeekBar _days = null!, _steps = null!;
    private CheckBox _limit = null!;
    private LinearLayout _colorRow = null!;

    public void Show()
    {
        var layout = new LinearLayout(context) { Orientation = Orientation.Vertical };
        layout.SetPadding(48, 24, 48, 0);

        _preview = new CurvePreview(context);
        layout.AddView(_preview, new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, 120));

        _farLabel = new TextView(context);
        var nearLabel = new TextView(context) { Text = "Deadline", Gravity = GravityFlags.End };
        var ends = new LinearLayout(context);
        ends.AddView(_farLabel, new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1f));
        ends.AddView(nearLabel, new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WrapContent, 1f));
        layout.AddView(ends);

        _daysLabel = new TextView(context);
        _days = new SeekBar(context) { Max = 59 }; // 1..60 days
        _days.ProgressChanged += (_, e) =>
        {
            if (!e.FromUser) return;
            _s.Days = e.Progress + 1;
            Refresh();
        };

        _colorRow = new LinearLayout(context);
        var scroll = new HorizontalScrollView(context);
        scroll.AddView(_colorRow);

        _limit = new CheckBox(context) { Text = "Limit to a few colors" };
        _limit.Click += (_, _) =>
        {
            _s.Steps = _limit.Checked ? _s.Colors.Count : 0;
            Refresh();
        };
        _stepsLabel = new TextView(context);
        _steps = new SeekBar(context);
        _steps.ProgressChanged += (_, e) =>
        {
            if (!e.FromUser) return;
            _s.Steps = e.Progress + _s.Colors.Count;
            Refresh();
        };

        layout.AddView(_daysLabel);
        layout.AddView(_days);
        layout.AddView(new TextView(context) { Text = "Key colors (tap to edit, hold to remove)" });
        layout.AddView(scroll);
        layout.AddView(_limit);
        layout.AddView(_stepsLabel);
        layout.AddView(_steps);
        Refresh();

        new AlertDialog.Builder(context)!
            .SetTitle("Deadline colors")!
            .SetView(layout)!
            .SetPositiveButton("Save", (_, _) => onSaved(_s))!
            .SetNegativeButton("Cancel", (_, _) => { })!
            .Show();
    }

    private void Refresh()
    {
        _s.Normalize();
        _farLabel.Text = $"{_s.Days} days left";
        _daysLabel.Text = $"Time span: {_s.Days} days";
        _days.Progress = _s.Days - 1;

        _limit.Checked = _s.Steps > 0;
        _steps.Enabled = _s.Steps > 0;
        _steps.Max = _s.MaxSteps - _s.Colors.Count;
        _steps.Progress = Math.Max(_s.Steps - _s.Colors.Count, 0);
        _stepsLabel.Text = _s.Steps > 0
            ? $"Colors: {_s.Steps} (min {_s.Colors.Count}, max {_s.MaxSteps})"
            : "Smooth";

        _preview.Colorizer = _s.ToColorizer();
        _preview.Invalidate();
        RebuildColorRow();
    }

    private void RebuildColorRow()
    {
        _colorRow.RemoveAllViews();
        for (var i = 0; i < _s.Colors.Count; i++)
        {
            var index = i;
            var button = new Button(context);
            button.SetBackgroundColor(new Color(_s.Colors[i]));
            button.Click += (_, _) => ColorPickerDialog.Show(context, new Color(_s.Colors[index]), c =>
            {
                _s.Colors[index] = c.ToArgb();
                Refresh();
            });
            button.LongClick += (_, _) =>
            {
                if (_s.Colors.Count <= 2) return;
                _s.Colors.RemoveAt(index);
                Refresh();
            };
            _colorRow.AddView(button, new LinearLayout.LayoutParams(120, 120) { RightMargin = 12 });
        }

        var add = new Button(context) { Text = "+" };
        add.Click += (_, _) =>
        {
            _s.Colors.Add(_s.Colors[^1]);
            Refresh();
        };
        _colorRow.AddView(add, new LinearLayout.LayoutParams(120, 120));
    }
}