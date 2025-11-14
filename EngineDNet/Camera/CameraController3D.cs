using EngineDNet.Utilities;
using System.Numerics;

namespace EngineDNet.Camera;
public class CameraController3D(Camera3D camera)
{
    public Camera3D Camera = camera;

    public float MinPitch = -89f;
    public float MaxPitch = 89f;

    public float Sensitivity = 0.09f;

    public float FreeSpeed = 10f;

    private float _zlean = 0;

    public void Update(Vector2 mouseDelta)
    {
        Camera.Pitch += mouseDelta.Y * Sensitivity;
        Camera.Yaw += mouseDelta.X * Sensitivity;

        Camera.Pitch = Math.Clamp(Camera.Pitch, MinPitch, MaxPitch);

        Camera.Rotation = new Vector3(Utils.Deg2Rad * Camera.Pitch, Utils.Deg2Rad * Camera.Yaw, 0);

        if (Core.CurrentPlayer != null)
        {
            Camera.Position = Core.CurrentPlayer.Position + Vector3.UnitY * (Core.CurrentPlayer.Height * 0.5f);
            Camera.Rotation.Z -= _zlean;
            _zlean = Utils.Lerp(_zlean, Core.CurrentPlayer.MoveDirection.X * 0.075f, 8 * Core.FrameTime);
        }
    }
}