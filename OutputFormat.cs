namespace BulkImagesOptimizer;

internal static class OutputFormat
{
    public static string ResolveExtension(string source, int outputMode)
    {
        if (outputMode == 0) return ".jpg";
        if (outputMode == 1) return ".png";
        if (outputMode == 3) return ".webp";
        if (outputMode != 2) throw new ArgumentOutOfRangeException(nameof(outputMode));

        string extension = Path.GetExtension(source);
        if (extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".png", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".webp", StringComparison.OrdinalIgnoreCase))
            return extension;

        throw new NotSupportedException("All (original) supports JPG, JPEG, PNG and WebP images. Choose .jpg, .png or .webp to convert other formats.");
    }
}
