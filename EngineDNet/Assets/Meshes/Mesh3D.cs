using EngineDNet.Utilities;
using OpenTK.Graphics.OpenGL4;
using System.Numerics;
using System.Runtime.InteropServices;

namespace EngineDNet.Assets.Meshes;
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
    private static Dictionary<List<float>, Mesh3D> _cachedMeshes = new();

    public static Mesh3D Load(List<float> Vertices)
    {
        if (_cachedMeshes.ContainsKey(Vertices)) return _cachedMeshes[Vertices];
        Mesh3D mesh = new(Vertices);
        _cachedMeshes[Vertices] = mesh;
        return mesh;
    }

    private Mesh3D(List<float> Vertices)
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

    public float CalculateMass()
    {
        Vector3 size = GetBoundingSize();
        return (size.X + size.Y + size.Z) / 3 * size.LengthSquared() * 0.35f;
    }

    public Vector3 GetBoundingSize()
    {
        if (CurrentVertices == null || CurrentVertices.Count < 3)
        {
            Utils.ColoredWriteLine("Vertices is not creater or vertices count is under 3. Returning zero", ConsoleColor.Yellow);
            return Vector3.Zero;
        }

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

        if (minX == float.MaxValue || minY == float.MaxValue || minZ == float.MaxValue)
        {
            Utils.ColoredWriteLine("Minimal values is NAN. Returning zero", ConsoleColor.Yellow);
            return Vector3.Zero;
        }

        return new Vector3(maxX - minX, maxY - minY, maxZ - minZ) * Size;
    }

}
