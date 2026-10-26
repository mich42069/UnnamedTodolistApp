namespace TodoListApp;

public class ActivityAdapter(Context context, List<TodoActivity> items, IDeadlineColorizer colorizer)
    : RecyclerView.Adapter
{
    
    public override int ItemCount => items.Count;

    public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType) =>
        new Holder(LayoutInflater.From(context)!.Inflate(Resource.Layout.item_activity, parent, false)!);

    public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
    {
        var view = holder.ItemView;
        var a = items[position];

        view.FindViewById<TextView>(Resource.Id.name)!.Text = a.Name;
        view.FindViewById<TextView>(Resource.Id.type)!.Text = a.Type.Name;

        var deadline = view.FindViewById<TextView>(Resource.Id.deadline)!;
        deadline.Text = a.Deadline.ToString("g");
        deadline.SetTextColor(Color.White);
        ((GradientDrawable)view.Background!.Mutate()!).SetColor(colorizer.GetColor(a.Deadline));
        var description = view.FindViewById<TextView>(Resource.Id.description)!;
        description.Text = a.Description;
        description.Visibility = a.Description is null or "no-description" ? ViewStates.Gone : ViewStates.Visible;
    }
    private class Holder(View view) : RecyclerView.ViewHolder(view) { }
}
// ...