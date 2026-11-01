namespace BulkImagesOptimizer;

public sealed class MainForm : Form
{
    private readonly ListBox files = new() { Dock = DockStyle.Fill, HorizontalScrollbar = true };
    private readonly NumericUpDown width = new() { Minimum = 1, Maximum = 30000, Value = 1920, Width = 85 };
    private readonly NumericUpDown height = new() { Minimum = 1, Maximum = 30000, Value = 1080, Width = 85 };
    private readonly NumericUpDown quality = new() { Minimum = 1, Maximum = 100, Value = 85, Width = 65 };
    private readonly NumericUpDown pngQuality = new() { Minimum = 1, Maximum = 100, Value = 85, Width = 65, Enabled = false };
    private readonly ComboBox format = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 150 };
    private readonly TextBox output = new() { Width = 360, ReadOnly = true };
    private readonly Label status = new() { AutoSize = true, Text = "Add images to get started.", Padding = new Padding(0, 8, 0, 0) };
    private readonly ProgressBar progress = new() { Dock = DockStyle.Bottom, Height = 22 };
    private readonly Button optimize = new() { Text = "Optimize images", AutoSize = true };
    private readonly FlowLayoutPanel controls = new() { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(8), WrapContents = true };

    public MainForm()
    {
        Text = "bulk_images_optimizer";
        ClientSize = new Size(940, 560);
        MinimumSize = new Size(760, 420);
        Font = new Font("Segoe UI", 10);
        format.Items.AddRange([".jpg", ".png", "All (original)"]);
        format.SelectedIndex = 0;
        format.SelectedIndexChanged += (_, _) =>
        {
            quality.Enabled = format.SelectedIndex != 1;
            pngQuality.Enabled = format.SelectedIndex != 0;
        };
        var add = new Button { Text = "Add images", AutoSize = true };
        var clear = new Button { Text = "Clear list", AutoSize = true };
        var browse = new Button { Text = "Output folder…", AutoSize = true };
        add.Click += (_, _) => AddImages();
        clear.Click += (_, _) => { files.Items.Clear(); status.Text = "List cleared."; };
        browse.Click += (_, _) =>
        {
            using var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog(this) == DialogResult.OK) output.Text = dialog.SelectedPath;
        };
        optimize.Click += async (_, _) => await OptimizeAsync();
        controls.Controls.AddRange([add, clear, new Label { Text = "Max width", AutoSize = true }, width,
            new Label { Text = "Max height", AutoSize = true }, height,
            new Label { Text = "JPEG quality", AutoSize = true }, quality,
            new Label { Text = "PNG quality", AutoSize = true }, pngQuality,
            new Label { Text = "Output extension", AutoSize = true }, format]);
        var bottom = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, Padding = new Padding(8) };
        bottom.Controls.AddRange([browse, output, optimize, status]);
        Controls.Add(files);
        Controls.Add(controls);
        Controls.Add(bottom);
        Controls.Add(progress);
    }

    private void AddImages()
    {
        using var dialog = new OpenFileDialog
        {
            Multiselect = true,
            Filter = "Images|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.tif;*.tiff|All files|*.*"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        foreach (var path in dialog.FileNames)
            if (!files.Items.Cast<string>().Contains(path, StringComparer.OrdinalIgnoreCase)) files.Items.Add(path);
        status.Text = $"{files.Items.Count} image(s) selected.";
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (!controls.Enabled)
        {
            e.Cancel = true;
            MessageBox.Show(this, "Please wait for the current batch to finish.", Text);
        }
        base.OnFormClosing(e);
    }

    private async Task OptimizeAsync()
    {
        if (files.Items.Count == 0 || string.IsNullOrWhiteSpace(output.Text))
        {
            MessageBox.Show(this, "Add images and choose an output folder first.", Text);
            return;
        }
        var paths = files.Items.Cast<string>().ToArray();
        var destination = output.Text;
        int maxWidth = (int)width.Value, maxHeight = (int)height.Value;
        long jpegQuality = (long)quality.Value;
        int selectedPngQuality = (int)pngQuality.Value;
        int outputMode = format.SelectedIndex;
        var errors = new List<string>();
        progress.Maximum = paths.Length;
        progress.Value = 0;
        controls.Enabled = false;
        optimize.Parent!.Enabled = false;
        var reporter = new Progress<int>(count =>
        {
            progress.Value = count;
            status.Text = $"Processed {count} of {paths.Length}";
        });
        try
        {
            await Task.Run(() =>
            {
                Directory.CreateDirectory(destination);
                for (int i = 0; i < paths.Length; i++)
                {
                    try { ImageExporter.Export(paths[i], destination, maxWidth, maxHeight, jpegQuality, selectedPngQuality, outputMode); }
                    catch (Exception ex) { errors.Add($"{Path.GetFileName(paths[i])}: {ex.Message}"); }
                    ((IProgress<int>)reporter).Report(i + 1);
                }
            });
            status.Text = $"Finished: {paths.Length - errors.Count} exported, {errors.Count} failed.";
            if (errors.Count > 0) MessageBox.Show(this, string.Join(Environment.NewLine, errors), "Export errors");
        }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, "Export failed"); }
        finally { controls.Enabled = true; optimize.Parent!.Enabled = true; }
    }

}
