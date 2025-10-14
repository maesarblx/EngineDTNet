using System.Numerics;
//using OpenTK.Mathematics;

namespace EngineDNet;

public abstract class Object2DMesh(Vector2 position, Vector2 size, float rotation, Mesh2D? mesh = null) : BaseObject2D(position, size, rotation)
{
    public Mesh2D? Mesh2D { get; set; } = mesh;
    public Vector3 Color { get; set; } = Vector3.One;
}