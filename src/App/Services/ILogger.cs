namespace AegisLink.App.Services;

/// <summary>
/// Structured logging interface with Info/Debug/Error levels
/// </summary>
public interface ILogger
{
    void Info(string message);
    void Debug(string message);
    void Error(string message, Exception? ex = null);
}
