using System.Numerics;
//using OpenTK.Mathematics;

namespace EngineDNet;

public class Camera()
{
    public Vector3 Position = Vector3.UnitY * 2;
    public Vector3 Rotation = Vector3.Zero;

    public float FOV = 70;
    public float Aspect = 2f;
    public float ZNear = 0.1f;
    public float ZFar = 1024;

    public float Yaw = 90f;
    public float Pitch = 0f;

    public Matrix4x4 GetViewMatrix()
    {
        return Matrix4x4.CreateTranslation(-Position) * GetRotationMatrix();
    }

    public Matrix4x4 GetProjectionMatrix()
    {
        return Matrix4x4.CreatePerspectiveFieldOfView(Utils.Rad(FOV), Aspect, ZNear, ZFar);
    }

    public Matrix4x4 GetRotationMatrix()
    {
        return Matrix4x4.CreateRotationY(Rotation.Y) * Matrix4x4.CreateRotationX(Rotation.X)  * Matrix4x4.CreateRotationZ(Rotation.Z);
    }

    public Vector3 GetFrontVector()
    {
        var yawRad = Utils.Rad(Yaw);
        return new Vector3((float)-Math.Sin(yawRad), 0, (float)Math.Cos(yawRad));
    }

    public Vector3 GetRightVector()
    {
        var yawRad = Utils.Rad(Yaw + 90);
        return new Vector3((float)-Math.Sin(yawRad), 0, (float)Math.Cos(yawRad));
    }
}