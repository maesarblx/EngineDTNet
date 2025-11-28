using EngineDNet.Camera;
using EngineDNet.Objects;
using EngineDNet.Rendering;
using EngineDNet.Workspace;
using OpenTK.Graphics.OpenGL4;
using System.Numerics;

namespace EngineDNet.Graphics.Renderers;

public class GameObjectRenderer : IRenderer3D
{
    public static void Render(GameObject Object, Shader? shader, Camera3D camera, LightingSettings lightingSettings)
    {
        if (Object.Mesh == null) return;
        shader?.Use();

        Matrix4x4 Model = Object.GetModelMatrix();
        Matrix4x4 ViewMatrix = camera.GetViewMatrix();
        Matrix4x4 ProjMatrix = camera.GetProjectionMatrix();

        shader?.SetUniform("Model", Model);
        shader?.SetUniform("ViewMatrix", ViewMatrix);
        shader?.SetUniform("ProjMatrix", ProjMatrix);
        shader?.SetUniform("TexCoordsMult", Object.TexCoordsMult);
        shader?.SetUniform("VecTexCoordsMult", Object.VecTexCoordsMult);
        GL.Enable(EnableCap.CullFace);
        GL.Enable(EnableCap.DepthTest);
        shader?.SetUniform("tex", 0);
        shader?.SetUniform("lightDirection", lightingSettings.LightDirection.X, lightingSettings.LightDirection.Y, lightingSettings.LightDirection.Z);
        shader?.SetUniform("ambientColor", lightingSettings.AmbientColor.X, lightingSettings.AmbientColor.Y, lightingSettings.AmbientColor.Z);
        shader?.SetUniform("ambientIntensity", lightingSettings.AmbientIntensity);
        shader?.SetUniform("directionalLightEnabled", lightingSettings.DirectionalLightEnabled ? 1 : 0);
        GL.BindVertexArray(Object.Mesh.VAO);
        Object.Texture?.Bind(TextureUnit.Texture0);
        GL.DrawArrays(PrimitiveType.Triangles, 0, Object.Mesh.VerticesCount);
        GL.BindVertexArray(0);
    }

    public static void BasicRender(GameObject Object, Shader shader)
    {
        if (Object.Mesh == null) return;
        shader.SetUniform("Model", Object.GetModelMatrix());
        GL.BindVertexArray(Object.Mesh.VAO);
        GL.DrawArrays(PrimitiveType.Triangles, 0, Object.Mesh.VerticesCount);
        GL.BindVertexArray(0);
    }
}