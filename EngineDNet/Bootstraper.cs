using EngineDNet.Utilities;
using OpenTK.Windowing.Common;
using System.Windows.Forms;

namespace EngineDNet;
public class Bootstraper
{
    public static void Main(string[] args)
    {
        var primaryScreen = Screen.PrimaryScreen!;
        if (primaryScreen == null)
            return;
        var screenBounds = primaryScreen.Bounds!;
        Core.WindowSize = new OpenTK.Mathematics.Vector2i(screenBounds.Width / 2, screenBounds.Height / 2);
        Core.Title = "EngineDNet";
        Core.VSync = VSyncMode.Off;

        Core.ShaderPaths = new Dictionary<string, string>() 
        {
            ["3D"] = "shaders/3d",
            ["2D"] = "shaders/2d",
            ["SH"] = "shaders/sh",
        };

        if (args.Length > 0)
            Core.MapName = args[0];
        else if (!File.Exists("dnafm.d"))
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