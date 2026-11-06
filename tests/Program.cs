using BulkImagesOptimizer;
using ImageMagick;

static void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}

Check(OutputFormat.ResolveExtension("input.png", 3) == ".webp", "WebP conversion mode");
Check(OutputFormat.ResolveExtension("input.WeBp", 2) == ".WeBp", "Original extension case");
Check(OutputFormat.ResolveExtension("input.webp", 0) == ".jpg", "WebP to JPEG");
Check(OutputFormat.ResolveExtension("input.webp", 1) == ".png", "WebP to PNG");
try { OutputFormat.ResolveExtension("input.gif", 2); throw new Exception("Unsupported original accepted"); }
catch (NotSupportedException) { }

string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".webp");
try
{
    using var source = new MagickImage(MagickColors.Transparent, 32, 16);
    using var png = new MemoryStream();
    source.Write(png, MagickFormat.Png);
    foreach (int quality in new[] { 1, 85, 100 })
    {
        png.Position = 0;
        using (var webp = File.Create(path)) WebpCodec.Encode(png, webp, quality);
        using var encoded = new MagickImage(path);
        Check(encoded.Format == MagickFormat.WebP, "Actual WebP encoding");
        using var decoded = new MemoryStream();
        WebpCodec.Decode(path, decoded);
        decoded.Position = 0;
        using var image = new MagickImage(decoded);
        Check(image.Width == 32 && image.Height == 16, "Round-trip dimensions");
        Check(image.HasAlpha, "Alpha retained");
        using var pixels = image.GetPixels();
        Check(pixels.GetPixel(0, 0).GetChannel(3) == 0, "Transparent pixel retained");
    }
    Console.WriteLine("PASS: format routing and WebP round trips at quality 1, 85, 100; dimensions and transparency preserved.");
}
finally { File.Delete(path); }
