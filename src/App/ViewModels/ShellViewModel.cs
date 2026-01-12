using AegisLink.App.Models;
using AegisLink.App.Services;
using AegisLink.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
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
    private readonly object _logLock = new(); // Thread-safety lock for terminal logs

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
    [ObservableProperty] private bool _isSignalLost = true;
    [ObservableProperty] private bool _isLinkEstablished = false;
    [ObservableProperty] private bool _isMuted = false;
    [ObservableProperty] private bool _isArmed = false;
    [ObservableProperty] private bool _isLocked = false;


    public string BuildVersion => Assembly.GetExecutingAssembly()
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "3.0.0";

    public ObservableCollection<string> TerminalLog { get; } = new();
    public ObservableCollection<string> ToastQueue { get; } = new();

    private readonly DispatcherTimer _clockTimer;
    private readonly DateTime _startTime = DateTime.Now;
    private int _packetCount = 0;
    private readonly DispatcherTimer _fpsTimer;

    public ShellViewModel(ILogger logger, IConfigService configService, ISoundService soundService, IScenarioService scenarioService, IDataLink dataLink)
    {
        _logger = logger;
        _configService = configService;
        _soundService = soundService;
        _scenarioService = scenarioService;

        _scenarioService.OnEventGenerated += OnTacticalEvent;
        dataLink.OnFrameReceived += OnUdpFrame;

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

        // [SENIOR] Enable multi-threaded access to UI collection
        BindingOperations.EnableCollectionSynchronization(TerminalLog, _logLock);

        _logger.Info("[SYSTEM] Aegis-Link v3.0.0 Initialized");
        _logger.Info($"[BUILD] {BuildVersion}");
        AddTerminalEntry("[SYSTEM] Version 3.0 Flagship Mode Active");
        AddTerminalEntry("[SYSTEM] Type 'help' for commands. Press ~ to toggle terminal.");
    }

    private void Watchdog_Tick(object? sender, EventArgs e)
    {
        var secondsSincePacket = (DateTime.Now - _lastPacketTime).TotalSeconds;
        
        if (CurrentState == ApplicationState.Tracking && secondsSincePacket > 5)
        {
            CurrentState = ApplicationState.Offline;
            ConnectionStatus = "LOST LINK";
            IsSignalLost = true;
            IsLinkEstablished = false;
            _soundService.PlayAlert();
            _logger.Info("[ALERT] Signal lost - no packets for 5 seconds");
            ShowToast("⚠ SIGNAL LOST");
        }
        else if (CurrentState == ApplicationState.Idle && secondsSincePacket > 5)
        {
            IsSignalLost = true;
            IsLinkEstablished = false;
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
            IsSignalLost = false;
            IsLinkEstablished = true;
            
            if (evt.Type == EventType.Burst)
            {
                _soundService.PlayChirp();
                ShowToast("📡 Burst Detected");
            }

            UpdateSignalGraph();
            LogToAar($"[TRACK] AZ:{Azimuth:F2} SIG:{_signalStrength:F2}");
        });
    }

    private void OnUdpFrame(TelemetryFrame frame)
    {
        _lastPacketTime = DateTime.Now;
        _packetCount++;

        Application.Current.Dispatcher.InvokeAsync(() =>
        {
            BatteryLevel = frame.BatteryLevel;
            // SignalStrength is no longer in the 5-byte protocol per Master Plan
            Azimuth = frame.Azimuth;
            Distance = frame.Elevation;
            
            IsArmed = frame.IsArmed;
            IsLocked = frame.IsLocked;

            CurrentState = ApplicationState.Tracking;
            ConnectionStatus = LaunchValidator.GetSafetyReport(frame);
            IsSignalLost = false;
            IsLinkEstablished = true;

            UpdateSignalGraph();
            LogToAar($"[LINK] {ConnectionStatus} AZ:{Azimuth:F2} EL:{Distance:F2} Bat:{BatteryLevel}%");
        });
    }

    private void UpdateSignalGraph()
    {
        const int MaxPoints = 100;
        const double GraphHeight = 70;
        
        double y = GraphHeight - ((_signalStrength + 80) / 80.0 * GraphHeight);
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
    private void FireMission()
    {
        if (IsArmed && IsLocked)
        {
            _logger.Info("[FIRE] Strategic Mission Initiated");
            AddTerminalEntry("[FIRE] CRIT: LAUNCH SIGNAL SENT");
            LogToAar("[CRIT] LAUNCH COMMAND EXECUTED");
            _soundService.PlayAlert();
            ShowToast("🚀 MISSILE AWAY");
        }
        else
        {
            _logger.Info("[FIRE] Safety Interlock Failure");
            AddTerminalEntry("[WARN] Safety Interlock Active: System Unarmed/No Lock");
            _soundService.PlayClick();
            ShowToast("⚠ LAUNCH ABORTED");
        }
    }

    [RelayCommand]
    private void ToggleArmament()
    {
        IsArmed = !IsArmed;
        _logger.Info($"[SYSTEM] Armament State: {(IsArmed ? "ARMED" : "SAFE")}");
        AddTerminalEntry($"[SYSTEM] Armament: {(IsArmed ? "ARMED" : "SAFE")}");
        _soundService.PlayClick();
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
    private void ClearLog() => TerminalLog.Clear();

    [RelayCommand]
    private void ToggleMute()
    {
        IsMuted = !IsMuted;
        _soundService.IsMuted = IsMuted;
        ShowToast(IsMuted ? "🔇 Audio Muted" : "🔊 Audio Enabled");
    }

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
        // [SENIOR] Direct addition to collection from any thread via synchronized lock
        lock (_logLock)
        {
            TerminalLog.Add($"[{DateTime.Now:HH:mm:ss}] {entry}");
            if (TerminalLog.Count > 200) TerminalLog.RemoveAt(0);
        }
    }

    public void ShowToast(string message)
    {
        ToastQueue.Add(message);
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
        timer.Tick += (s, e) => { if (ToastQueue.Count > 0) ToastQueue.RemoveAt(0); timer.Stop(); };
        timer.Start();
    }

    private void LogToAar(string entry)
    {
        try
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var aarPath = Path.Combine(appData, "AegisLink", "logs", "AAR_LOG.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(aarPath)!);
            File.AppendAllTextAsync(aarPath, $"[{DateTime.Now:O}] {entry}\n");
        }
        catch { /* Fallback to standard logger if disk is full/locked */ }
    }
}
