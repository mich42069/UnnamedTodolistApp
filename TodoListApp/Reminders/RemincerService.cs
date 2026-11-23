using TodoListBackend;

[assembly: UsesPermission("android.permission.RECEIVE_BOOT_COMPLETED")]
[assembly: UsesPermission("android.permission.POST_NOTIFICATIONS")]

namespace TodoListApp;

public static class ReminderService
{
    private const string ChannelId = "reminders";

    public static void Process(Context context, ITodoListHandler handler)
    {
        var store = new SettingsStore(context);
        var now = DateTime.Now;
        var all = ReminderPlanner.Plan(handler, store.LoadReminderSettings());

        foreach (var r in ReminderPlanner.Due(all, store.LastReminderCheck ?? now, now)) Notify(context, r);
        store.LastReminderCheck = now;
        Schedule(context, ReminderPlanner.Next(all, now));
    }

    private static void Schedule(Context context, DateTime? next)
    {
        var alarms = (AlarmManager)context.GetSystemService(Context.AlarmService)!;
        var pending = PendingIntent.GetBroadcast(context, 0, new Intent(context, typeof(ReminderReceiver)),
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable)!;
        if (next is null) alarms.Cancel(pending);
        else alarms.SetAndAllowWhileIdle(AlarmType.RtcWakeup,
            new DateTimeOffset(next.Value).ToUnixTimeMilliseconds(), pending);
    }

    private static void Notify(Context context, Reminder r)
    {
        var manager = (NotificationManager)context.GetSystemService(Context.NotificationService)!;
        manager.CreateNotificationChannel(
            new NotificationChannel(ChannelId, "Deadline reminders", NotificationImportance.Default));

        var open = PendingIntent.GetActivity(context, 0, new Intent(context, typeof(MainActivity)),
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
        var due = r.Deadline.TimeOfDay == TimeSpan.Zero ? $"{r.Deadline:ddd d MMM}" : $"{r.Deadline:ddd d MMM, HH:mm}";

        var notification = new Notification.Builder(context, ChannelId)
            .SetSmallIcon(Android.Resource.Drawable.IcDialogInfo)!
            .SetContentTitle(r.ActivityName)!
            .SetContentText($"Due {due} · {r.ListName}")!
            .SetContentIntent(open)!
            .SetAutoCancel(true)!
            .Build()!;
        manager.Notify(HashCode.Combine(r.ActivityId, r.FireAt), notification);
    }
}

[BroadcastReceiver(Enabled = true, Exported = true)]
[IntentFilter(new[] { Intent.ActionBootCompleted, Intent.ActionMyPackageReplaced })]
public class ReminderReceiver : BroadcastReceiver
{
    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context is null) return;
        var handler = new TodoListHandler(System.IO.Path.Combine(context.FilesDir!.AbsolutePath, "activities.json"));
        handler.Load();
        ReminderService.Process(context, handler);
    }
}