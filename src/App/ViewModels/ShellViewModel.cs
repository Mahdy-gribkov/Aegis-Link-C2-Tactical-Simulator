using AegisLink.App.Models;
using AegisLink.App.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace AegisLink.App.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    private readonly ILogger _logger;
    private readonly IConfigService _configService;
    private readonly ISoundService _soundService;
    private readonly IScenarioService _scenarioService;
    private readonly DispatcherTimer _watchdogTimer;
    private DateTime _lastPacketTime = DateTime.Now;

    // Observable Properties (CommunityToolkit source-generated)
    [ObservableProperty] private ApplicationState _currentState = ApplicationState.Idle;
    [ObservableProperty] private string _connectionStatus = "STANDBY";
    [ObservableProperty] private int _batteryLevel = 100;
    [ObservableProperty] private float _signalStrength = -40f;
    [ObservableProperty] private double _azimuth = 0;
    [ObservableProperty] private double _distance = 50;
    [ObservableProperty] private bool _isTerminalVisible = false;
    [ObservableProperty] private string _missionClock = "00:00:00";
    [ObservableProperty] private PointCollection _signalPoints = new();
    [ObservableProperty] private int _fps = 60;
    [ObservableProperty] private int _packetsPerSecond = 0;

    public string BuildVersion => Assembly.GetExecutingAssembly()
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "3.0.0";

    public ObservableCollection<string> TerminalLog { get; } = new();
    public ObservableCollection<string> ToastQueue { get; } = new();

    private readonly DispatcherTimer _clockTimer;
    private readonly DateTime _startTime = DateTime.Now;
    private int _packetCount = 0;
    private readonly DispatcherTimer _fpsTimer;

    public ShellViewModel(ILogger logger, IConfigService configService, ISoundService soundService, IScenarioService scenarioService)
    {
        _logger = logger;
        _configService = configService;
        _soundService = soundService;
        _scenarioService = scenarioService;

        _scenarioService.OnEventGenerated += OnTacticalEvent;

        // Mission Clock
        _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _clockTimer.Tick += (s, e) => MissionClock = (DateTime.Now - _startTime).ToString(@"hh\:mm\:ss");
        _clockTimer.Start();

        // Watchdog (5 second timeout)
        _watchdogTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _watchdogTimer.Tick += Watchdog_Tick;
        _watchdogTimer.Start();

        // FPS/PPS counter
        _fpsTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _fpsTimer.Tick += (s, e) => { PacketsPerSecond = _packetCount; _packetCount = 0; };
        _fpsTimer.Start();

        _logger.Info("[SYSTEM] Aegis-Link v3.0.0 Initialized");
        _logger.Info($"[BUILD] {BuildVersion}");
        AddTerminalEntry("[SYSTEM] Type 'help' for commands. Press ~ to toggle terminal.");
    }

    private void Watchdog_Tick(object? sender, EventArgs e)
    {
        if (CurrentState == ApplicationState.Tracking && (DateTime.Now - _lastPacketTime).TotalSeconds > 5)
        {
            CurrentState = ApplicationState.Offline;
            ConnectionStatus = "LOST LINK";
            _soundService.PlayAlert();
            _logger.Info("[ALERT] Signal lost - no packets for 5 seconds");
            ShowToast("⚠ SIGNAL LOST");
        }
    }

    private void OnTacticalEvent(TacticalEvent evt)
    {
        _lastPacketTime = DateTime.Now;
        _packetCount++;
        
        Application.Current.Dispatcher.InvokeAsync(() =>
        {
            SignalStrength = evt.SignalStrength;
            CurrentState = ApplicationState.Tracking;
            ConnectionStatus = "◉ TRACKING";
            
            if (evt.Type == EventType.Burst)
            {
                _soundService.PlayBeep();
                ShowToast("📡 Burst Detected");
            }

            UpdateSignalGraph();
        });
    }

    private void UpdateSignalGraph()
    {
        const int MaxPoints = 100;
        const double GraphHeight = 70;
        
        double y = GraphHeight - ((SignalStrength + 80) / 80.0 * GraphHeight);
        y = Math.Clamp(y, 0, GraphHeight);

        var newPoints = new PointCollection();
        foreach (var pt in SignalPoints)
        {
            if (pt.X > 0) newPoints.Add(new Point(pt.X - 2, pt.Y));
        }
        newPoints.Add(new Point(MaxPoints * 2, y));
        while (newPoints.Count > MaxPoints) newPoints.RemoveAt(0);
        
        SignalPoints = newPoints;
    }

    [RelayCommand]
    private void InitiateScan()
    {
        if (_scenarioService.IsRunning)
        {
            _scenarioService.Stop();
            CurrentState = ApplicationState.Idle;
            ConnectionStatus = "STANDBY";
            _logger.Info("[SCAN] Stopped");
            ShowToast("Scan Stopped");
        }
        else
        {
            _scenarioService.Start();
            CurrentState = ApplicationState.Scanning;
            ConnectionStatus = "SCANNING...";
            _logger.Info("[SCAN] Started on simulated channel");
            ShowToast("Scan Active");
        }
    }

    [RelayCommand]
    private void CapturePacket()
    {
        var snapshot = $"AZ:{Azimuth:N1} DIST:{Distance:N1} SIG:{SignalStrength:N1}dB";
        _logger.Info($"[CAPTURE] {snapshot}");
        _soundService.PlayClick();
        ShowToast("Packet Captured");
    }

    [RelayCommand]
    private void ToggleTerminal() => IsTerminalVisible = !IsTerminalVisible;

    [RelayCommand]
    private void ExecuteCommand(string? input)
    {
        var parsed = CommandTokenizer.Parse(input);
        if (parsed == null)
        {
            AddTerminalEntry("[ERROR] Invalid command");
            return;
        }

        AddTerminalEntry($"> {input}");
        _soundService.PlayClick();

        switch (parsed.Command)
        {
            case "HELP":
                AddTerminalEntry("Commands: scan, stop, status, clear, export -log, help");
                break;
            case "SCAN":
                InitiateScan();
                break;
            case "STOP":
                _scenarioService.Stop();
                CurrentState = ApplicationState.Idle;
                ConnectionStatus = "STANDBY";
                AddTerminalEntry("[SCAN] Stopped");
                break;
            case "STATUS":
                AddTerminalEntry($"[STATUS] State:{CurrentState} AZ:{Azimuth:N1} SIG:{SignalStrength:N1}dB");
                break;
            case "CLEAR":
                TerminalLog.Clear();
                break;
            case "EXPORT":
                if (parsed.Flags.ContainsKey("log")) ExportLog();
                else AddTerminalEntry("[ERROR] Usage: export -log");
                break;
            default:
                AddTerminalEntry($"[ERROR] Unknown command: {parsed.Command}");
                break;
        }
    }

    private void ExportLog()
    {
        try
        {
            var docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var path = Path.Combine(docs, $"AegisLink_Log_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
            File.WriteAllLines(path, TerminalLog);
            AddTerminalEntry($"[EXPORT] Saved to {Path.GetFileName(path)}");
            ShowToast("Log Exported");
        }
        catch (Exception ex)
        {
            AddTerminalEntry($"[ERROR] Export failed: {ex.Message}");
        }
    }

    public void AddTerminalEntry(string entry)
    {
        TerminalLog.Add($"[{DateTime.Now:HH:mm:ss}] {entry}");
        if (TerminalLog.Count > 200) TerminalLog.RemoveAt(0);
    }

    public void ShowToast(string message)
    {
        ToastQueue.Add(message);
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
        timer.Tick += (s, e) => { if (ToastQueue.Count > 0) ToastQueue.RemoveAt(0); timer.Stop(); };
        timer.Start();
    }
}
