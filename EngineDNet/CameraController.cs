using System.Numerics;

namespace EngineDNet;
public class CameraController(Camera camera)
{
    public Camera Camera = camera;

    public float MinPitch = -89f;
    public float MaxPitch = 89f;

    public float Sensitivity = 0.09f;

    public float FreeSpeed = 10f;

    public void Update(Vector2 mouseDelta, float deltaTime)
    {
        Camera.Pitch += mouseDelta.Y * Sensitivity;
        Camera.Yaw += mouseDelta.X * Sensitivity;

        Camera.Pitch = Utils.Clamp(Camera.Pitch, MinPitch, MaxPitch);

        Camera.Rotation = new Vector3(Utils.Deg2Rad * (Camera.Pitch), Utils.Deg2Rad * (Camera.Yaw), 0);
        if (Core.CurrentPlayer != null)
            Camera.Position = Core.CurrentPlayer.Position + (Vector3.UnitY * (Core.CurrentPlayer.Height * 0.5f));
    }
}