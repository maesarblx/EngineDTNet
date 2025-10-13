using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace EngineDNet;

public class Shader: IDisposable
{
    private int ProgramId { get; set; }
    private int VertexShaderId { get; set; }
    private int FragmentShaderId { get; set; }

    public void Use() => GL.UseProgram(ProgramId);
    private static int CompileShader(ShaderType shaderType, string source)
    {
        var shader = GL.CreateShader(shaderType);
        GL.ShaderSource(shader, source);
        GL.CompileShader(shader);
        
        GL.GetShader(shader, ShaderParameter.CompileStatus, out var success);
        
        if (success != 0) return shader;
        GL.GetShaderInfoLog(shader, out var info);
        GL.DeleteShader(shader);
        throw new Exception($"Failed to compile shader: {info}");
    }

    public void SetUniform(string name, Matrix4 matrix4) => GL.ProgramUniformMatrix4(ProgramId, GL.GetUniformLocation(ProgramId, name), false, ref matrix4);
    public void SetUniform(string name, int x) => GL.ProgramUniform1(ProgramId, GL.GetUniformLocation(ProgramId, name), x);
    public void SetUniform(string name, int x, int y) => GL.ProgramUniform2(ProgramId, GL.GetUniformLocation(ProgramId, name), x, y);
    public void SetUniform(string name, int x, int y, int z) => GL.ProgramUniform3(ProgramId, GL.GetUniformLocation(ProgramId, name), x, y, z);
    public void SetUniform(string name, int x, int y, int z, int w) => GL.ProgramUniform4(ProgramId, GL.GetUniformLocation(ProgramId, name), x, y, z, w);
    public void SetUniform(string name, float x) => GL.ProgramUniform1(ProgramId, GL.GetUniformLocation(ProgramId, name), x);
    public void SetUniform(string name, float x, float y) => GL.ProgramUniform2(ProgramId, GL.GetUniformLocation(ProgramId, name), x, y);
    public void SetUniform(string name, float x, float y, float z) => GL.ProgramUniform3(ProgramId, GL.GetUniformLocation(ProgramId, name), x, y, z);
    public void SetUniform(string name, float x, float y, float z, float w) => GL.ProgramUniform4(ProgramId, GL.GetUniformLocation(ProgramId, name), x, y, z, w);
    
    public void SetUniform(string name, Vector2 vector) => GL.ProgramUniform2(ProgramId, GL.GetUniformLocation(ProgramId, name), vector);
    public void SetUniform(string name, Vector3 vector) => GL.ProgramUniform3(ProgramId, GL.GetUniformLocation(ProgramId, name), vector);
    public void SetUniform(string name, Vector4 vector) => GL.ProgramUniform4(ProgramId, GL.GetUniformLocation(ProgramId, name), vector);
    
    public Shader(string vertexSource, string fragmentSource)
    {
        VertexShaderId = CompileShader(ShaderType.VertexShader, vertexSource);
        FragmentShaderId = CompileShader(ShaderType.FragmentShader, fragmentSource);
        
        ProgramId = GL.CreateProgram();
        GL.AttachShader(ProgramId, VertexShaderId);
        GL.AttachShader(ProgramId, FragmentShaderId);
        GL.LinkProgram(ProgramId);
        
        GL.GetProgram(ProgramId, GetProgramParameterName.LinkStatus, out var success);
        if(success != 0) return;

        GL.GetProgramInfoLog(ProgramId, out var info);
        throw new Exception($"Failed to link program: {info}");
    }

    ~Shader()
    {
        ReleaseUnmanagedResources();
    }

    private void ReleaseUnmanagedResources()
    {
        GL.DeleteProgram(ProgramId);
        GL.DeleteShader(VertexShaderId);
        GL.DeleteShader(FragmentShaderId);
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }
}