using BepuPhysics;
using BepuPhysics.Collidables;
using EngineDNet.Assets.Meshes;
using EngineDNet.Assets.Textures;
using EngineDNet.Objects;
using EngineDNet.Utilities;
using System.Numerics;
using System.Text.Json;

namespace EngineDNet.Assets.Loaders;

public class MapObject
{
    public string Texture { get; set; } = "";
    public string Mesh { get; set; } = "";
    public bool PhysicsEnabled { get; set; } = false;
    public float? Mass { get; set; }
    public float? TexCoordsMult { get; set; } = 1;
    public float[]? VecTexCoordsMult { get; set; }
    public float[] Position { get; set; } = new float[3];
    public float[] Rotation { get; set; } = new float[3];
    public float[] Scale { get; set; } = new float[3];
}

public class MapData
{
    public Dictionary<string, MapObject> Objects { get; set; } = new();
}

public class MapLoader : IAssetLoader
{
    public bool CanLoad(string extension) => extension == ".dnm" || extension == ".json";

    public List<GameObject> Load(string path)
    {
        string json = File.ReadAllText(path);
        MapData map = JsonSerializer.Deserialize<MapData>(json);
        List<GameObject> result = new();
        if (map == null) return result;

        foreach (var (name, obj) in map.Objects)
        {
            Mesh3D mesh = Mesh3D.Load(Core.AssetManager.Load<List<float>>($"models/{obj.Mesh}"));
            Texture2D texture = Texture2D.Load($"textures/{obj.Texture}");
            Vector3 position = new(obj.Position[0], obj.Position[1], obj.Position[2]);
            Vector3 rotation = new(obj.Rotation[0], obj.Rotation[1], obj.Rotation[2]);
            Vector3 scale = new(obj.Scale[0], obj.Scale[1], obj.Scale[2]);
            GameObject gameObject = new(name, position, rotation, scale, mesh, texture, null, obj.Mass);

            mesh.Size = scale;

            gameObject.TexCoordsMult = obj.TexCoordsMult != null ? (float)obj.TexCoordsMult : 1;
            gameObject.VecTexCoordsMult = obj.VecTexCoordsMult != null ? new Vector2(obj.VecTexCoordsMult[0], obj.VecTexCoordsMult[1]) : Vector2.One;
            gameObject.PhysicsEnabled = obj.PhysicsEnabled;

            if (obj.Mass == null)
                gameObject.MassCalculate();

            gameObject.InitializePhysics();

            Utils.ColoredWriteLine($"[MapLoader] Loaded object: {name}", ConsoleColor.Magenta);

            result.Add(gameObject);
        }

        return result;
    }

    public async Task<object> LoadAsync(string path)
    {
        return await Task.Run(() => Load(path));
    }

    object IAssetLoader.Load(string path)
    {
        return Load(path);
    }
}