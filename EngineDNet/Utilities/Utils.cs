using System.Numerics;

namespace EngineDNet.Utilities;

public static class Utils
{
    private static Random _rand = new();
    public const float DegToRad = (float)Math.PI / 180f;
    public static Vector3 FlatXZ = Vector3.UnitX + Vector3.UnitZ;
    public static float Deg2Rad = (float)Math.PI / 180f;
    public static float Rad2Deg = (float)Math.PI * 180f;
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
        return (float)(_rand.NextDouble() * (max - min) + min);
    }
}
