using OpenTK.Windowing.Common;
using System.IO.Enumeration;

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

        if (args.Length > 0)
            Core.MapName = args[0];
        else if (!File.Exists("./dnafm.d"))
        {
            Utils.ColoredWriteLine("Current maps available:", ConsoleColor.Yellow);
            foreach (string s in Directory.GetFiles("./maps"))
            {
                Utils.ColoredWriteLine($"* {Path.GetFileNameWithoutExtension(s)}", ConsoleColor.DarkGreen);
            }
            Console.Write("Map: ");
            var newMapName = Console.ReadLine();
            if (newMapName != null)
                Core.MapName = newMapName;
        }

        Core.Start();
    }
}