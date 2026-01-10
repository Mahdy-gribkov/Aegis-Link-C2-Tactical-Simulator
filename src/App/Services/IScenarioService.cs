using AegisLink.App.Models;

namespace AegisLink.App.Services;

public interface IScenarioService
{
    void Start();
    void Stop();
    bool IsRunning { get; }
    event Action<TacticalEvent>? OnEventGenerated;
}
