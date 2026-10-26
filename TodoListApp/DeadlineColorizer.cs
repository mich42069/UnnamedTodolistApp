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

public interface IDeadlineColorizer { Color GetColor(DateTime deadline); }

public class GradientDeadlineColorizer(Color far, Color near, TimeSpan span, ICurve curve) : IDeadlineColorizer
{
    public Color GetColor(DateTime deadline)
    {
        var t = curve.Apply(Math.Clamp(1 - (deadline - DateTime.Now) / span, 0, 1));
        return Color.Rgb(
            (int)(far.R + (near.R - far.R) * t),
            (int)(far.G + (near.G - far.G) * t),
            (int)(far.B + (near.B - far.B) * t));
    }
}