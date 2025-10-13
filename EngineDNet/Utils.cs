namespace EngineDNet;
public static class Utils
{
    public static float Rad(float x)
    {
        return x * ((float) Math.PI / 180f);
    }
    public static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }
    public static void ColoredWriteLine(string x, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(x);
        Console.ForegroundColor = ConsoleColor.White;
    }
    public static float RandomFloat(float min, float max)
    {
        return (float)(Core.Rand.NextDouble() * (max - min) + min);
    }
    public static float Clamp(float value, float min, float max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }
}
