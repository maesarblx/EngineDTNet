using EngineDNet.Assets.Meshes;
using EngineDNet.Rendering;
using OpenTK.Graphics.OpenGL4;
using System.Numerics;

namespace EngineDNet.Graphics.Renderers;

public static class TextRenderer
{
    public static void Render(string text, FontMesh fontMesh, Shader shader, Matrix4x4 baseMatrix, float fontSize, Vector3 color)
    {
        var model = Matrix4x4.Identity;
        var offset = Vector3.Zero;
        foreach (var t in text)
        {
            switch (t)
            {
                case ' ':
                    offset += Vector3.UnitX;
                    model = Matrix4x4.CreateTranslation(offset);
                    continue;
                case '\n':
                    offset = new Vector3(0.0f, offset.Y + 3.0f, 0.0f);
                    model = Matrix4x4.CreateTranslation(offset);
                    continue;
            }

            if (!fontMesh.Glyphs.TryGetValue(t, out var character)) continue;
            var mesh = character.Mesh;
            if (mesh == null) continue;
            shader.Use();
            shader.SetUniform("color", color);
            shader.SetUniform("model", model * Matrix4x4.CreateScale(fontSize) * baseMatrix);
            offset += Vector3.UnitX * character.Advance;
            model = Matrix4x4.CreateTranslation(offset);
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
            GL.BindVertexArray(mesh.Vao);
            GL.DrawElements(PrimitiveType.Triangles, mesh.IndicesCount, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);
        }
    }
}