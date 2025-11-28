using EngineDNet.Rendering;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Numerics;

namespace EngineDNet.Graphics;
public class ShadowMap : IDisposable // fix later brah
{
    private int _depthMapFBO;
    public int DepthMap { get; private set; }
    public int ShadowWidth { get; private set; } = 1024;
    public int ShadowHeight { get; private set; } = 1024;
    public Action RenderScene;

    public ShadowMap(Action RenderSceneAction, int size = 1024)
    {
        RenderScene = RenderSceneAction;
        ShadowWidth = ShadowHeight = size;

        _depthMapFBO = GL.GenFramebuffer();
        DepthMap = GL.GenTexture();

        GL.BindTexture(TextureTarget.Texture2D, DepthMap);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent24, ShadowWidth, ShadowHeight, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
        float[] border = new float[] { 1f, 1f, 1f, 1f };
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, border);

        GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName)All.TextureCompareMode, (int)All.CompareRefToTexture);
        GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName)All.TextureCompareFunc, (int)All.Lequal);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _depthMapFBO);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, DepthMap, 0);

        GL.DrawBuffer(DrawBufferMode.None);
        GL.ReadBuffer(ReadBufferMode.None);

        var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        if (status != FramebufferErrorCode.FramebufferComplete)
            Console.WriteLine($"[ShadowMap] Depth FBO incomplete: {status}");

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GL.BindTexture(TextureTarget.Texture2D, 0);
    }

    public void Render()
    {
        int[] oldViewport = new int[4];
        GL.GetInteger(GetPName.Viewport, oldViewport);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _depthMapFBO);
        GL.Viewport(0, 0, ShadowWidth, ShadowHeight);

        GL.Enable(EnableCap.DepthTest);
        GL.ClearDepth(1.0);
        GL.Clear(ClearBufferMask.DepthBufferBit);

        RenderScene?.Invoke();

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GL.Viewport(oldViewport[0], oldViewport[1], oldViewport[2], oldViewport[3]);
    }

    public void Dispose()
    {
        if (_depthMapFBO != 0) GL.DeleteFramebuffer(_depthMapFBO);
        if (DepthMap != 0) GL.DeleteTexture(DepthMap);
        _depthMapFBO = 0;
        DepthMap = 0;
    }
}