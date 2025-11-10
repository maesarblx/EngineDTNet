using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Numerics;

namespace EngineDNet;

public static class Core
{
    private static readonly GameWindow Window = new(
        GameWindowSettings.Default,
        new NativeWindowSettings()
        {
            NumberOfSamples = 0,
        }
    );

    private static Camera _camera = null!;
    private static CameraController _cameraController = null!;
    private static Scene _curScene = null!;

    private static Framebuffer _fbuffer = null!;
    private static Rect2D _screenRect = null!;

    private static Text2D _fpsText = null!;

    private static Dictionary<string, Shader> _shaders = null!;

    public static Random Rand = new();
    public static MonitorInfoData CurrentMonitor = MonitorUtils.GetAllMonitors()[0];
    public static float ElapsedTime = 0f;
    public static string MapName = "Testplate";

    public static Dictionary<string, string> ShaderPaths = null!;

    public static Player? CurrentPlayer;

    public static OpenTK.Mathematics.Vector2i WindowSize
    {
        get => Window.Size;
        set => Window.Size = value;
    }

    public static Camera CurrentCamera
    {
        get => _camera;
        private set => _camera = value;
    }

    public static Scene CurrentScene
    {
        get => _curScene;
        private set => _curScene = value;
    }

    public static string Title
    {
        get => Window.Title;
        set => Window.Title = value;
    }

    public static VSyncMode VSync
    {
        get => Window.VSync;
        set => Window.VSync = value;
    }

    public static GameObject LoadObject(string name, Vector3 position, Vector3 rotation, Vector3 scale, GameObject? parent, float? mass = null)
    {
        var mesh = Mesh3D.Load(MeshLoader.Load($"models/{name}.obj"));
        var texture = Texture2D.Load($"textures/{name}.png");
        var gameObject = new GameObject(name, position, rotation, scale, mesh, texture, parent, mass);

        return gameObject;
    }

    public static bool IsKeyDown(Keys Key)
    {
        return Window.IsKeyDown(Key);
    }

    public static bool IsKeyPressed(Keys Key)
    {
        return Window.IsKeyPressed(Key);
    }

    private static void render3D()
    {
        if (CurrentScene.Skybox != null)
            GameObjectRenderer.Render(CurrentScene.Skybox.Object, _shaders["Main3D"], _camera, CurrentScene.SceneLightingSettings);
        foreach (var v in CurrentScene.Root.Children)
        {
            GameObjectRenderer.Render(v, _shaders["Main3D"], _camera, CurrentScene.SceneLightingSettings);
        }
    }

    public static void LoadMap(string name)
    {
        var mapJsonRaw = File.ReadAllText($"maps/{name}.dnm");
        var objects = MapLoader.Load(mapJsonRaw);
        foreach (var obj in objects)
        {
            CurrentScene.Root.Children.Add(obj);
        }
    }

    private static void WindowOnLoad()
    {
        _shaders = new()
        {
            ["Main3D"] = new(File.ReadAllText($"{ShaderPaths["3D"]}/main.dsv"), File.ReadAllText($"{ShaderPaths["3D"]}/main.dsf")),
            ["Text"] = new(File.ReadAllText($"{ShaderPaths["2D"]}/text.dsv"), File.ReadAllText($"{ShaderPaths["2D"]}/text.dsf")),
            ["PostFX"] = new(File.ReadAllText($"{ShaderPaths["SH"]}/postfx.dsv"), File.ReadAllText($"{ShaderPaths["SH"]}/postfx.dsf")),
        };

        _fbuffer = new();

        _screenRect = new(Vector2.Zero, Vector2.Zero, 0);

        _curScene = new();

        _camera = new();
        _cameraController = new(_camera);

        CurrentPlayer = new();

        Object2DRenderer.RectMesh = new Mesh2D([
            // Top Left
            new Mesh2D.Vertex(new Vector2(-1.0f,1.0f), new Vector2(0.0f, 1.0f)),
            // Top Right
            new Mesh2D.Vertex(new Vector2(1.0f,1.0f), new Vector2(1.0f, 1.0f)),
            // Bottom Left
            new Mesh2D.Vertex(new Vector2(-1.0f,-1.0f), new Vector2(0.0f, 0.0f)),
            // Bottom Right
            new Mesh2D.Vertex(new Vector2(1.0f,-1.0f), new Vector2(1.0f, 0.0f)),
        ], [
            0, 1, 3,
            0, 2, 3
        ]);

        Object2DRenderer.TextShader = _shaders["Text"];
        var fontMesh = new FontMesh("fonts/Pixel.otf");

        {
            var tex = new Text2D(new Vector2(45.0f, 400.0f), Vector2.One * 0.5f, 0.0f, fontMesh)
            {
                Color = new Vector3(1.0f, 1.0f, 1.0f),
                Text = "<FPS>",
            };
            _fpsText = tex;
        }

        GL.ClearColor(0f, 0f, 0f, 1.0f);

        GL.Enable(EnableCap.Multisample);
        GL.Enable(EnableCap.LineSmooth);
        GL.Enable(EnableCap.PolygonSmooth);

        GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
        GL.Hint(HintTarget.PolygonSmoothHint, HintMode.Nicest);

        _shaders["PostFX"].SetUniform("saturation", _curScene.SceneLightingSettings.Saturation);
        _shaders["PostFX"].SetUniform("contrast", _curScene.SceneLightingSettings.Contrast);
        _shaders["PostFX"].SetUniform("brightness", _curScene.SceneLightingSettings.Brightness);
        _shaders["PostFX"].SetUniform("exposure", _curScene.SceneLightingSettings.Exposure);
        _shaders["PostFX"].SetUniform("tint", _curScene.SceneLightingSettings.Tint);
        _shaders["PostFX"].SetUniform("gamma", Settings.Gamma);

        Window.CursorState = CursorState.Grabbed;
        Window.CenterWindow();

        _curScene.Init();
        LoadMap(MapName);
    }

    private static void WindowOnRenderFrame(FrameEventArgs e)
    {
        var dt = (float)e.Time;

        ElapsedTime += dt;

        CurrentCamera.Aspect = (float)Window.Size.X / Window.Size.Y;

        CurrentPlayer?.ProcessInput(dt);
        CurrentPlayer?.RenderUpdate(dt);

        _cameraController?.Update((Vector2)Window.MouseState.Delta);

        if (_cameraController != null)
            CurrentScene.RenderUpdate(_cameraController.Camera);

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        CurrentScene.Update(dt);
        _fbuffer.Draw(render3D, Window.ClientSize.X, Window.ClientSize.Y);
        Object2DRenderer.Render(_screenRect, _shaders["PostFX"], _fbuffer.TextureID);
        Object2DRenderer.Render(_fpsText, _shaders["Text"], Window.ClientSize.X, Window.ClientSize.Y);
        Window.SwapBuffers();
    }

    private static void WindowOnUpdateFrame(FrameEventArgs e)
    {
        CurrentPlayer?.FixedUpdate((float)e.Time);
        _fpsText.Text = $"FPS: {Math.Floor(1.0f / e.Time)}";
        if (Window.IsKeyDown(Keys.Escape))
        {
            Window.Close();
        }
    }

    private static void WindowOnResize(ResizeEventArgs e)
    {
        GL.Viewport(0, 0, Window.ClientSize.X, Window.ClientSize.Y);
    }

    public static void Start()
    {
        Window.Load += WindowOnLoad;
        Window.RenderFrame += WindowOnRenderFrame;
        Window.Resize += WindowOnResize;
        Window.UpdateFrame += WindowOnUpdateFrame;
        Window.Run();
    }
}