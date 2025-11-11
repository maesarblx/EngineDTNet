using EngineDNet.Assets.Meshes;

namespace EngineDNet.Managers;
public static class Mesh3DManager
{
    private static Dictionary<string, Mesh3D> _meshes = new Dictionary<string, Mesh3D>();

    public static void AddMesh(string name, Mesh3D mesh)
    {
        if (!_meshes.ContainsKey(name))
        {
            _meshes[name] = mesh;
        }
        else
        {
            throw new ArgumentException($"A mesh with the name '{name}' already exists.");
        }
    }

    public static Mesh3D GetMesh(string name)
    {
        if (_meshes.TryGetValue(name, out var mesh))
        {
            return mesh;
        }
        else
        {
            throw new KeyNotFoundException($"No mesh found with the name '{name}'.");
        }
    }

    public static bool RemoveMesh(string name)
    {
        return _meshes.Remove(name);
    }

    public static void ClearMeshes()
    {
        _meshes.Clear();
    }
}