using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineDNet
{
    public class LightingSettings
    {
        public Vector3 AmbientColor = new Vector3(1.0f, 1.0f, 1.0f);
        public float AmbientIntensity = 0.1f;
        public Vector3 LightDirection = new Vector3(-0.2f, -1.0f, -0.3f);
        public bool DirectionalLightEnabled = false;
    }
}
