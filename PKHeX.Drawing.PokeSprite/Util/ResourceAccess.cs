using System.Reflection;
using SkiaSharp;

namespace PKHeX.Drawing.PokeSprite
{
    public static class ResourceAccess
    {
        public static SKBitmap LoadResource(string path, SKBitmap failureBitmap = null)
        {
            try
            {
                using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
                return SKBitmap.Decode(stream);
            }
            catch
            {
                return failureBitmap;
            }
        }
    }
}
