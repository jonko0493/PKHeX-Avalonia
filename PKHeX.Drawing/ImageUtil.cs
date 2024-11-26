using System;
using System.Net.Mime;
using System.Runtime.InteropServices;
using SkiaSharp;

namespace PKHeX.Drawing;

/// <summary>
/// Image Layering/Blending Utility
/// </summary>
public static class ImageUtil
{
    public static SKBitmap LayerImage(SKBitmap baseLayer, SKBitmap overLayer, int x, int y, double transparency)
    {
        overLayer = ChangeOpacity(overLayer, transparency);
        return LayerImage(baseLayer, overLayer, x, y);
    }

    public static SKBitmap LayerImage(SKBitmap baseLayer, SKBitmap overLayer, int x, int y)
    {
        SKBitmap img = new(baseLayer.Info);
        SKCanvas canvas = new(img);
        canvas.DrawBitmap(overLayer, x, y);
        return img;
    }

    public static SKBitmap ChangeOpacity(SKBitmap img, double trans)
    {
        var bmp = img.Copy();
        GetBitmapData(bmp, out SKImageInfo bmpData, out var ptr, out byte[] data);

        Marshal.Copy(ptr, data, 0, data.Length);
        SetAllTransparencyTo(data, trans);
        Marshal.Copy(data, 0, ptr, data.Length);

        return bmp;
    }

    public static SKBitmap ChangeAllColorTo(SKBitmap img, SKColor c)
    {
        var bmp = img.Copy();
        GetBitmapData(bmp, out SKImageInfo bmpData, out var ptr, out byte[] data);

        Marshal.Copy(ptr, data, 0, data.Length);
        ChangeAllColorTo(data, c);
        Marshal.Copy(data, 0, ptr, data.Length);

        return bmp;
    }

    public static SKBitmap ChangeTransparentTo(SKBitmap img, SKColor c, byte trans, int start = 0, int end = -1)
    {
        var bmp = img.Copy();
        GetBitmapData(bmp, out SKImageInfo bmpData, out var ptr, out byte[] data);

        Marshal.Copy(ptr, data, 0, data.Length);
        if (end == -1)
            end = data.Length - 4;
        SetAllTransparencyTo(data, c, trans, start, end);
        Marshal.Copy(data, 0, ptr, data.Length);

        return bmp;
    }

    public static SKBitmap BlendTransparentTo(SKBitmap img, SKColor c, byte trans, int start = 0, int end = -1)
    {
        var bmp = img.Copy();
        GetBitmapData(bmp, out SKImageInfo bmpData, out var ptr, out byte[] data);

        Marshal.Copy(ptr, data, 0, data.Length);
        if (end == -1)
            end = data.Length - 4;
        BlendAllTransparencyTo(data, c, trans, start, end);
        Marshal.Copy(data, 0, ptr, data.Length);

        return bmp;
    }

    public static SKBitmap WritePixels(SKBitmap img, SKColor c, int start, int end)
    {
        var bmp = img.Copy();
        GetBitmapData(bmp, out SKImageInfo bmpData, out var ptr, out byte[] data);

        Marshal.Copy(ptr, data, 0, data.Length);
        ChangeAllTo(data, c, start, end);
        Marshal.Copy(data, 0, ptr, data.Length);

        return bmp;
    }

    public static SKBitmap ToGrayscale(SKBitmap img)
    {
        var bmp = img.Copy();
        GetBitmapData(bmp, out SKImageInfo bmpData, out var ptr, out byte[] data);

        Marshal.Copy(ptr, data, 0, data.Length);
        SetAllColorToGrayScale(data);
        Marshal.Copy(data, 0, ptr, data.Length);

        return bmp;
    }

    private static void GetBitmapData(SKBitmap bmp, out SKImageInfo bmpData, out nint ptr, out byte[] data)
    {
        bmpData = new(bmp.Width, bmp.Height, bmp.ColorType);
        ptr = bmp.GetPixels();
        data = new byte[bmp.Width * bmp.Height * 4];
    }

    public static SKBitmap GetBitmap(byte[] data, int width, int height, int length, SKColorType format = SKColorType.Rgba8888)
    {
        var bmp = new SKBitmap(new(width, height, format));
        Marshal.Copy(data, 0, bmp.GetPixels(), length);
        return bmp;
    }

    public static SKBitmap GetBitmap(byte[] data, int width, int height, SKColorType format = SKColorType.Rgba8888)
    {
        return GetBitmap(data, width, height, data.Length, format);
    }

    public static byte[] GetPixelData(SKBitmap bitmap)
    {
        var argbData = new byte[bitmap.Width * bitmap.Height * 4];
        Marshal.Copy(bitmap.GetPixels(), argbData, 0, bitmap.Width * bitmap.Height * 4);
        return argbData;
    }

    public static void SetAllUsedPixelsOpaque(Span<byte> data)
    {
        for (int i = 0; i < data.Length; i += 4)
        {
            if (data[i + 3] != 0)
                data[i + 3] = 0xFF;
        }
    }

    public static void RemovePixels(Span<byte> pixels, ReadOnlySpan<byte> original)
    {
        var arr = MemoryMarshal.Cast<byte, uint>(pixels);
        for (int i = original.Length - 4; i >= 0; i -= 4)
        {
            if (original[i + 3] != 0)
                arr[i >> 2] = 0;
        }
    }

    private static void SetAllTransparencyTo(Span<byte> data, double trans)
    {
        for (int i = 0; i < data.Length; i += 4)
            data[i + 3] = (byte)(data[i + 3] * trans);
    }

    public static void SetAllTransparencyTo(Span<byte> data, SKColor c, byte trans, int start, int end)
    {
        var arr = MemoryMarshal.Cast<byte, uint>(data);
        var value = (uint)new SKColor(c.Red, c.Green, c.Blue, trans);
        for (int i = end; i >= start; i -= 4)
        {
            if (data[i + 3] == 0)
                arr[i >> 2] = value;
        }
    }

    public static void BlendAllTransparencyTo(Span<byte> data, SKColor c, byte trans, int start, int end)
    {
        var arr = MemoryMarshal.Cast<byte, uint>(data);
        var value = (uint)new SKColor(trans, c.Red, c.Green, c.Blue);
        for (int i = end; i >= start; i -= 4)
        {
            var alpha = data[i + 3];
            if (alpha == 0)
                arr[i >> 2] = value;
            else if (alpha != 0xFF)
                arr[i >> 2] = BlendColor(arr[i >> 2], value);
        }
    }

    // heavily favor second (new) color
    private static uint BlendColor(uint color1, uint color2, double amount = 0.2)
    {
        var a1 = (color1 >> 24) & 0xFF;
        var r1 = (color1 >> 16) & 0xFF;
        var g1 = (color1 >> 8) & 0xFF;
        var b1 = color1 & 0xFF;

        var a2 = (color2 >> 24) & 0xFF;
        var r2 = (color2 >> 16) & 0xFF;
        var g2 = (color2 >> 8) & 0xFF;
        var b2 = color2 & 0xFF;

        byte a = (byte)((a1 * amount) + (a2 * (1 - amount)));
        byte r = (byte)((r1 * amount) + (r2 * (1 - amount)));
        byte g = (byte)((g1 * amount) + (g2 * (1 - amount)));
        byte b = (byte)((b1 * amount) + (b2 * (1 - amount)));

        return (uint)((a << 24) | (r << 16) | (g << 8) | b);
    }

    public static void ChangeAllTo(Span<byte> data, SKColor c, int start, int end)
    {
        var arr = MemoryMarshal.Cast<byte, uint>(data[start..end]);
        var value = (uint)c;
        arr.Fill(value);
    }

    public static void ChangeAllColorTo(Span<byte> data, SKColor c)
    {
        byte R = c.Red;
        byte G = c.Green;
        byte B = c.Blue;
        for (int i = 0; i < data.Length; i += 4)
        {
            if (data[i + 3] == 0)
                continue;
            data[i + 0] = B;
            data[i + 1] = G;
            data[i + 2] = R;
        }
    }

    private static void SetAllColorToGrayScale(Span<byte> data)
    {
        for (int i = 0; i < data.Length; i += 4)
        {
            if (data[i + 3] == 0)
                continue;
            byte greyS = (byte)((0.3 * data[i + 2]) + (0.59 * data[i + 1]) + (0.11 * data[i + 0]));
            data[i + 0] = greyS;
            data[i + 1] = greyS;
            data[i + 2] = greyS;
        }
    }

    public static void GlowEdges(Span<byte> data, byte blue, byte green, byte red, int width, int reach = 3, double amount = 0.0777)
    {
        PollutePixels(data, width, reach, amount);
        CleanPollutedPixels(data, blue, green, red);
    }

    private const int PollutePixelColorIndex = 0;

    private static void PollutePixels(Span<byte> data, int width, int reach, double amount)
    {
        int stride = width * 4;
        int height = data.Length / stride;
        for (int i = 0; i < data.Length; i += 4)
        {
            // only pollute outwards if the current pixel is fully opaque
            if (data[i + 3] == 0)
                continue;

            int x = (i % stride) / 4;
            int y = (i / stride);
            {
                int left = Math.Max(0, x - reach);
                int right = Math.Min(width - 1, x + reach);
                int top = Math.Max(0, y - reach);
                int bottom = Math.Min(height - 1, y + reach);
                for (int ix = left; ix <= right; ix++)
                {
                    for (int iy = top; iy <= bottom; iy++)
                    {
                        // update one of the color bits
                        // it is expected that a transparent pixel RGBA value is 0.
                        var c = 4 * (ix + (iy * width));
                        ref var b = ref data[c + PollutePixelColorIndex];
                        b += (byte)(amount * (0xFF - b));
                    }
                }
            }
        }
    }

    private static void CleanPollutedPixels(Span<byte> data, byte blue, byte green, byte red)
    {
        for (int i = 0; i < data.Length; i += 4)
        {
            // only clean if the current pixel isn't transparent
            if (data[i + 3] != 0)
                continue;

            // grab the transparency from the donor byte
            var transparency = data[i + PollutePixelColorIndex];
            if (transparency == 0)
                continue;

            data[i + 0] = blue;
            data[i + 1] = green;
            data[i + 2] = red;
            data[i + 3] = transparency;
        }
    }
}
