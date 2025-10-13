using OpenTK.Mathematics;

namespace EngineDNet;
public class Scene
{
    public GameObject Root { get; private set; }
    public LightingSettings SceneLightingSettings = new LightingSettings();
    public Scene()
    {
        Root = new GameObject("Root", Vector3.Zero, Vector3.Zero, Vector3.One);
    }
}
