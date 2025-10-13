using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace EngineDNet;

public static class Object2DRenderer
{
    public static Mesh2D? RectMesh;
    public static Shader? TextShader;
    public static void Render(Object2DMesh objectMesh2d, Shader shader, int screenWidth, int screenHeight)
    {
        var mesh2d = objectMesh2d.Mesh2D;
        shader.Use();
        GL.Disable(EnableCap.DepthTest);
        GL.Disable(EnableCap.CullFace);
        var projection = Matrix4.CreateOrthographicOffCenter(0.0f, screenWidth, screenHeight, 0.0f, -1.0f, 1.0f);
        var size = objectMesh2d.Size;
        var model = Matrix4.CreateScale(new Vector3(size.X, size.Y, 1.0f))
                    * Matrix4.CreateRotationZ(MathHelper.DegToRad * objectMesh2d.Rotation)
                    * Matrix4.CreateTranslation(new Vector3(objectMesh2d.Position));
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
                //GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);
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
}