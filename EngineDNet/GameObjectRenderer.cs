using OpenTK.Graphics.OpenGL4;

namespace EngineDNet;
internal class GameObjectRenderer
{
    public static void Render(GameObject Object, Shader shader, Camera camera, LightingSettings lightingSettings)
    {
        if (Object.Mesh == null) return;
        shader.Use();

        var Model = Object.GetModelMatrix();
        var ViewMatrix = camera.GetViewMatrix();
        var ProjMatrix = camera.GetProjectionMatrix();

        shader.SetUniform("Model", Model);
        shader.SetUniform("ViewMatrix", ViewMatrix);
        shader.SetUniform("ProjMatrix", ProjMatrix);
        shader.SetUniform("TexCoordsMult", Object.TexCoordsMult);
        shader.SetUniform("VecTexCoordsMult", Object.VecTexCoordsMult);
        GL.Enable(EnableCap.CullFace);
        GL.Enable(EnableCap.DepthTest);
        shader.SetUniform("tex", 0);
        shader.SetUniform("lightDirection", lightingSettings.LightDirection.X, lightingSettings.LightDirection.Y, lightingSettings.LightDirection.Z);
        shader.SetUniform("ambientColor", lightingSettings.AmbientColor.X, lightingSettings.AmbientColor.Y, lightingSettings.AmbientColor.Z);
        shader.SetUniform("ambientIntensity", lightingSettings.AmbientIntensity);
        shader.SetUniform("directionalLightEnabled", lightingSettings.DirectionalLightEnabled ? 1 : 0);
        GL.BindVertexArray(Object.Mesh.VAO);
        Object.Texture?.Bind(TextureUnit.Texture0);
        GL.DrawArrays(PrimitiveType.Triangles, 0, Object.Mesh.VerticesCount);
        GL.BindVertexArray(0);
    }
}