using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace EngineDNet
{
    internal class GameObjectRenderer
    {
        public static void Render(GameObject Object, Shader shader, Camera camera, LightingSettings lightingSettings)
        {
            if (Object.Mesh == null) return;
            shader.Use();

            Matrix4 Model = Object.GetModelMatrix();
            Matrix4 ViewMatrix = camera.GetViewMatrix();
            Matrix4 ProjMatrix = camera.GetProjectionMatrix();

            shader.SetUniform("Model", Model);
            shader.SetUniform("ViewMatrix", ViewMatrix);
            shader.SetUniform("ProjMatrix", ProjMatrix);
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthTest);
            shader.SetUniform("tex", 0);
            shader.SetUniform("lightDirection", lightingSettings.LightDirection.X, lightingSettings.LightDirection.Y, lightingSettings.LightDirection.Z);
            shader.SetUniform("ambientColor", lightingSettings.AmbientColor.X, lightingSettings.AmbientColor.Y, lightingSettings.AmbientColor.Z);
            shader.SetUniform("ambientIntensity", lightingSettings.AmbientIntensity);
            shader.SetUniform("shadowsEnabled", lightingSettings.ShadowsEnabled ? 1 : 0);
            GL.BindVertexArray(Object.Mesh.VAO);
            Object.Texture?.Bind(TextureUnit.Texture0);
            GL.DrawArrays(PrimitiveType.Triangles, 0, Object.Mesh.VerticesCount);
            GL.BindVertexArray(0);
        }
    }
}
