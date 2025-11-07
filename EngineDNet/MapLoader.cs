using BepuPhysics;
using BepuPhysics.Collidables;
using System.Numerics;
using System.Text.Json;

namespace EngineDNet;

public class MapObject
{
    public string Texture { get; set; } = "";
    public string Mesh { get; set; } = "";
    public bool PhysicsEnabled { get; set; } = false;
    public float? Mass { get; set; }
    public float? TexCoordsMult { get; set; } = 1;
    public float[] Position { get; set; } = new float[3];
    public float[] Rotation { get; set; } = new float[3];
    public float[] Scale { get; set; } = new float[3];
}

public class MapData
{
    public Dictionary<string, MapObject> Objects { get; set; } = new();
}

public static class MapLoader
{
    public static List<GameObject> Load(string json)
    {
        var map = JsonSerializer.Deserialize<MapData>(json);
        var result = new List<GameObject>();
        if (map == null) return result;

        foreach (var (name, obj) in map.Objects)
        {
            var mesh = new Mesh3D(MeshLoader.Load($"models/{obj.Mesh}"));
            var texture = new Texture2D($"textures/{obj.Texture}");
            var position = new Vector3(obj.Position[0], obj.Position[1], obj.Position[2]);
            var rotation = new Vector3(Utils.Rad(obj.Rotation[0]), Utils.Rad(obj.Rotation[1]), Utils.Rad(obj.Rotation[2]));
            var scale = new Vector3(obj.Scale[0], obj.Scale[1], obj.Scale[2]);
            var gameObject = new GameObject(name, position, rotation, scale, mesh, texture, null, obj.Mass);
            gameObject.TexCoordsMult = obj.TexCoordsMult != null ? (float)obj.TexCoordsMult : 1;
            mesh.Size = scale;
            gameObject.PhysicsEnabled = obj.PhysicsEnabled;
            if (obj.Mass == null)
                gameObject.MassCalculate();
            gameObject.InitializePhysics();
            Utils.ColoredWriteLine($"[MapLoader] Loaded object: {name}", ConsoleColor.Magenta);
            result.Add(gameObject);
        }

        return result;
    }
}