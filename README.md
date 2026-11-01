# bulk_images_optimizer

A Windows Forms desktop application for batch resizing and exporting images.

## Develop and run

Install the .NET 10 SDK on Windows, or Visual Studio with .NET 10 support and the **.NET desktop development** workload. Open `bulk_images_optimizer.csproj` in Visual Studio and press F5, or run:

```powershell
dotnet build
dotnet run
```

Add images, set maximum dimensions and JPEG quality, select JPEG or PNG, choose an output folder, and click **Optimize images**. Aspect ratio is preserved; small images are never enlarged. Existing files are never overwritten. JPEG flattens transparency onto white; PNG preserves transparency. Exports omit source metadata and use only the first frame of animated/multipage images. Output size may exceed the original, particularly for PNG.

The UI is defined in C# in `MainForm.cs`. Running the application requires Windows; Linux can cross-compile with the .NET SDK and Windows targeting packs but cannot run WinForms.
