using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace EngineDNet;
public class CameraController(Camera camera)
{
    public Camera Camera = camera;

    public float MinPitch = -89f;
    public float MaxPitch = 89f;

    public float Sensitivity = 0.09f;

    public float FreeSpeed = 10f;

    public void Update(Vector2 mouseDelta, float deltaTime, bool W, bool A, bool S, bool D, bool E, bool Q)
    {
        Camera.Pitch += mouseDelta.Y * Sensitivity;
        Camera.Yaw += mouseDelta.X * Sensitivity;

        Camera.Pitch = Utils.Clamp(Camera.Pitch, MinPitch, MaxPitch);

        Camera.Rotation = new Vector3(Utils.Rad(Camera.Pitch), Utils.Rad(Camera.Yaw), 0);

        var Move = Vector3.Zero;
        if (W)
        {
            Move -= Camera.GetFrontVector();
        }
        if (A)
        {
            Move += Camera.GetRightVector();
        }
        if (S)
        {
            Move += Camera.GetFrontVector();
        }
        if (D)
        {
            Move -= Camera.GetRightVector();
        }
        if (E)
        {
            Move += Vector3.UnitY;
        }
        if (Q)
        {
            Move -= Vector3.UnitY;
        }
        Move *= FreeSpeed * deltaTime;
        Camera.Position += Move;
    }
}