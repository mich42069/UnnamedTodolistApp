namespace TodoListApp;

public class ActivityDialogs(Context context, IBackend backend, Action onChanged)
{
    private DateTime? _lastDeadline;

    public void ShowActivity(TodoActivity? existing = null)
    {
        var deadline = existing?.Deadline ?? _lastDeadline ?? DateTime.Now.AddDays(1);
        TodoActivityType? selectedType = existing?.Type;

        var layout = new LinearLayout(context) { Orientation = Orientation.Vertical };
        layout.SetPadding(48, 24, 48, 0);
        var name = new EditText(context)
        {
            Hint = "Name"
        };
        var description = new EditText(context)
        {
            Hint = "Description"
        };
        var typeButton = new Button(context)
        {
            Text = selectedType?.Name ?? "Choose type"
        };
        var deadlineButton = new Button(context)
        {
            Text = deadline.ToString("g")
        };

        if (existing is { } e)
        {
            name.Text = e.Name;
            if (e.Description is not (null or "no-description")) description.Text = e.Description;
        }

        layout.AddView(name);
        layout.AddView(description);
        layout.AddView(typeButton);
        layout.AddView(deadlineButton);

        typeButton.Click += (_, _) => ShowTypePicker(t =>
        {
            selectedType = t;
            typeButton.Text = t.Name;
        });

        deadlineButton.Click += (_, _) =>
            new DatePickerDialog(context, (_, d) =>
                    new TimePickerDialog(context, (_, t) =>
                    {
                        deadline = d.Date.Add(new TimeSpan(t.HourOfDay, t.Minute, 0));
                        deadlineButton.Text = deadline.ToString("g");
                    }, deadline.Hour, deadline.Minute, true).Show(),
                deadline.Year, deadline.Month - 1, deadline.Day).Show();

        new AlertDialog.Builder(context)!
            .SetTitle(existing is null ? "New activity" : "Edit activity")!
            .SetView(layout)!
            .SetPositiveButton(existing is null ? "Create" : "Save", (_, _) =>
            {
                if (string.IsNullOrWhiteSpace(name.Text) || selectedType is null
                                                         || !backend.GetActivityTypeHandler().GetActivityTypes()
                                                             .Any(t => t.Id == selectedType.Value.Id))
                {
                    Toast.MakeText(context, "Name and type are required", ToastLength.Short)!.Show();
                    return;
                }

                var desc = string.IsNullOrWhiteSpace(description.Text) ? "no-description" : description.Text;
                var handler = backend.GetActivityHandler();

                if (existing is { } old)
                {
                    var edited = old;
                    edited.Name = name.Text;
                    edited.Type = selectedType.Value;
                    edited.Description = desc;
                    edited.Deadline = deadline;
                    handler.DeleteActivity(old.Id);
                    handler.TryAddActivity(old.Id, edited);
                }
                else
                {
                    handler.CreateActivity(new TodoActivity(name.Text, deadline, selectedType.Value, desc));
                    _lastDeadline = deadline;
                }

                onChanged();
            })!
            .SetNegativeButton("Cancel", (_, _) => { })!
            .Show();
    }

    public void ShowDelete(TodoActivity activity)
    {
        new AlertDialog.Builder(context)!
            .SetTitle($"Delete \"{activity.Name}\"?")!
            .SetPositiveButton("Delete", (_, _) =>
            {
                backend.GetActivityHandler().DeleteActivity(activity.Id);
                onChanged();
            })!
            .SetNegativeButton("Cancel", (_, _) => { })!
            .Show();
    }

    private void ShowTypePicker(Action<TodoActivityType> onPicked)
    {
        var handler = backend.GetActivityTypeHandler();
        var types = handler.GetActivityTypes().ToList();
        var items = types.Select(t => t.Name).Append("+ Add new type").ToArray();

        var dialog = new AlertDialog.Builder(context)!
            .SetTitle("Type")!
            .SetItems(items, (_, e) =>
            {
                if (e.Which < types.Count) onPicked(types[e.Which]);
                else ShowAddTypeDialog(onPicked);
            })!
            .Show()!;

        dialog.ListView!.ItemLongClick += (_, e) =>
        {
            if (e.Position >= types.Count) return;
            var type = types[e.Position];

            if (backend.GetActivityHandler().GetActivities().Any(a => a.Type.Id == type.Id))
            {
                Toast.MakeText(context, "Type is used by existing activities", ToastLength.Short)!.Show();
                return;
            }

            new AlertDialog.Builder(context)!
                .SetTitle($"Delete \"{type.Name}\"?")!
                .SetPositiveButton("Delete", (_, _) =>
                {
                    handler.RemoveActivityType(type.Id);
                    dialog.Dismiss();
                    ShowTypePicker(onPicked);
                })!
                .SetNegativeButton("Cancel", (_, _) => { })!
                .Show();
        };
    }

    private void ShowAddTypeDialog(Action<TodoActivityType> onPicked)
    {
        var input = new EditText(context) { Hint = "Type name" };
        new AlertDialog.Builder(context)!
            .SetTitle("New type")!
            .SetView(input)!
            .SetPositiveButton("Add", (_, _) =>
            {
                if (string.IsNullOrWhiteSpace(input.Text)) return;
                var handler = backend.GetActivityTypeHandler();
                if (handler.AddActivityType(input.Text))
                    onPicked(handler.GetActivityTypes().First(t => t.Name == input.Text));
                else
                    ShowTypePicker(onPicked);
            })!
            .SetNegativeButton("Cancel", (_, _) => ShowTypePicker(onPicked))!
            .Show();
    }
}