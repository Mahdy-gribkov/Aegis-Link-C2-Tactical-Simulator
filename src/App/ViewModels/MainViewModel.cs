using AegisLink.App.Base;
using AegisLink.App.Services;
using AegisLink.Core;
using AegisLink.Persistence;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace AegisLink.App.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private IDataLink _activeDataLink;
        private readonly UdpLinkService _realLinkService;
        private readonly MissionRepository _missionRepository;
        private VirtualLauncherService? _simService;
        private readonly DispatcherTimer _missionClockTimer;
        private DateTime _missionStartTime;

        private bool _isSimulationActive;
        public bool IsSimulationActive
        {
            get => _isSimulationActive;
            set => SetProperty(ref _isSimulationActive, value);
        }

        // Mission Clock
        private string _missionClock = "00:00:00";
        public string MissionClock
        {
            get => _missionClock;
            set => SetProperty(ref _missionClock, value);
        }

        // Terminal
        private int _terminalHeight = 0;
        public int TerminalHeight
        {
            get => _terminalHeight;
            set => SetProperty(ref _terminalHeight, value);
        }

        private Visibility _terminalVisibility = Visibility.Collapsed;
        public Visibility TerminalVisibility
        {
            get => _terminalVisibility;
            set => SetProperty(ref _terminalVisibility, value);
        }

        // Telemetry Properties
        private double _azimuth;
        public double Azimuth
        {
            get => _azimuth;
            set => SetProperty(ref _azimuth, value);
        }

        private double _elevation;
        public double Elevation
        {
            get => _elevation;
            set => SetProperty(ref _elevation, value);
        }

        private int _batteryLevel;
        public int BatteryLevel
        {
            get => _batteryLevel;
            set => SetProperty(ref _batteryLevel, value);
        }

        private float _signalStrength;
        public float SignalStrength
        {
            get => _signalStrength;
            set => SetProperty(ref _signalStrength, value);
        }

        private string _connectionStatus = "STANDBY";
        public string ConnectionStatus
        {
            get => _connectionStatus;
            set => SetProperty(ref _connectionStatus, value);
        }

        // Logs
        public ObservableCollection<string> CommandLog { get; } = new ObservableCollection<string>();
        private readonly object _logLock = new object();

        // Commands
        public ICommand SendPingCommand { get; }
        public ICommand ToggleSimulationCommand { get; }

        public MainViewModel(UdpLinkService realLinkService, MissionRepository missionRepository)
        {
            _realLinkService = realLinkService ?? throw new ArgumentNullException(nameof(realLinkService));
            _missionRepository = missionRepository ?? throw new ArgumentNullException(nameof(missionRepository));
            _activeDataLink = _realLinkService;

            BindingOperations.EnableCollectionSynchronization(CommandLog, _logLock);
            _activeDataLink.OnFrameReceived += OnFrameReceived;

            SendPingCommand = new RelayCommand(ExecuteSendPing);
            ToggleSimulationCommand = new RelayCommand(ExecuteToggleSimulation);

            // Mission Clock
            _missionStartTime = DateTime.Now;
            _missionClockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _missionClockTimer.Tick += (s, e) => 
            {
                var elapsed = DateTime.Now - _missionStartTime;
                MissionClock = elapsed.ToString(@"hh\:mm\:ss");
            };
            _missionClockTimer.Start();
            
            AddLogEntry("[SYSTEM] Aegis-Link Tactical Console Initialized");
            AddLogEntry("[SYSTEM] Mode: STANDBY | Awaiting Link");
            _ = _missionRepository.LogEventAsync("SYSTEM_INIT", "Flagship Console v1.6.0 started.");
        }

        private void ExecuteToggleSimulation(object? obj)
        {
            _activeDataLink.OnFrameReceived -= OnFrameReceived;

            if (IsSimulationActive)
            {
                AddLogEntry("[MODE] Switching to SIMULATION...");
                ConnectionStatus = "SIMULATING";
                _simService = new VirtualLauncherService();
                _activeDataLink = _simService;
                _ = _missionRepository.LogEventAsync("MODE_SWITCH", "SIMULATION mode activated.");
            }
            else
            {
                AddLogEntry("[MODE] Switching to LIVE HARDWARE...");
                ConnectionStatus = "STANDBY";
                _simService?.Dispose();
                _simService = null;
                _activeDataLink = _realLinkService;
                _ = _missionRepository.LogEventAsync("MODE_SWITCH", "LIVE HARDWARE mode activated.");
            }

            _activeDataLink.OnFrameReceived += OnFrameReceived;
        }

        private void OnFrameReceived(TelemetryFrame frame)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Azimuth = frame.Latitude;
                Elevation = frame.Longitude;
                BatteryLevel = frame.BatteryLevel;
                SignalStrength = frame.SignalStrength;
                ConnectionStatus = "◉ LINKED";
            });
        }

        private async void ExecuteSendPing(object? obj)
        {
            AddLogEntry("[COMMS] Transmitting PING...");
            byte[] ping = new byte[] { 0x50, 0x49, 0x4E, 0x47 };
            await _activeDataLink.SendCommandAsync(ping);
            await _missionRepository.LogEventAsync("COMMAND_SENT", "PING");
            AddLogEntry("[COMMS] PING acknowledged");
        }

        public void AddLogEntry(string message)
        {
            lock (_logLock)
            {
                CommandLog.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
            }
        }
    }
}
