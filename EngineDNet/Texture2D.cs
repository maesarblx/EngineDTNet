using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using OpenTK.Graphics.OpenGL4;

namespace EngineDNet;
public class Texture2D
{
    public int Width { get; set; }
    public int Height { get; set; }
    private int _textureId;
    private static Dictionary<string, Texture2D> _cachedTextures = new();
    public Texture2D(string path)
    {
        if (!File.Exists(path))
        {
            Utils.ColoredWriteLine($"[Texture2D] Texture at path {path} does not exist! Using missing texture", ConsoleColor.Yellow);
            path = "./Textures/missing.png";
        }

        if (!File.Exists(path))
        {
            Utils.ColoredWriteLine($"[Texture2D] missing missing texture, fuck you", ConsoleColor.Red);
            return;
        }

        using var image = Image.Load<Rgba32>(path);

        Width = image.Width;
        Height = image.Height;

        var pixelArray = new Rgba32[Width * Height];
        image.CopyPixelDataTo(pixelArray);

        var byteSpan = MemoryMarshal.AsBytes(pixelArray.AsSpan());

        _textureId = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, _textureId);

        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, byteSpan.ToArray());
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

        GL.BindTexture(TextureTarget.Texture2D, 0);
    }

    public void Bind(TextureUnit unit)
    {
        GL.ActiveTexture(unit);
        GL.BindTexture(TextureTarget.Texture2D, _textureId);
    }

    public static Texture2D Load(string path)
    {
        if (_cachedTextures.ContainsKey(path)) return _cachedTextures[path];
        var texture = new Texture2D(path);
        _cachedTextures[path] = texture;
        return texture;
    }
}