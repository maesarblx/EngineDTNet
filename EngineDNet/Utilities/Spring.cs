using System.Numerics;

namespace EngineDNet.Utilities;

public class Spring
{
    public Vector3 Position;
    public Vector3 Velocity;
    public float Damping = 9;
    public float Speed = 65;

    public Spring(float Damper = 9, float speed = 65)
    {
        Damping = Damper;
        Speed = speed;
    }

    public void Update()
    {
        if (Velocity.Length() < 0.00001f)
        {
            Position = Vector3.Zero; Velocity = Vector3.Zero;
            return;
        }
        Position += Velocity * Core.FrameTime;
        float damping = 1 - (Damping * Core.FrameTime);
        if (damping < 0)
            damping = 0;
        Velocity *= damping;

        float springForceMagnitude = Speed * Core.FrameTime;
        springForceMagnitude = Math.Clamp(springForceMagnitude, 0, 2);
        Velocity -= Position * springForceMagnitude;

        Position = new Vector3(
            Math.Clamp(Position.X, -89, 89),
            Math.Clamp(Position.Y, -179, 179),
            Math.Clamp(Position.Z, -89, 89)
        );
    }
}
