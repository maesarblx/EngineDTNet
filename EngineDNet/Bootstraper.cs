using EngineDNet.Utilities;
using OpenTK.Windowing.Common;
using System.Drawing;
using System.Windows.Forms;
using EngineDNet.Assets;

namespace EngineDNet;
public class Bootstraper
{
    public static void Main(string[] args)
    {
        Screen primaryScreen = Screen.PrimaryScreen!;
        if (primaryScreen == null)
            return;
        Rectangle screenBounds = primaryScreen.Bounds!;
        Core.WindowSize = new OpenTK.Mathematics.Vector2i(screenBounds.Width / 2, screenBounds.Height / 2);
        Core.Title = "EngineDNet";
        Core.VSync = VSyncMode.Off;

        Core.ShaderSources = new()
        {
            ["3D"] = new()
            {
                ["Main"] = new() { EmbeddedShaderLoader.LoadShaderByRelativePath("3D/main.dsf"), EmbeddedShaderLoader.LoadShaderByRelativePath("3D/main.dsv") },
                ["ShadowMap"] = new() { EmbeddedShaderLoader.LoadShaderByRelativePath("3D/shadowmap.dsf"), EmbeddedShaderLoader.LoadShaderByRelativePath("3D/shadowmap.dsv") },
            },
            ["2D"] = new()
            {
                ["Text"] = new() { EmbeddedShaderLoader.LoadShaderByRelativePath("2D/text.dsf"), EmbeddedShaderLoader.LoadShaderByRelativePath("2D/text.dsv") },
            },
            ["SH"] = new()
            {
                ["PostFX"] = new() { EmbeddedShaderLoader.LoadShaderByRelativePath("SH/postfx.dsf"), EmbeddedShaderLoader.LoadShaderByRelativePath("SH/postfx.dsv") },
            },
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