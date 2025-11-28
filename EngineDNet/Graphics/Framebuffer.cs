using OpenTK.Graphics.OpenGL4;
using System;

namespace EngineDNet.Rendering;
 
public class Framebuffer : IDisposable
{
    private int _fbo;
    public int TextureID { get; private set; }
    private int _rbo;

    public Framebuffer()
    {
        _fbo = GL.GenFramebuffer();
        TextureID = GL.GenTexture();
        _rbo = 0;
    }

    public void Draw(Action renderable, int width, int height)
    {
        int[] oldViewport = new int[4];
        GL.GetInteger(GetPName.Viewport, oldViewport);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _fbo);

        GL.BindTexture(TextureTarget.Texture2D, TextureID);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, nint.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, TextureID, 0);

        GL.DrawBuffers(1, new DrawBuffersEnum[] { DrawBuffersEnum.ColorAttachment0 });

        if (_rbo == 0) _rbo = GL.GenRenderbuffer();
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _rbo);
        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, width, height);
        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, _rbo);

        FramebufferErrorCode status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        if (status != FramebufferErrorCode.FramebufferComplete)
        {
            Console.WriteLine($"[Framebuffer] Incomplete framebuffer: {status}");
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
            GL.Viewport(oldViewport[0], oldViewport[1], oldViewport[2], oldViewport[3]);
            return;
        }

        GL.Viewport(0, 0, width, height);

        GL.ClearColor(0f, 0f, 0f, 1f);
        GL.Enable(EnableCap.DepthTest);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        renderable();

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GL.BindTexture(TextureTarget.Texture2D, 0);
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);

        GL.Viewport(oldViewport[0], oldViewport[1], oldViewport[2], oldViewport[3]);
    }

    private void DisposeUnmanagedResources(bool disposed)
    {
        if (_fbo != 0) GL.DeleteFramebuffer(_fbo);
        if (TextureID != 0) GL.DeleteTexture(TextureID);
        if (_rbo != 0) GL.DeleteRenderbuffer(_rbo);
        _fbo = 0;
        TextureID = 0;
        _rbo = 0;
    }

    ~Framebuffer()
    {
        DisposeUnmanagedResources(false);
    }

    public void Dispose()
    {
        DisposeUnmanagedResources(true);
        GC.SuppressFinalize(this);
    }
}
