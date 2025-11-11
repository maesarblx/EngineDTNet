namespace EngineDNet.Assets;

public class AssetManager : IAssetManager
{
    private readonly Dictionary<string, object> _cache = new();
    private readonly List<IAssetLoader> _loaders;

    public AssetManager(IEnumerable<IAssetLoader> loaders)
    {
        _loaders = loaders.ToList();
    }

    public async Task<T> LoadAsync<T>(string path) where T : class
    {
        if (_cache.TryGetValue(path, out var obj))
            return obj as T;

        var loader = _loaders.FirstOrDefault(l => l.CanLoad(Path.GetExtension(path)));
        if (loader == null) throw new InvalidOperationException("No loader");
        var loaded = await loader.LoadAsync(path);
        _cache[path] = loaded;
        return loaded as T;
    }

    public T Load<T>(string path) where T : class
    {
        if (_cache.TryGetValue(path, out var obj))
            return obj as T;

        var loader = _loaders.FirstOrDefault(l => l.CanLoad(Path.GetExtension(path)));
        if (loader == null) throw new InvalidOperationException("No loader");
        var loaded = loader.Load(path);
        _cache[path] = loaded;
        return loaded as T;
    }

    public T Get<T>(string id) where T : class => _cache.TryGetValue(id, out var v) ? v as T : null;
    public void Unload(string id) => _cache.Remove(id);
    public void Dispose() => _cache.Clear();
}
