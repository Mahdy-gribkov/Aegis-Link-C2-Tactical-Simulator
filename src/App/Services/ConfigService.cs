using AegisLink.App.Models;
using System.IO;
using System.Text.Json;

namespace AegisLink.App.Services;

public class ConfigService : IConfigService
{
    private readonly string _configPath;
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public ConfigService()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var configDir = Path.Combine(appData, "AegisLink");
        Directory.CreateDirectory(configDir);
        _configPath = Path.Combine(configDir, "config.json");
    }

    public AppConfig Load()
    {
        try
        {
            if (File.Exists(_configPath))
            {
                var json = File.ReadAllText(_configPath);
                return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
            }
        }
        catch { }
        return new AppConfig();
    }

    public void Save(AppConfig config)
    {
        try
        {
            var json = JsonSerializer.Serialize(config, _jsonOptions);
            File.WriteAllText(_configPath, json);
        }
        catch { }
    }
}
