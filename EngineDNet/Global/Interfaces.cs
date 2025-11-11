using EngineDNet.Assets.Meshes;
using EngineDNet.Camera;
using EngineDNet.Objects;
using EngineDNet.Rendering;
using EngineDNet.Workspace;
using System.Numerics;

public interface IRenderer3D
{
    static void Render(GameObject @object, Shader shader, Camera3D camera, LightingSettings lighting) { }
}

public interface IRenderer2D
{
    static void Render(Object2DMesh @mesh, Shader shader, int screenWidth, int screenHeight) { }
}

public interface IRendererText
{
    static void Render(string text, FontMesh fontMesh, Shader shader, Matrix4x4 baseMatrix, float fontSize, Vector3 color) { }
}

public interface IAssetManager : IDisposable
{
    Task<T> LoadAsync<T>(string path) where T : class;
    T Get<T>(string id) where T : class;
    void Unload(string id);
}

public interface IAssetLoader
{
    bool CanLoad(string extension);
    object Load(string path);
    Task<object> LoadAsync(string path);
}

public interface IMapLoader
{
    bool CanLoad(string extension);
    List<GameObject> Load(string path);
    Task<List<GameObject>> LoadAsync(string path);
}