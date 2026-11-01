namespace BulkImagesOptimizer;

public sealed class ImageEnhancerForm : BatchImageForm
{
    public ImageEnhancerForm() : base(enhance: true)
    {
        StartPosition = FormStartPosition.CenterParent;
    }
}
