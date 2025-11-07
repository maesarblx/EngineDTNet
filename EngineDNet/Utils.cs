using System.Numerics;

namespace EngineDNet;
public static class Utils
{
    public const float DegToRad = (float)Math.PI / 180f;
    public static Vector3 FlatXZ = Vector3.UnitX + Vector3.UnitZ;
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
    public static Vector3 QuaternionToEuler(Quaternion q)
    {
        // нормируем на всякий случай
        q = Quaternion.Normalize(q);

        // формулы стандартные для получения yaw/pitch/roll (X=pitch, Y=yaw, Z=roll)
        float ysqr = q.Y * q.Y;

        // X (pitch)
        float t0 = +2.0f * (q.W * q.X + q.Y * q.Z);
        float t1 = +1.0f - 2.0f * (q.X * q.X + ysqr);
        float X = MathF.Atan2(t0, t1);

        // Y (yaw)
        float t2 = +2.0f * (q.W * q.Y - q.Z * q.X);
        t2 = Math.Clamp(t2, -1f, 1f);
        float Y = MathF.Asin(t2);

        // Z (roll)
        float t3 = +2.0f * (q.W * q.Z + q.X * q.Y);
        float t4 = +1.0f - 2.0f * (ysqr + q.Z * q.Z);
        float Z = MathF.Atan2(t3, t4);

        return new Vector3(X, Y, Z); // в радианах
    }
}
