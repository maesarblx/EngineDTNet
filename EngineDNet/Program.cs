using OpenTK.Windowing.Common;

namespace EngineDNet;
public class Program
{
    public static void Main(string[] args)
    {
        Core.WindowSize = new OpenTK.Mathematics.Vector2i(Core.CurrentMonitor.Width / 2, Core.CurrentMonitor.Height / 2);
        Core.Title = "EngineDNet";
        Core.VSync = VSyncMode.Off;

        Core.VertexShaderPath = "shaders/3d/main.dsv";
        Core.FragmentShaderPath = "shaders/3d/main.dsf";

        Core.TextVertexShaderPath = "shaders/2d/text.dsv";
        Core.TextFragmentShaderPath = "shaders/2d/text.dsf";

        Core.Start();
    }
}