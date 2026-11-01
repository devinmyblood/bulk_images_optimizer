# bulk_images_optimizer

A Windows Forms desktop application for batch resizing and exporting images.

## Develop and run

Install Visual Studio 2022 version 17.8 or later with the **.NET desktop development** workload and the .NET 8 SDK. The project targets .NET 8 LTS. Open `bulk_images_optimizer.csproj` in Visual Studio and press F5, or run:

```powershell
dotnet build
dotnet run
```

Add images, set maximum dimensions and JPEG quality, choose `.jpg` or `.png` under **Output extension**, choose an output folder, and click **Optimize images**. Exports keep the original base filename with the selected extension. If a filename already exists, a numeric suffix such as `_1` is added to avoid overwriting it. Aspect ratio is preserved; small images are never enlarged. JPEG flattens transparency onto white; PNG preserves transparency. Exports omit source metadata and use only the first frame of animated/multipage images. Output size may exceed the original, particularly for PNG.

The UI is defined in C# in `MainForm.cs`. Running the application requires Windows; Linux can cross-compile with the .NET SDK and Windows targeting packs but cannot run WinForms.
