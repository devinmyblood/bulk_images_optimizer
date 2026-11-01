# bulk_images_optimizer

A Windows Forms desktop application for batch resizing and exporting images.

## Develop and run

Install Visual Studio 2022 version 17.8 or later with the **.NET desktop development** workload and the .NET 8 SDK. The project targets .NET 8 LTS. Open `bulk_images_optimizer.csproj` in Visual Studio and press F5, or run:

```powershell
dotnet build
dotnet run
```

The project explicitly uses `BulkImagesOptimizer.Program` as its startup object, which opens `MainForm`. In a Visual Studio solution with multiple projects, right-click **bulk_images_optimizer** and choose **Set as Startup Project** to launch it with F5.

Add images, set maximum dimensions and JPEG quality, choose `.jpg` or `.png` under **Output extension**, choose an output folder, and click **Optimize images**. Exports keep the original base filename with the selected extension. If a filename already exists, a numeric suffix such as `_1` is added to avoid overwriting it. Aspect ratio is preserved; small images are never enlarged. JPEG flattens transparency onto white; PNG preserves transparency. Exports omit source metadata and use only the first frame of animated/multipage images. Output size may exceed the original, particularly for PNG.

The shared UI is defined in C# in `BatchImageForm.cs`, with `MainForm.cs` and `ImageEnhancerForm.cs` selecting the workflow. Running the application requires Windows; Linux can cross-compile with the .NET SDK and Windows targeting packs but cannot run WinForms.

## PNG quality

Select `.png` to enable **PNG quality** (1–100). Lower values reduce RGB color precision before PNG compression, trading color detail for potentially smaller files. At 100, full RGB precision is retained. Alpha transparency is preserved at every quality setting. JPEG and PNG have separate quality inputs. PNG file sizes depend on image content and are not guaranteed to be smaller than the source, especially when converting JPEG photographs.

## Mixed JPG and PNG batches

Choose **All (original)** under **Output extension** to keep each JPG, JPEG and PNG image's original extension, including its letter case. Both quality inputs are enabled: **JPEG quality** applies to JPG/JPEG images and **PNG quality** applies to PNG images. Original base filenames and overwrite protection still apply. Other source formats are reported as failed in this mode; choose `.jpg` or `.png` to convert those formats instead.

## Image enhancer

Click **Image enhancer** at the top left to open the enhancement form. It has its own image list and output folder, plus all the optimizer's resize, JPEG/PNG quality, mixed-format output, progress, and filename protection options. Close the enhancer to return to the optimizer.

Adjust **Brightness** (-100 to 100), **Contrast** (0–200%), **Saturation** (0–200%), and **Sharpen** (0–100%), then click **Enhance images**. Brightness 0, contrast/saturation 100, and sharpen 0 leave colors unchanged before export. The initial settings apply 25% sharpening; **Reset enhancements** restores these initial settings. Enhancements run after resizing and before output compression. PNG alpha is preserved; JPEG uses a white background. Lower PNG quality can reduce detail added by enhancements; use 100 for full color precision.

These controls adjust appearance and edge clarity; they cannot reconstruct missing detail in a blurred or heavily compressed original. The forms wait for active batches to finish before closing.
