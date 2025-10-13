using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

namespace EngineDNet;
public class Program
{
    public static void Main(string[] args)
    {
        Core.WindowSize = new Vector2i(Core.CurrentMonitor.Width / 2, Core.CurrentMonitor.Height / 2);
        Core.Title = "Nate Higgers";
        Core.VSync = VSyncMode.Off;

        Core.VertexShaderPath = "shaders/3d/main.vs";
        Core.FragmentShaderPath = "shaders/3d/main.frag";

        Core.TextVertexShaderPath = "shaders/2d/text.frag";
        Core.TextFragmentShaderPath = "shaders/2d/text.vs";

        Core.Start();
    }
}