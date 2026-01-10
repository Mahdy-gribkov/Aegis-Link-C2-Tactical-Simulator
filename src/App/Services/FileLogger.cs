using System.Diagnostics;
using System.IO;

namespace AegisLink.App.Services;

public class FileLogger : ILogger
{
    private readonly string _logPath;
    private readonly object _lock = new();
    public event Action<string>? OnLog;

    public FileLogger()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var logDir = Path.Combine(appData, "AegisLink", "logs");
        Directory.CreateDirectory(logDir);
        _logPath = Path.Combine(logDir, $"session_{DateTime.Now:yyyyMMdd_HHmmss}.log");
    }

    public void Info(string message)
    {
        var entry = $"[{DateTime.Now:HH:mm:ss}] [INFO] {message}";
        WriteLog(entry);
        OnLog?.Invoke(entry);
    }

    public void Debug(string message)
    {
#if DEBUG
        var entry = $"[{DateTime.Now:HH:mm:ss}] [DEBUG] {message}";
        System.Diagnostics.Debug.WriteLine(entry);
#endif
    }

    public void Error(string message, Exception? ex = null)
    {
        var entry = $"[{DateTime.Now:HH:mm:ss}] [ERROR] {message}";
        if (ex != null) entry += $"\n{ex}";
        WriteLog(entry);
        OnLog?.Invoke(entry);
        WriteCrashLog(entry);
    }

    private void WriteLog(string entry)
    {
        lock (_lock)
        {
            try { File.AppendAllText(_logPath, entry + "\n"); }
            catch { /* Ignore disk errors */ }
        }
    }

    private void WriteCrashLog(string entry)
    {
        try
        {
            var crashPath = Path.Combine(Path.GetDirectoryName(_logPath)!, "crash.log");
            File.AppendAllText(crashPath, entry + "\n");
        }
        catch { }
    }
}
