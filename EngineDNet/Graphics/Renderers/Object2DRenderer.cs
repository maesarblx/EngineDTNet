using EngineDNet.Assets.Meshes;
using EngineDNet.Assets.Textures;
using EngineDNet.Rendering;
using EngineDNet.Utilities;
using OpenTK.Graphics.OpenGL4;
using System.Numerics;

namespace EngineDNet.Graphics.Renderers;

public class Object2DRenderer : IRenderer2D
{
    public static Mesh2D? RectMesh;
    public static Shader? TextShader;
    public static void Render(Object2DMesh objectMesh2d, Shader shader, int screenWidth, int screenHeight)
    {
        Mesh2D? mesh2d = objectMesh2d.Mesh2D;
        shader.Use();
        GL.Disable(EnableCap.DepthTest);
        GL.Disable(EnableCap.CullFace);
        Matrix4x4 projection = Matrix4x4.CreateOrthographicOffCenter(0.0f, screenWidth, screenHeight, 0.0f, -1.0f, 1.0f);
        Vector2 size = objectMesh2d.Size;
        Matrix4x4 model = Matrix4x4.CreateScale(new Vector3(size.X, size.Y, 1.0f))
                    * Matrix4x4.CreateRotationZ(Utils.DegToRad * objectMesh2d.Rotation)
                    * Matrix4x4.CreateTranslation(Vector3.UnitX * objectMesh2d.Position.X + Vector3.UnitY * objectMesh2d.Position.Y);
        shader.SetUniform("projection", projection);
        shader.SetUniform("model", model);
        shader.SetUniform("color", objectMesh2d.Color);
        switch (objectMesh2d)
        {
            case Rect2D when RectMesh != null:
                GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
                GL.BindVertexArray(RectMesh.Vao);
                GL.DrawElements(PrimitiveType.Triangles, RectMesh.IndicesCount, DrawElementsType.UnsignedInt, 0);
                break;
            case Text2D text when TextShader != null:
                TextRenderer.Render(text.Text, text.FontMesh, TextShader, model, text.TextSize, objectMesh2d.Color);
                break;
            default:
            {
                if(mesh2d != null)
                {
                    GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
                    GL.BindVertexArray(mesh2d.Vao);
                    GL.DrawElements(PrimitiveType.Triangles, mesh2d.IndicesCount, DrawElementsType.UnsignedInt, 0);
                }
                break;
            }
        }
        GL.BindVertexArray(0);
    }

    public static void Render(Object2DMesh objectMesh2d, Shader shader, int textureID)
    {
        Mesh2D? mesh2d = objectMesh2d.Mesh2D;
        shader.Use();
        GL.Disable(EnableCap.DepthTest);
        GL.Disable(EnableCap.CullFace);
        shader.SetUniform("tex", 0);
        GL.BindTexture(TextureTarget.Texture2D, textureID);
        switch (objectMesh2d)
        {
            case Rect2D when RectMesh != null:
                GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
                GL.BindVertexArray(RectMesh.Vao);
                GL.DrawElements(PrimitiveType.Triangles, RectMesh.IndicesCount, DrawElementsType.UnsignedInt, 0);
                break;
            default:
                {
                    if (mesh2d != null)
                    {
                        GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
                        GL.BindVertexArray(mesh2d.Vao);
                        GL.DrawElements(PrimitiveType.Triangles, mesh2d.IndicesCount, DrawElementsType.UnsignedInt, 0);
                    }
                    break;
                }
        }
        GL.BindVertexArray(0);
    }
}