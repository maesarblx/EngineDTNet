using OpenTK.Graphics.OpenGL4;
using System.Numerics;
//using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace EngineDNet;

public static class Core
{
    private static readonly GameWindow Window = new(
        GameWindowSettings.Default,
        new NativeWindowSettings()
        {
            NumberOfSamples = 4
        }
    );

    private static string _vertexShaderPath = null!;
    private static string _fragmentShaderPath = null!;

    private static string _textVertexShaderPath = null!;
    private static string _textFragmentShaderPath = null!;

    private static Shader _shader = null!;
    private static Shader _textShader = null!;
    private static Camera _camera = null!;
    private static CameraController _cameraController = null!;
    private static Scene _curScene = null!;

    private static Text2D _fpsText = null!;

    public static Random Rand = new();
    public static MonitorInfoData CurrentMonitor = MonitorUtils.GetAllMonitors()[0];
    public static float ElapsedTime = 0f;

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

    public static string VertexShaderPath
    {
        get => _vertexShaderPath;
        set => _vertexShaderPath = value;
    }

    public static string FragmentShaderPath
    {
        get => _fragmentShaderPath;
        set => _fragmentShaderPath = value;
    }

    public static string TextVertexShaderPath
    {
        get => _textVertexShaderPath;
        set => _textVertexShaderPath = value;
    }

    public static string TextFragmentShaderPath
    {
        get => _textFragmentShaderPath;
        set => _textFragmentShaderPath = value;
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

    public static GameObject LoadObject(string Name, Vector3 Position, Vector3 Rotation, Vector3 Scale)
    {
        var Mesh = new Mesh3D(MeshLoader.Load($"models/{Name}.obj"));
        var Texture = new Texture2D($"textures/{Name}.png");
        var Object = new GameObject(Name, Position, Rotation, Scale, Mesh, Texture);

        return Object;
    }


    private static void WindowOnLoad()
    {
        var MapName = "Testplate";

        _shader = new(File.ReadAllText(_vertexShaderPath), File.ReadAllText(_fragmentShaderPath));
        _textShader = new(File.ReadAllText(_textVertexShaderPath), File.ReadAllText(_textFragmentShaderPath));
        _camera = new();
        _curScene = new();
        _cameraController = new(_camera);

        Object2DRenderer.TextShader = _textShader;
        var fontMesh = new FontMesh("fonts/Axiforma-Regular.ttf");

        {
            var tex = new Text2D(new Vector2(45.0f, 400.0f), Vector2.One, 0.0f, fontMesh)
            {
                Color = new Vector3(1.0f, 1.0f, 1.0f),
                Text = "<FPS>",
            };
            _fpsText = tex;
        }

        GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
        GL.Enable(EnableCap.Multisample);
        GL.Enable(EnableCap.LineSmooth);
        GL.Enable(EnableCap.PolygonSmooth);
        GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
        GL.Hint(HintTarget.PolygonSmoothHint, HintMode.Nicest);

        Window.CursorState = CursorState.Grabbed;
        Window.CenterWindow();

        var mapJsonRaw = File.ReadAllText($"maps/{MapName}.json");
        var objects = MapLoader.Load(mapJsonRaw);
        foreach (var obj in objects)
        {
            Core.CurrentScene.Root.AddChild(obj);
        }
    }

    private static void WindowOnRenderFrame(FrameEventArgs e)
    {
        ElapsedTime += (float)e.Time;

        CurrentCamera.Aspect = (float)Window.Size.X / Window.Size.Y;

        _cameraController.Update((Vector2)Window.MouseState.Delta, (float)e.Time, Window.IsKeyDown(Keys.W), Window.IsKeyDown(Keys.A), Window.IsKeyDown(Keys.S), Window.IsKeyDown(Keys.D), Window.IsKeyDown(Keys.E), Window.IsKeyDown(Keys.Q));

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        CurrentScene.Update((float)e.Time);
        foreach (var v in CurrentScene.Root.Children)
        {
            GameObjectRenderer.Render(v, _shader, _camera, CurrentScene.SceneLightingSettings);
        }
        Object2DRenderer.Render(_fpsText, _textShader, Window.ClientSize.X, Window.ClientSize.Y);

        Window.SwapBuffers();
    }

    private static void WindowOnUpdateFrame(FrameEventArgs e)
    {
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
