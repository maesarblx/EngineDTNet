using OpenTK.Mathematics;

namespace EngineDNet;

public abstract class BaseObject2D(Vector2 position, Vector2 size, float rotation)
{
    public Vector2 Position { get; set; } = position;
    public Vector2 Size { get; set; } = size;
    public float Rotation { get; set; } = rotation;
}