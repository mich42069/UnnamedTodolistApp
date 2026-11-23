namespace TodoListApp;

public class CurvePreview(Context context) : View(context)
{
    private readonly Paint _paint = new();
    public IDeadlineColorizer? Colorizer { get; set; }

    protected override void OnDraw(Canvas canvas)
    {
        base.OnDraw(canvas);
        if (Colorizer is null) return;
        for (var x = 0; x < Width; x++)
        {
            _paint.Color = Colorizer.Sample(x / (double)Math.Max(Width - 1, 1));
            canvas.DrawLine(x, 0, x, Height, _paint);
        }
    }
}