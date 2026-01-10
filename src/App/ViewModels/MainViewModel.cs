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
        private readonly DispatcherTimer _simulationTimer;
        private DateTime _missionStartTime;

        public event Action<string>? ToastRequested;
        public event Action? RxFlashRequested;

        private bool _isSimulationActive;
        public bool IsSimulationActive
        {
            get => _isSimulationActive;
            set => SetProperty(ref _isSimulationActive, value);
        }

        // Navigation
        private DataTemplate? _currentViewTemplate;
        public DataTemplate? CurrentViewTemplate
        {
            get => _currentViewTemplate;
            set => SetProperty(ref _currentViewTemplate, value);
        }

        // Mission Clock
        private string _missionClock = "00:00:00";
        public string MissionClock
        {
            get => _missionClock;
            set => SetProperty(ref _missionClock, value);
        }

        // Terminal
        private Visibility _terminalVisibility = Visibility.Collapsed;
        public Visibility TerminalVisibility
        {
            get => _terminalVisibility;
            set => SetProperty(ref _terminalVisibility, value);
        }

        // Telemetry
        private double _azimuth;
        public double Azimuth
        {
            get => _azimuth;
            set => SetProperty(ref _azimuth, value);
        }

        private double _distance = 50;
        public double Distance
        {
            get => _distance;
            set => SetProperty(ref _distance, value);
        }

        private double _elevation;
        public double Elevation
        {
            get => _elevation;
            set => SetProperty(ref _elevation, value);
        }

        private int _batteryLevel = 100;
        public int BatteryLevel
        {
            get => _batteryLevel;
            set
            {
                SetProperty(ref _batteryLevel, value);
                OnPropertyChanged(nameof(BatteryColor));
            }
        }

        public System.Windows.Media.Brush BatteryColor => BatteryLevel < 20 
            ? System.Windows.Media.Brushes.Red 
            : System.Windows.Media.Brushes.Lime;

        private float _signalStrength = -40;
        public float SignalStrength
        {
            get => _signalStrength;
            set
            {
                SetProperty(ref _signalStrength, value);
                OnPropertyChanged(nameof(SignalColor));
            }
        }

        public System.Windows.Media.Brush SignalColor => SignalStrength < -80 
            ? System.Windows.Media.Brushes.Yellow 
            : System.Windows.Media.Brushes.Cyan;

        private string _connectionStatus = "STANDBY";
        public string ConnectionStatus
        {
            get => _connectionStatus;
            set => SetProperty(ref _connectionStatus, value);
        }

        public ObservableCollection<string> CommandLog { get; } = new();
        private readonly object _logLock = new();

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
            _missionClockTimer.Tick += (s, e) => MissionClock = (DateTime.Now - _missionStartTime).ToString(@"hh\:mm\:ss");
            _missionClockTimer.Start();

            // Simulation Timer (Active sim with synthetic telemetry)
            _simulationTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            _simulationTimer.Tick += SimulationTimer_Tick;

            AddLogEntry("[SYSTEM] Aegis-Link Phoenix v2.0.0 Initialized");
            AddLogEntry("[SYSTEM] Press [~] for terminal, [F11] for fullscreen");
            _ = _missionRepository.LogEventAsync("SYSTEM_INIT", "Phoenix v2.0.0 started.");
        }

        private void SimulationTimer_Tick(object? sender, EventArgs e)
        {
            if (!IsSimulationActive) return;

            Azimuth = (Azimuth + 2) % 360;
            Distance = 50 + 30 * Math.Sin(DateTime.Now.Ticks / 10000000.0);
            BatteryLevel = Math.Max(5, 100 - (int)((DateTime.Now - _missionStartTime).TotalSeconds / 2) % 100);
            SignalStrength = -40 + (float)(20 * Math.Sin(DateTime.Now.Ticks / 5000000.0));
        }

        public void NavigateToRadar()
        {
            CurrentViewTemplate = Application.Current.MainWindow?.FindResource("RadarTemplate") as DataTemplate;
            AddLogEntry("[NAV] Switched to RADAR view");
        }

        public void NavigateToDashboard()
        {
            CurrentViewTemplate = Application.Current.MainWindow?.FindResource("DashboardTemplate") as DataTemplate;
        }

        public void ToggleTerminal()
        {
            TerminalVisibility = TerminalVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void ExecuteToggleSimulation(object? obj)
        {
            _activeDataLink.OnFrameReceived -= OnFrameReceived;

            if (IsSimulationActive)
            {
                AddLogEntry("[MODE] SIMULATION ACTIVE - Synthetic telemetry");
                ConnectionStatus = "◉ SIMULATING";
                _simService = new VirtualLauncherService();
                _activeDataLink = _simService;
                _simulationTimer.Start();
                ToastRequested?.Invoke("Simulation Mode Active");
            }
            else
            {
                AddLogEntry("[MODE] Returning to LIVE HARDWARE");
                ConnectionStatus = "STANDBY";
                _simService?.Dispose();
                _simService = null;
                _activeDataLink = _realLinkService;
                _simulationTimer.Stop();
            }

            _activeDataLink.OnFrameReceived += OnFrameReceived;
            _ = _missionRepository.LogEventAsync("MODE_SWITCH", IsSimulationActive ? "SIMULATION" : "LIVE");
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
                RxFlashRequested?.Invoke();
            });
        }

        private async void ExecuteSendPing(object? obj)
        {
            AddLogEntry("[COMMS] Transmitting PING...");
            byte[] ping = new byte[] { 0x50, 0x49, 0x4E, 0x47 };
            await _activeDataLink.SendCommandAsync(ping);
            await _missionRepository.LogEventAsync("COMMAND_SENT", "PING");
            AddLogEntry("[COMMS] PING transmitted");
            System.Media.SystemSounds.Beep.Play();
        }

        public void AddLogEntry(string message)
        {
            lock (_logLock)
            {
                CommandLog.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
                if (CommandLog.Count > 100) CommandLog.RemoveAt(0);
            }
        }
    }
}
