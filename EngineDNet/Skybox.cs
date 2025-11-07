using System.Numerics;

namespace EngineDNet;

public class Skybox
{
    public GameObject Object;
    public Skybox() 
    {
        Object = Core.LoadObject("Skybox", Vector3.Zero, Vector3.Zero, Vector3.One * 512, Core.CurrentScene.Root);
    }
}