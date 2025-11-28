using EngineDNet.Assets;
using EngineDNet.Assets.Loaders;
using EngineDNet.Assets.Meshes;
using EngineDNet.Assets.Textures;
using EngineDNet.Camera;
using EngineDNet.Graphics;
using EngineDNet.Graphics.Renderers;
using EngineDNet.Objects;
using EngineDNet.Rendering;
using EngineDNet.Workspace;
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

    private static Camera3D _camera = null!;
    private static CameraController3D _cameraController = null!;
    private static Scene _curScene = null!;
    private static ShadowMap _shadowMap = null!;

    private static Framebuffer _fbuffer = null!;
    private static Rect2D _screenRect = null!;

    private static Text2D _fpsText = null!;

    private static Dictionary<string, Shader> _shaders = null!;

    public static Random Rand = new();
    public static float ElapsedTime = 0f;
    public static string MapName = "Testplate";

    public static AssetManager AssetManager = null!;

    public static Dictionary<string, Dictionary<string, List<string>>> ShaderSources = null!;

    public static Player? CurrentPlayer;

    public static float FrameTime;

    private static Matrix4x4 _lightSpaceMatrix = Matrix4x4.Identity;

    public static OpenTK.Mathematics.Vector2i WindowSize
    {
        get => Window.Size;
        set => Window.Size = value;
    }

    public static Camera3D CurrentCamera
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
        var mesh = Mesh3D.Load(AssetManager.Load<List<float>>($"models/{name}.obj"));
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

    private static void Render3D()
    {
        if (CurrentScene.Skybox != null)
            GameObjectRenderer.Render(CurrentScene.Skybox.Object, _shaders["Main3D"], _camera, CurrentScene.SceneLightingSettings);
        foreach (var v in CurrentScene.Root.Children)
        {
            GameObjectRenderer.Render(v, _shaders["Main3D"], _camera, CurrentScene.SceneLightingSettings);
        }
    }

    private static void Render3DShadowMap()
    {
        var shader = _shaders["ShadowMap"];
        shader.Use();
        var nearZPlane = 1f;
        var farZPlane = 7.5f;
        var lightProjection = Matrix4x4.CreateOrthographicOffCenter(-10, 10, -10, 10, nearZPlane, farZPlane);
        var lightView = Matrix4x4.CreateLookAt(new Vector3(-2.0f, 4.0f, -1.0f), Vector3.Zero, Vector3.UnitY);
        var lightSpaceMatrix = lightProjection * lightView;

        _lightSpaceMatrix = lightSpaceMatrix;

        shader.SetUniform("lightSpaceMatrix", lightSpaceMatrix);
        foreach (var v in CurrentScene.Root.Children)
        {
            GameObjectRenderer.BasicRender(v, shader);
        }
    }

    public static void LoadMap(string name)
    {
        var objects = AssetManager.Load<List<GameObject>>($"maps/{name}.dnm");
        foreach (var obj in objects)
        {
            CurrentScene.Root.Children.Add(obj);
        }
    }

    private static void WindowOnLoad()
    {
        _shaders = new()
        {
            ["Main3D"] = new(ShaderSources["3D"]["Main"][1], ShaderSources["3D"]["Main"][0]),
            ["ShadowMap"] = new(ShaderSources["3D"]["ShadowMap"][1], ShaderSources["3D"]["ShadowMap"][0]),
            ["Text"] = new(ShaderSources["2D"]["Text"][1], ShaderSources["2D"]["Text"][0]),
            ["PostFX"] = new(ShaderSources["SH"]["PostFX"][1], ShaderSources["SH"]["PostFX"][0]),
        };

        _fbuffer = new();

        _screenRect = new(Vector2.Zero, Vector2.Zero, 0);

        _curScene = new();

        _camera = new();
        _cameraController = new(_camera);

        CurrentPlayer = new();

        var loaders = new IAssetLoader[] { new MeshLoader(), new MapLoader() };
        AssetManager = new(loaders);

        _shadowMap = new(Render3DShadowMap);

        Object2DRenderer.RectMesh = new Mesh2D([
            new Mesh2D.Vertex(new Vector2(-1.0f,1.0f), new Vector2(0.0f, 1.0f)),
            new Mesh2D.Vertex(new Vector2(1.0f,1.0f), new Vector2(1.0f, 1.0f)),
            new Mesh2D.Vertex(new Vector2(-1.0f,-1.0f), new Vector2(0.0f, 0.0f)),
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
        float dt = (float)e.Time;

        FrameTime = dt;
        ElapsedTime += dt;

        CurrentCamera.Aspect = (float)Window.Size.X / Window.Size.Y;

        CurrentPlayer?.ProcessInput(dt);
        CurrentPlayer?.RenderUpdate(dt);

        _cameraController?.Update((Vector2)Window.MouseState.Delta);

        if (_cameraController != null)
            CurrentScene.RenderUpdate(_cameraController.Camera);

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        CurrentScene.Update(dt);

        _fbuffer.Draw(Render3D, Window.ClientSize.X, Window.ClientSize.Y);

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