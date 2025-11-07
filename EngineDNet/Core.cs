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

    public static GameObject LoadObject(string name, Vector3 position, Vector3 rotation, Vector3 scale, GameObject parent, float? mass = null)
    {
        var mesh = new Mesh3D(MeshLoader.Load($"models/{name}.obj"));
        var texture = new Texture2D($"textures/{name}.png");
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
        foreach (var v in CurrentScene.Root.Children)
        {
            GameObjectRenderer.Render(v, _shader, _camera, CurrentScene.SceneLightingSettings);
        }
    }

    public static void LoadMap(string name)
    {
        var mapJsonRaw = File.ReadAllText($"maps/{name}.json");
        var objects = MapLoader.Load(mapJsonRaw);
        foreach (var obj in objects)
        {
            CurrentScene.Root.Children.Add(obj);
        }
    }

    private static void WindowOnLoad()
    {
        _shader = new(File.ReadAllText(_vertexShaderPath), File.ReadAllText(_fragmentShaderPath));
        _textShader = new(File.ReadAllText(_textVertexShaderPath), File.ReadAllText(_textFragmentShaderPath));
        _camera = new();
        _curScene = new();
        _cameraController = new(_camera);
        CurrentPlayer = new();

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

        GL.ClearColor(0f, 0f, 0f, 1.0f);
        GL.Enable(EnableCap.Multisample);
        GL.Enable(EnableCap.LineSmooth);
        GL.Enable(EnableCap.PolygonSmooth);
        GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
        GL.Hint(HintTarget.PolygonSmoothHint, HintMode.Nicest);

        Window.CursorState = CursorState.Grabbed;
        Window.CenterWindow();

        _curScene.Init();
        LoadMap("Testplate");
    }

    private static void WindowOnRenderFrame(FrameEventArgs e)
    {
        ElapsedTime += (float)e.Time;

        CurrentCamera.Aspect = (float)Window.Size.X / Window.Size.Y;

        CurrentPlayer?.ProcessInput();

        _cameraController?.Update((Vector2)Window.MouseState.Delta, (float)e.Time);

        if (_cameraController != null)
            CurrentScene.RenderUpdate(_cameraController.Camera);

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        CurrentScene.Update((float)e.Time);
        render3D();
        Object2DRenderer.Render(_fpsText, _textShader, Window.ClientSize.X, Window.ClientSize.Y);
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
