namespace BulkImagesOptimizer;

internal sealed record EnhancementSettings(int Brightness, int Contrast, int Saturation, int Sharpen);

internal static class ImageEnhancement
{
    // Packed, straight-alpha BGRA pixels. Alpha is never modified.
    public static void Apply(byte[] pixels, int width, int height, EnhancementSettings settings)
    {
        if (width <= 0 || height <= 0 || pixels.Length != checked(width * height * 4))
            throw new ArgumentException("Pixel buffer dimensions do not match.");
        if (settings.Brightness is < -100 or > 100 || settings.Contrast is < 0 or > 200
            || settings.Saturation is < 0 or > 200 || settings.Sharpen is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(settings));

        double brightness = settings.Brightness * 2.55;
        double contrast = settings.Contrast / 100.0;
        double saturation = settings.Saturation / 100.0;
        for (int i = 0; i < pixels.Length; i += 4)
        {
            if (pixels[i + 3] == 0) continue;
            double b = pixels[i], g = pixels[i + 1], r = pixels[i + 2];
            double luminance = 0.2126 * r + 0.7152 * g + 0.0722 * b;
            pixels[i] = Clamp(((luminance + (b - luminance) * saturation) - 127.5) * contrast + 127.5 + brightness);
            pixels[i + 1] = Clamp(((luminance + (g - luminance) * saturation) - 127.5) * contrast + 127.5 + brightness);
            pixels[i + 2] = Clamp(((luminance + (r - luminance) * saturation) - 127.5) * contrast + 127.5 + brightness);
        }

        if (settings.Sharpen == 0) return;
        var original = (byte[])pixels.Clone();
        double amount = settings.Sharpen / 100.0;
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            int center = (y * width + x) * 4;
            if (original[center + 3] == 0) continue;
            double blue = 0, green = 0, red = 0, weight = 0;
            for (int ny = Math.Max(0, y - 1); ny <= Math.Min(height - 1, y + 1); ny++)
            for (int nx = Math.Max(0, x - 1); nx <= Math.Min(width - 1, x + 1); nx++)
            {
                int neighbor = (ny * width + nx) * 4;
                double alpha = original[neighbor + 3] / 255.0;
                blue += original[neighbor] * alpha;
                green += original[neighbor + 1] * alpha;
                red += original[neighbor + 2] * alpha;
                weight += alpha;
            }
            pixels[center] = Clamp(original[center] + amount * (original[center] - blue / weight));
            pixels[center + 1] = Clamp(original[center + 1] + amount * (original[center + 1] - green / weight));
            pixels[center + 2] = Clamp(original[center + 2] + amount * (original[center + 2] - red / weight));
        }
    }

    private static byte Clamp(double value) => (byte)Math.Clamp((int)Math.Round(value), 0, 255);
}
