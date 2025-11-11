using EngineDNet.Utilities;
using System.Numerics;

namespace EngineDNet.Camera;

public class Camera3D()
{
    public Vector3 Position = Vector3.UnitY * 2;
    public Vector3 Rotation = Vector3.Zero;

    public float FOV = 70;
    public float Aspect = 2f;
    public float ZNear = 0.1f;
    public float ZFar = 2048;

    public float Yaw = 90f;
    public float Pitch = 0f;

    public Matrix4x4 GetViewMatrix()
    {
        return Matrix4x4.CreateTranslation(-Position) * GetRotationMatrix();
    }

    public Matrix4x4 GetProjectionMatrix()
    {
        return Matrix4x4.CreatePerspectiveFieldOfView(Utils.Deg2Rad * FOV, Aspect, ZNear, ZFar);
    }

    public Matrix4x4 GetRotationMatrix()
    {
        return Matrix4x4.CreateRotationY(Rotation.Y) * Matrix4x4.CreateRotationX(Rotation.X)  * Matrix4x4.CreateRotationZ(Rotation.Z);
    }

    public Vector3 GetFrontVector()
    {
        var yawRad = Utils.Deg2Rad * Yaw;
        return new Vector3((float)-Math.Sin(yawRad), 0, (float)Math.Cos(yawRad));
    }

    public Vector3 GetRightVector()
    {
        var yawRad = Utils.Deg2Rad * (Yaw + 90);
        return new Vector3((float)-Math.Sin(yawRad), 0, (float)Math.Cos(yawRad));
    }

    public Vector3 GetUpVector()
    {
        float yawRad = Utils.Deg2Rad * Yaw;
        float pitchRad = Utils.Deg2Rad * Pitch;

        pitchRad = Math.Clamp(pitchRad, -MathF.PI / 2f + 0.001f, MathF.PI / 2f - 0.001f);

        Vector3 front = new Vector3(
            -MathF.Sin(yawRad) * MathF.Cos(pitchRad),
             MathF.Sin(pitchRad),
             MathF.Cos(yawRad) * MathF.Cos(pitchRad)
        );
        front = Vector3.Normalize(front);

        Vector3 right = Vector3.Normalize(Vector3.Cross(front, Vector3.UnitY));

        Vector3 up = Vector3.Normalize(Vector3.Cross(right, front));

        return up;
    }

}