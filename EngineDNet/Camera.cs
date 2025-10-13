using OpenTK.Mathematics;

namespace EngineDNet;

public class Camera()
{
    public Vector3 Position = Vector3.UnitZ * 3;
    public Vector3 Rotation = -Vector3.UnitZ;

    public float FOV = 70;
    public float Aspect = 2f;
    public float ZNear = 0.1f;
    public float ZFar = 1024;

    public float Yaw = 0f;
    public float Pitch = 0f;

    public Matrix4 GetViewMatrix()
    {
        return Matrix4.CreateTranslation(-Position) * GetRotationMatrix();
    }

    public Matrix4 GetProjectionMatrix()
    {
        return Matrix4.CreatePerspectiveFieldOfView(Utils.Rad(FOV), Aspect, ZNear, ZFar);
    }

    public Matrix4 GetRotationMatrix()
    {
        return Matrix4.CreateRotationY(Rotation.Y) * Matrix4.CreateRotationX(Rotation.X)  * Matrix4.CreateRotationZ(Rotation.Z);
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