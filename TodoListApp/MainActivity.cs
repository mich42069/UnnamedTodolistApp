namespace TodoListApp;

[Activity(Label = "@string/app_name", MainLauncher = true, Theme = "@android:style/Theme.DeviceDefault.NoActionBar")]
public class MainActivity : Activity
{
    private readonly List<TodoActivity> _shown = [];
    private IBackend _backend = null!;
    private ActivityAdapter _adapter = null!;
    private ActivityDialogs _dialogs = null!;
    private BackupManager _backup = null!;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        SetContentView(Resource.Layout.activity_main);

        _backend = new Backend(System.IO.Path.Combine(FilesDir!.AbsolutePath, "activities.json"));
        _backend.Load();

        _dialogs = new ActivityDialogs(this, _backend, Refresh);
        _backup = new BackupManager(this, _backend, Refresh);

        var colorizer = new GradientDeadlineColorizer(
            Color.ForestGreen, Color.DarkRed, TimeSpan.FromDays(14), new LinearCurve());
        _adapter = new ActivityAdapter(this, _shown, colorizer);

        var list = FindViewById<RecyclerView>(Resource.Id.activityList)!;
        list.SetLayoutManager(new LinearLayoutManager(this));
        list.SetAdapter(_adapter);
        new ItemTouchHelper(new SwipeCallback(
            position => { var a = _shown[position]; Refresh(); _dialogs.ShowDelete(a); },
            position => { var a = _shown[position]; Refresh(); _dialogs.ShowActivity(a); }
        )).AttachToRecyclerView(list);

        FindViewById<Button>(Resource.Id.addButton)!.Click += (_, _) => _dialogs.ShowActivity();
        FindViewById<Button>(Resource.Id.backupButton)!.Click += (_, _) => _backup.ShowDialog();
        Refresh();
    }

    protected override void OnPause()
    {
        base.OnPause();
        _backend.Save();
    }

    protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
    {
        base.OnActivityResult(requestCode, resultCode, data);
        _backup.HandleResult(requestCode, resultCode, data);
    }

    private void Refresh()
    {
        _shown.Clear();
        _shown.AddRange(_backend.GetActivityHandler().GetActivities().OrderBy(a => a.Deadline));
        _adapter.NotifyDataSetChanged();
    }
}