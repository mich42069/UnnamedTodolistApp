namespace TodoListApp;


class SwipeCallback(Action<int> onSwipeLeft, Action<int> onSwipeRight)
    : ItemTouchHelper.SimpleCallback(0, ItemTouchHelper.Left | ItemTouchHelper.Right)
{
    public override bool OnMove(RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder,
        RecyclerView.ViewHolder target) => false;

    public override void OnSwiped(RecyclerView.ViewHolder viewHolder, int direction)
    {
        var position = viewHolder.BindingAdapterPosition;
        if (position < 0) return;
        if (direction == ItemTouchHelper.Left) onSwipeLeft(position);
        else onSwipeRight(position);
    }

    public override void OnChildDraw(Canvas c, RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder,
        float dX, float dY, int actionState, bool isCurrentlyActive)
    {
        var item = viewHolder.ItemView;
        if (dX != 0)
        {
            var background = new ColorDrawable(dX < 0 ? Color.DarkRed : Color.SteelBlue);
            if (dX < 0) background.SetBounds(item.Right + (int)dX, item.Top, item.Right, item.Bottom);
            else background.SetBounds(item.Left, item.Top, item.Left + (int)dX, item.Bottom);
            background.Draw(c);
        }
        base.OnChildDraw(c, recyclerView, viewHolder, dX, dY, actionState, isCurrentlyActive);
    }
}