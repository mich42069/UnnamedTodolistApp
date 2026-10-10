using Android.Content;
using Android.Graphics;
using Android.Views;
using Backend = TodoListBackend.TodoListBackend;
using TodoActivity = TodoListBackend.Activity;

namespace TodoListApp;

[Activity(Label = "@string/app_name", MainLauncher = true, Theme = "@android:style/Theme.DeviceDefault")]
public class MainActivity : Activity
{
    private ListView _list = null!;
    private List<TodoActivity> _shown = [];
    private Backend _backend = null!;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        SetContentView(Resource.Layout.activity_main);

        _backend = new Backend(System.IO.Path.Combine(FilesDir!.AbsolutePath, "activities.json"));
        _backend.Load();
        _list = FindViewById<ListView>(Resource.Id.activityList)!;
        FindViewById<Button>(Resource.Id.addButton)!.Click += (_, _) => ShowCreateDialog();
        _list.ItemLongClick += (_, e) => ShowDeleteDialog(_shown[e.Position]);
        Refresh();
    }

    protected override void OnPause()
    {
        base.OnPause();
        _backend.Save();
    }
    
    private void Refresh()
    {
        _shown = _backend.GetActivities().OrderBy(a => a.Deadline).ToList();
        _list.Adapter = new ActivityAdapter(this, _shown);
    }

    private void ShowCreateDialog()
    {
        var deadline = DateTime.Now.AddDays(1);

        var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };
        layout.SetPadding(48, 24, 48, 0);
        var name = new EditText(this) { Hint = "Name" };
        var description = new EditText(this) { Hint = "Description (optional)" };
        var deadlineButton = new Button(this) { Text = deadline.ToString("g") };
        layout.AddView(name);
        layout.AddView(description);
        layout.AddView(deadlineButton);

        deadlineButton.Click += (_, _) =>
            new DatePickerDialog(this, (_, d) =>
                    new TimePickerDialog(this, (_, t) =>
                    {
                        deadline = d.Date.Add(new TimeSpan(t.HourOfDay, t.Minute, 0));
                        deadlineButton.Text = deadline.ToString("g");
                    }, deadline.Hour, deadline.Minute, true).Show(),
                deadline.Year, deadline.Month - 1, deadline.Day).Show();

        new AlertDialog.Builder(this)!
            .SetTitle("New activity")!
            .SetView(layout)!
            .SetPositiveButton("Create", (_, _) =>
            {
                if (string.IsNullOrWhiteSpace(name.Text)) return;
                var desc = string.IsNullOrWhiteSpace(description.Text) ? "no-description" : description.Text;
                _backend.CreateActivity(new TodoActivity(name.Text, deadline, desc));
                Refresh();
            })!
            .SetNegativeButton("Cancel", (_, _) => { })!
            .Show();
    }

    private void ShowDeleteDialog(TodoActivity activity)
    {
        new AlertDialog.Builder(this)!
            .SetTitle($"Delete \"{activity.Name}\"?")!
            .SetPositiveButton("Delete", (_, _) =>
            {
                _backend.DeleteActivity(activity.Id);
                Refresh();
            })!
            .SetNegativeButton("Cancel", (_, _) => { })!
            .Show();
    }
    
    private class ActivityAdapter(Context context, List<TodoActivity> items) : BaseAdapter
    {
        public override int Count => items.Count;
        public override Java.Lang.Object? GetItem(int position) => null;
        public override long GetItemId(int position) => position;

        public override View GetView(int position, View? convertView, ViewGroup? parent)
        {
            var view = convertView
                       ?? LayoutInflater.From(context)!.Inflate(Resource.Layout.item_activity, parent, false)!;
            var a = items[position];

            view.FindViewById<TextView>(Resource.Id.name)!.Text = a.Name;

            var deadline = view.FindViewById<TextView>(Resource.Id.deadline)!;
            deadline.Text = a.Deadline.ToString("g");
            deadline.SetTextColor(Color.ParseColor(a.Deadline < DateTime.Now ? "#CF6679" : "#03DAC5"));

            var description = view.FindViewById<TextView>(Resource.Id.description)!;
            description.Text = a.Description;
            description.Visibility = a.Description is null or "no-description" ? ViewStates.Gone : ViewStates.Visible;

            return view;
        }
    }
}