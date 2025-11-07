using System.Numerics;
namespace EngineDNet;

public class Text2D(Vector2 position, Vector2 size, float rotation, FontMesh fontMesh): Object2DMesh(position, size, rotation)
{
    public string Text { get; set; } = "";
    public FontMesh FontMesh = fontMesh;
    public float TextSize = 12f;
}