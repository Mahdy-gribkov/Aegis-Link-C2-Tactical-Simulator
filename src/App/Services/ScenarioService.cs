using AegisLink.App.Models;
using System.Windows.Threading;

namespace AegisLink.App.Services;

public class ScenarioService : IScenarioService
{
    private readonly DispatcherTimer _timer;
    private readonly Random _random = new();
    private int _tickCount = 0;
    
    private readonly List<TacticalEvent> _scenarioCycle = new()
    {
        new(DateTime.Now, EventType.KeepAlive, -45f, "System heartbeat"),
        new(DateTime.Now, EventType.Ping, -42f, "Probe response"),
        new(DateTime.Now, EventType.DataPacket, -38f, "{\"az\":45,\"dist\":120}"),
        new(DateTime.Now, EventType.Burst, -35f, "Signal burst detected"),
        new(DateTime.Now, EventType.DataPacket, -40f, "{\"az\":90,\"dist\":85}"),
    };

    public bool IsRunning { get; private set; }
    public event Action<TacticalEvent>? OnEventGenerated;

    public ScenarioService()
    {
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _timer.Tick += Timer_Tick;
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        _tickCount++;
        var baseEvent = _scenarioCycle[_tickCount % _scenarioCycle.Count];
        
        var evt = new TacticalEvent(
            DateTime.Now,
            baseEvent.Type,
            baseEvent.SignalStrength + (float)(_random.NextDouble() * 10 - 5),
            baseEvent.PayloadData
        );
        
        OnEventGenerated?.Invoke(evt);
    }

    public void Start()
    {
        IsRunning = true;
        _timer.Start();
    }

    public void Stop()
    {
        IsRunning = false;
        _timer.Stop();
    }
}
