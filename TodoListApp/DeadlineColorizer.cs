namespace TodoListApp;

public interface ICurve { double Apply(double t); }

public class LinearCurve : ICurve
{
    public double Apply(double t) => t;
}

public class PowerCurve(double exponent) : ICurve
{
    public double Apply(double t) => Math.Pow(t, exponent);
}

public interface IDeadlineColorizer
{
    Color GetColor(DateTime deadline);
    Color Sample(double t); // t: 0 = far from deadline, 1 = at deadline
}

public class GradientDeadlineColorizer(IReadOnlyList<Color> stops, TimeSpan span, ICurve curve, int steps = 0)
    : IDeadlineColorizer
{
    public Color GetColor(DateTime deadline) =>
        Sample(Math.Clamp(1 - (deadline - DateTime.Now) / span, 0, 1));

    public Color Sample(double t)
    {
        t = curve.Apply(t);
        if (steps > 0) t = Math.Min(Math.Floor(t * steps), steps - 1) / (steps - 1);

        var scaled = t * (stops.Count - 1);
        var i = Math.Min((int)scaled, stops.Count - 2);
        var f = scaled - i;
        Color a = stops[i], b = stops[i + 1];
        return Color.Rgb(
            (int)(a.R + (b.R - a.R) * f),
            (int)(a.G + (b.G - a.G) * f),
            (int)(a.B + (b.B - a.B) * f));
    }
}