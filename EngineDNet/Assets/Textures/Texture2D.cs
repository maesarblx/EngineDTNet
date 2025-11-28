using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using OpenTK.Graphics.OpenGL4;
using EngineDNet.Utilities;

namespace EngineDNet.Assets.Textures;
public class Texture2D
{
    public int Width { get; set; }
    public int Height { get; set; }
    private int _textureId;
    private static Dictionary<string, Texture2D> _cachedTextures = new();
    private Texture2D(string path)
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

        using Image<Rgba32> image = Image.Load<Rgba32>(path);

        Width = image.Width;
        Height = image.Height;

        Rgba32[]? pixelArray = new Rgba32[Width * Height];
        image.CopyPixelDataTo(pixelArray);

        Span<byte> byteSpan = MemoryMarshal.AsBytes(pixelArray.AsSpan());

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
        Texture2D texture = new(path);
        _cachedTextures[path] = texture;
        return texture;
    }
}