using ImageMagick;

namespace BulkImagesOptimizer;

internal static class WebpCodec
{
    public static void Decode(string source, Stream destination)
    {
        // Read only the first frame, matching the other image formats.
        using var image = new MagickImage(source, new MagickReadSettings { FrameIndex = 0, FrameCount = 1 });
        image.AutoOrient();
        image.Strip();
        image.Write(destination, MagickFormat.Png);
    }

    public static void Encode(Stream source, Stream destination, int quality)
    {
        using var image = new MagickImage(source);
        image.Strip();
        image.Quality = (uint)quality;
        image.Write(destination, MagickFormat.WebP);
    }
}
