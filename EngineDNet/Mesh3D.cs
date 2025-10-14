using OpenTK.Graphics.OpenGL4;
using System.Numerics;
//using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace EngineDNet;
public class Mesh3D
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex
    {
        public Vector3 Position;
        public Vector2 TexCoord;
        public Vector3 Normal;
    }

    public int VAO { get; private set; }
    public int VerticesCount { get; private set; }
    public List<float> CurrentVertices { get; private set; }
    public Vector3 Size;
    private int _vbo;

    public Mesh3D(List<float> Vertices)
    {
        int stride = 8 * sizeof(float);
        VAO = GL.GenVertexArray();
        _vbo = GL.GenBuffer();
        VerticesCount = Vertices.Count / 8;
        CurrentVertices = Vertices;
        GL.BindVertexArray(VAO);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, Vertices.Count * sizeof(float), Vertices.ToArray(), BufferUsageHint.StaticDraw);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, stride, 5 * sizeof(float));
        GL.EnableVertexAttribArray(2);
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindVertexArray(0);
    }

    public Vector3 GetBoundingSize()
    {
        if (CurrentVertices == null || CurrentVertices.Count < 3)
            return Vector3.Zero;

        // Структура: 8 float на вершину, позиция в первых 3
        const int stride = 8;
        float minX = float.MaxValue, minY = float.MaxValue, minZ = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue, maxZ = float.MinValue;

        for (int i = 0; i + 2 < CurrentVertices.Count; i += stride)
        {
            float x = CurrentVertices[i];
            float y = CurrentVertices[i + 1];
            float z = CurrentVertices[i + 2];

            if (x < minX) minX = x;
            if (y < minY) minY = y;
            if (z < minZ) minZ = z;

            if (x > maxX) maxX = x;
            if (y > maxY) maxY = y;
            if (z > maxZ) maxZ = z;
        }

        // Если не было валидных вершин (защита на случай некорректных данных)
        if (minX == float.MaxValue || minY == float.MaxValue || minZ == float.MaxValue)
            return Vector3.Zero;

        return new Vector3(maxX - minX, maxY - minY, maxZ - minZ) * Size;
    }

}
