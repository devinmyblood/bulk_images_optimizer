using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace BulkImagesOptimizer;

internal static class ImageExporter
{
    public static void Export(string source, string destination, int maxWidth, int maxHeight, long quality, int pngQuality, int outputMode, int webpQuality = 85)
    {
        string extension = OutputFormat.ResolveExtension(source, outputMode);
        bool png = extension.Equals(".png", StringComparison.OrdinalIgnoreCase);
        bool webp = extension.Equals(".webp", StringComparison.OrdinalIgnoreCase);
        bool jpeg = !png && !webp;
        using var image = LoadImage(source);
        // Apply camera orientation before calculating the bounding box.
        if (image.PropertyIdList.Contains(0x0112))
        {
            var value = image.GetPropertyItem(0x0112)?.Value;
            var rotation = value is { Length: >= 2 } ? BitConverter.ToUInt16(value, 0) : 1;
            image.RotateFlip(rotation switch
            {
                2 => RotateFlipType.RotateNoneFlipX, 3 => RotateFlipType.Rotate180FlipNone,
                4 => RotateFlipType.Rotate180FlipX, 5 => RotateFlipType.Rotate90FlipX,
                6 => RotateFlipType.Rotate90FlipNone, 7 => RotateFlipType.Rotate270FlipX,
                8 => RotateFlipType.Rotate270FlipNone, _ => RotateFlipType.RotateNoneFlipNone
            });
        }
        double scale = Math.Min(1, Math.Min((double)maxWidth / image.Width, (double)maxHeight / image.Height));
        using var bitmap = new Bitmap(Math.Max(1, (int)Math.Round(image.Width * scale)),
            Math.Max(1, (int)Math.Round(image.Height * scale)),
            PixelFormat.Format32bppArgb);
        using (var graphics = Graphics.FromImage(bitmap))
        {
            graphics.Clear(jpeg ? Color.White : Color.Transparent);
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphics.DrawImage(image, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
        }
        if (png) ReducePngColors(bitmap, pngQuality);
        string stem = Path.GetFileNameWithoutExtension(source);
        // CreateNew prevents replacing originals or an earlier export.
        for (int suffix = 0; ; suffix++)
        {
            string target = Path.Combine(destination, stem + (suffix == 0 ? "" : $"_{suffix}") + extension);
            FileStream stream;
            try { stream = new FileStream(target, FileMode.CreateNew, FileAccess.Write); }
            catch (IOException) when (File.Exists(target)) { continue; }
            try
            {
                using (stream)
                {
                    if (jpeg)
                    {
                        using var parameters = new EncoderParameters(1);
                        parameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
                        bitmap.Save(stream, ImageCodecInfo.GetImageEncoders().First(codec => codec.FormatID == ImageFormat.Jpeg.Guid), parameters);
                    }
                    else if (webp)
                    {
                        using var pixels = new MemoryStream();
                        bitmap.Save(pixels, ImageFormat.Png);
                        pixels.Position = 0;
                        WebpCodec.Encode(pixels, stream, webpQuality);
                    }
                    else bitmap.Save(stream, ImageFormat.Png);
                }
            }
            catch { File.Delete(target); throw; }
            return;
        }
    }

    private static Image LoadImage(string source)
    {
        if (!Path.GetExtension(source).Equals(".webp", StringComparison.OrdinalIgnoreCase))
            return Image.FromFile(source);

        using var pixels = new MemoryStream();
        WebpCodec.Decode(source, pixels);
        pixels.Position = 0;
        using var decoded = Image.FromStream(pixels);
        // Detach from the stream, which GDI+ otherwise requires to remain open.
        return new Bitmap(decoded);
    }

    private static void ReducePngColors(Bitmap bitmap, int quality)
    {
        // PNG has no JPEG-style encoder quality setting. Quantize RGB before
        // lossless PNG encoding; preserve the alpha channel at full precision.
        if (quality >= 100) return;
        int levels = 2 + (int)Math.Round(254 * Math.Pow((quality - 1) / 99.0, 3));
        var lookup = new byte[256];
        for (int i = 0; i < lookup.Length; i++)
            lookup[i] = (byte)Math.Round(Math.Round(i * (levels - 1) / 255.0) * 255 / (levels - 1));
        var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
            ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
        try
        {
            var row = new byte[bitmap.Width * 4];
            for (int y = 0; y < bitmap.Height; y++)
            {
                var address = IntPtr.Add(data.Scan0, y * data.Stride);
                Marshal.Copy(address, row, 0, row.Length);
                for (int x = 0; x < row.Length; x += 4)
                {
                    row[x] = lookup[row[x]];
                    row[x + 1] = lookup[row[x + 1]];
                    row[x + 2] = lookup[row[x + 2]];
                }
                Marshal.Copy(row, 0, address, row.Length);
            }
        }
        finally { bitmap.UnlockBits(data); }
    }
}
