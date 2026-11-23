using Android.OS;

namespace TodoListApp;

[Activity(Label = "@string/app_name", MainLauncher = true, Theme = "@android:style/Theme.DeviceDefault.NoActionBar")]
public class MainActivity : Activity
{
	private ListDialogs _listDialogs = null!;
	private Button _listButton = null!;
	private ColorProfile _profile = null!;
	private AutoBackup _autoBackup = null!;
	private readonly List<TodoActivity> _shown = [];
	private IBackend _backend = null!;
	private ActivityAdapter _adapter = null!;
	private ActivityDialogs _dialogs = null!;
	private BackupManager _backup = null!;
	private SettingsStore _store = null!;
	private ITodoListHandler _handler = null!;
	private bool _showDone;

	protected override void OnCreate(Bundle? savedInstanceState)
	{
		base.OnCreate(savedInstanceState);
		SetContentView(Resource.Layout.activity_main);
		_listButton = FindViewById<Button>(Resource.Id.listButton)!;

		_store = new SettingsStore(this);
		_handler = new TodoListHandler(System.IO.Path.Combine(FilesDir!.AbsolutePath, "activities.json"));
		_handler.Load();
		_handler.EnsureColorProfiles(_store.LoadProfile());
		_handler.CurrentChanged += _ => OnListChanged();

		_listDialogs = new ListDialogs(this, _handler, OnListChanged);
		_listButton.Click += (_, _) => _listDialogs.ShowSwitcher();

		UseList();
		_adapter = new ActivityAdapter(this, _shown, _profile.ToColorizer());
		_backup = new BackupManager(this, _handler, OnImported);
		_autoBackup = new AutoBackup(this, _handler, _store, OnImported);


		var options = BuildOptions();

		var list = FindViewById<RecyclerView>(Resource.Id.activityList)!;
		list.SetLayoutManager(new LinearLayoutManager(this));
		list.SetAdapter(_adapter);
		new ItemTouchHelper(new SwipeCallback(
			position =>
			{
				var a = _shown[position];
				if (_showDone)
				{
					Refresh();
					_dialogs.ShowDelete(a);
				}
				else SetDone(a, true);
			},
			position =>
			{
				var a = _shown[position];
				if (_showDone) SetDone(a, false);
				else
				{
					Refresh();
					_dialogs.ShowActivity(a);
				}
			},
			() => _showDone
		)).AttachToRecyclerView(list);

		FindViewById<Button>(Resource.Id.addButton)!.Click += (_, _) => _dialogs.ShowActivity();
		FindViewById<Button>(Resource.Id.optionsButton)!.Click += (_, _) => BuildOptions().Show();
		Refresh();
		if (Build.VERSION.SdkInt >= (BuildVersionCodes)33
			&& CheckSelfPermission("android.permission.POST_NOTIFICATIONS") != Android.Content.PM.Permission.Granted)
			RequestPermissions(["android.permission.POST_NOTIFICATIONS"], 10);
	}

	private OptionsMenu BuildOptions() => new(this,
	[
		(_showDone ? "Show open activities" : "Show completed", ToggleDone),
		("Start on list", () => _listDialogs.ShowStartOn()),
		("Reminder times", () => new ReminderDialog(this, _store, _handler).Show()),
		("Auto-backup folder", () => _autoBackup.PickFolder()),
		("Backup", () => _backup.ShowDialog())
	]);

	private void SetDone(TodoActivity a, bool done)
	{
		_backend.GetActivityHandler().SetCompleted(a.Id, done ? DateTime.Now : null);
		Toast.MakeText(this, done ? "Moved to completed" : "Restored", ToastLength.Short)!.Show();
		Refresh();
	}

	private void ToggleDone()
	{
		_showDone = !_showDone;
		FindViewById<Button>(Resource.Id.addButton)!.Visibility = _showDone ? ViewStates.Gone : ViewStates.Visible;
		UpdateListButton();
		Refresh();
	}

	private void UpdateListButton() =>
		_listButton.Text = $"{_backend.Name} ▾{(_showDone ? " · completed" : "")}";


	private void UseList()
	{
		_backend = _handler.Current();
		_profile = _backend.GetColorProfile();
		_dialogs = new ActivityDialogs(this, _backend, Refresh);
		UpdateListButton();
	}

	private void OnListChanged()
	{
		UseList();
		_adapter.Colorizer = _profile.ToColorizer();
		Refresh();
	}

	private void OnImported()
	{
		_handler.EnsureColorProfiles(_profile);
		OnListChanged();
	}

	protected override void OnPause()
	{
		base.OnPause();
		_handler.Save();
		ReminderService.Process(this, _handler);
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
		var activities = _backend.GetActivityHandler().GetActivities();
		_shown.Clear();
		_shown.AddRange(_showDone
			? activities.Where(a => a.CompletedAt is not null).OrderByDescending(a => a.CompletedAt)
			: activities.Where(a => a.CompletedAt is null).OrderBy(a => a.Deadline));
		_adapter.NotifyDataSetChanged();
	}
}