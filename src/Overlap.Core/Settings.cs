using System.Text.Json;

namespace Overlap.Core;

public sealed class OverlaySettings
{
    public double X { get; set; } = 120;
    public double Y { get; set; } = 120;
    public double Scale { get; set; } = 1.0;
    public double Opacity { get; set; } = 0.85;
}

public sealed class SettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly string _path;

    public SettingsStore(string? appDataRoot = null)
    {
        var basePath = appDataRoot ?? Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _path = Path.Combine(basePath, "Overlap", "settings.json");
    }

    public OverlaySettings Load()
    {
        try
        {
            if (!File.Exists(_path))
            {
                return new OverlaySettings();
            }

            var text = File.ReadAllText(_path);
            return JsonSerializer.Deserialize<OverlaySettings>(text, JsonOptions) ?? new OverlaySettings();
        }
        catch
        {
            return new OverlaySettings();
        }
    }

    public void Save(OverlaySettings settings)
    {
        var dir = Path.GetDirectoryName(_path);
        if (!string.IsNullOrWhiteSpace(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var content = JsonSerializer.Serialize(settings, JsonOptions);
        File.WriteAllText(_path, content);
    }
}
