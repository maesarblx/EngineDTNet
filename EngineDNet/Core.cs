using OpenTK;
using OpenTK.Audio;
using OpenTK.Compute;
using OpenTK.Core;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using OpenTK.Mathematics;
using OpenTK.Platform;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Timers;
using System.Drawing;
using EngineDNet;

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
    private static Camera _camera = null!;
    private static Scene _curScene = null!;

    public static Random Rand = new Random();
    public static MonitorInfoData CurrentMonitor = MonitorUtils.GetAllMonitors()[0];
    public static float ElapsedTime = 0f;

    public static Vector2i WindowSize
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

    private static void WindowOnLoad()
    {
        _shader = new Shader(File.ReadAllText(_vertexShaderPath), File.ReadAllText(_fragmentShaderPath));
        _camera = new Camera();
        _curScene = new Scene();

        GL.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
        GL.Enable(EnableCap.Multisample);
        GL.Enable(EnableCap.LineSmooth);
        GL.Enable(EnableCap.PolygonSmooth);
        GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
        GL.Hint(HintTarget.PolygonSmoothHint, HintMode.Nicest);
    }

    private static void WindowOnRenderFrame(FrameEventArgs e)
    {
        ElapsedTime += (float)e.Time;

        CurrentCamera.Aspect = (float)Window.Size.X / Window.Size.Y;

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        foreach (var v in CurrentScene.Root.Children)
        {
            GameObjectRenderer.Render(v, _shader, _camera, CurrentScene.SceneLightingSettings);
        }

        Window.SwapBuffers();
    }

    private static void WindowOnUpdateFrame(FrameEventArgs e)
    {
        
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
