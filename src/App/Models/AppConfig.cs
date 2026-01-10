namespace AegisLink.App.Models;

/// <summary>
/// Application configuration persisted to config.json
/// </summary>
public class AppConfig
{
    public int UdpPort { get; set; } = 5555;
    public string ThemeName { get; set; } = "Dark";
    public bool IsSoundEnabled { get; set; } = true;
    public WindowBounds LastWindowBounds { get; set; } = new();
    public int LastMonitor { get; set; } = 0;
    public string? LastSessionPath { get; set; }
}

public class WindowBounds
{
    public double X { get; set; } = 100;
    public double Y { get; set; } = 100;
    public double Width { get; set; } = 1400;
    public double Height { get; set; } = 800;
}
