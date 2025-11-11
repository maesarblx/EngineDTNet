using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using System.Numerics;

namespace EngineDNet;

public class Mesh2D: IDisposable
{
    public int Vao { get; private set; }
    private int Vbo { get; set; }
    private int Ebo { get; set; }
    public int IndicesCount { get; private set; }
    public int VerticesCount { get; private set; }
    
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex(Vector2 position, Vector2 texCoords)
    {
        public Vector2 Position = position;
        public Vector2 TexCoords = texCoords;
    }
    
    public Mesh2D(Vertex[] mesh, int[] indices)
    {
        Vao = GL.GenVertexArray();
        Vbo = GL.GenBuffer();
        Ebo = GL.GenBuffer();

        IndicesCount = indices.Length;
        VerticesCount = mesh.Length;
        
        GL.BindVertexArray(Vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, Vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, mesh.Length * Marshal.SizeOf<Vertex>(), mesh, BufferUsageHint.StaticDraw);
        
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, Ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * Marshal.SizeOf<int>(), indices, BufferUsageHint.StaticDraw);
        
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, Marshal.SizeOf<Vertex>(), 0);
        GL.EnableVertexAttribArray(0);
        
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, Marshal.SizeOf<Vertex>(), (int)Marshal.OffsetOf<Vertex>(nameof(Vertex.TexCoords)));
        GL.EnableVertexAttribArray(1);
        
        GL.BindVertexArray(0);
    }

    ~Mesh2D()
    {
        ReleaseUnmanagedResources(false);
    }

    private void ReleaseUnmanagedResources(bool disposed)
    {
        if(!disposed) return;
        GL.DeleteVertexArray(Vao);
        GL.DeleteBuffer(Vbo);
        GL.DeleteBuffer(Ebo);
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources(true);
        GC.SuppressFinalize(this);
    }
}