using System.Numerics;

namespace EngineDNet.Workspace;

public class LightingSettings
{
    public Vector3 AmbientColor = new Vector3(1.0f, 1.0f, 1.0f);
    public float AmbientIntensity = 0.1f;
    public float Saturation = 1f;
    public float Contrast = 1f;
    public float Brightness = 0f;
    public float Exposure = 1f;
    public Vector3 Tint = new(1, 1, 1);
    public Vector3 LightDirection = new Vector3(-0.2f, -1.0f, -0.3f);
    public bool DirectionalLightEnabled = false;
}