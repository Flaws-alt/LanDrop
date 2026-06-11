using System;
using System.IO;
using System.Text.Json;

namespace LanDrop.Models;

public class AppSettings
{
    public string DisplayName { get; set; } = string.Empty;
    public string ReceiveDirectory { get; set; } = string.Empty;
    public bool AutoStart { get; set; }

    private static string FilePath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LanDrop", "settings.json");

    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                var json = File.ReadAllText(FilePath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
        }
        catch { }

        return new AppSettings
        {
            DisplayName = Environment.MachineName,
            ReceiveDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "LanDrop"),
            AutoStart = false
        };
    }

    public void Save()
    {
        var dir = Path.GetDirectoryName(FilePath)!;
        Directory.CreateDirectory(dir);
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(FilePath, json);
    }
}
