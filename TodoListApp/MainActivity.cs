namespace TodoListApp;

[Activity(Label = "@string/app_name", MainLauncher = true, Theme = "@android:style/Theme.DeviceDefault.NoActionBar")]
public class MainActivity : Activity
{
    private ColorProfile _profile = null!;
    private AutoBackup _autoBackup = null!;
    private readonly List<TodoActivity> _shown = [];
    private IBackend _backend = null!;
    private ActivityAdapter _adapter = null!;
    private ActivityDialogs _dialogs = null!;
    private BackupManager _backup = null!;
    private SettingsStore _store = null!;
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        SetContentView(Resource.Layout.activity_main);

        _backend = new Backend(System.IO.Path.Combine(FilesDir!.AbsolutePath, "activities.json"));
        _backend.Load();

        _dialogs = new ActivityDialogs(this, _backend, Refresh);
        
        _store = new SettingsStore(this);
        _profile = _store.LoadProfile();
        _backup = new BackupManager(this, _backend, () => _profile, OnImported);
        _adapter = new ActivityAdapter(this, _shown, _profile.ToColorizer());
        _autoBackup = new AutoBackup(this, _backend, _store, () => _profile, OnImported);
        
        
        var options = new OptionsMenu(this,
        [
            ("Deadline colors", () => new PresetPickerDialog(this, _profile, OnProfileChanged).Show()),
            ("Backup", () => _backup.ShowDialog()),
            ("Auto-backup folder", () => _autoBackup.PickFolder())
        ]);

        var list = FindViewById<RecyclerView>(Resource.Id.activityList)!;
        list.SetLayoutManager(new LinearLayoutManager(this));
        list.SetAdapter(_adapter);
        new ItemTouchHelper(new SwipeCallback(
            position => { var a = _shown[position]; Refresh(); _dialogs.ShowDelete(a); },
            position => { var a = _shown[position]; Refresh(); _dialogs.ShowActivity(a); }
        )).AttachToRecyclerView(list);

        FindViewById<Button>(Resource.Id.addButton)!.Click += (_, _) => _dialogs.ShowActivity();
        FindViewById<Button>(Resource.Id.optionsButton)!.Click += (_, _) => options.Show();
        Refresh();
    }
    
    private void OnImported(ColorProfile? imported)
    {
        if (imported is not null)
        {
            _profile = imported;
            OnProfileChanged();
        }
        Refresh();
    }
    
    protected override void OnPause()
    {
        base.OnPause();
        _backend.Save();
        _autoBackup.Write();
    }

    protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
    {
        base.OnActivityResult(requestCode, resultCode, data);
        _backup.HandleResult(requestCode, resultCode, data);
        _autoBackup.HandleResult(requestCode, resultCode, data);
    }

    private void Refresh()
    {
        _shown.Clear();
        _shown.AddRange(_backend.GetActivityHandler().GetActivities().OrderBy(a => a.Deadline));
        _adapter.NotifyDataSetChanged();
    }
    
    private void OnProfileChanged()
    {
        _store.SaveProfile(_profile);
        _adapter.Colorizer = _profile.ToColorizer();
        _adapter.NotifyDataSetChanged();
    }
    
}