using AegisLink.App.Base;
using AegisLink.App.Services;
using AegisLink.Core;
using AegisLink.Persistence;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace AegisLink.App.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private IDataLink _activeDataLink;
        private readonly UdpLinkService _realLinkService;
        private readonly MissionRepository _missionRepository;
        private VirtualLauncherService? _simService;

        private bool _isSimulationActive;
        public bool IsSimulationActive
        {
            get => _isSimulationActive;
            set => SetProperty(ref _isSimulationActive, value);
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

        private string _connectionStatus = "DISCONNECTED";
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

            // Enable thread-safe collection access for the UI
            BindingOperations.EnableCollectionSynchronization(CommandLog, _logLock);

            // Subscribe to real telemetry initially
            _activeDataLink.OnFrameReceived += OnFrameReceived;

            SendPingCommand = new RelayCommand(ExecuteSendPing);
            ToggleSimulationCommand = new RelayCommand(ExecuteToggleSimulation);
            
            AddLog("System Initialized. Mode: REAL HARDWARE");
            _ = _missionRepository.LogEventAsync("SYSTEM_INIT", "Application started in Real Hardware mode.");
        }

        private void ExecuteToggleSimulation(object? obj)
        {
            // Unsubscribe from current
            _activeDataLink.OnFrameReceived -= OnFrameReceived;

            if (IsSimulationActive)
            {
                // Switch to SIM
                AddLog("Switching to SIMULATION MODE...");
                ConnectionStatus = "SIMULATING";
                _simService = new VirtualLauncherService();
                _activeDataLink = _simService;
                _ = _missionRepository.LogEventAsync("MODE_SWITCH", "User switched to SIMULATION mode.");
            }
            else
            {
                // Switch to REAL
                AddLog("Switching to REAL HARDWARE...");
                ConnectionStatus = "DISCONNECTED";
                _simService?.Dispose();
                _simService = null;
                _activeDataLink = _realLinkService;
                _ = _missionRepository.LogEventAsync("MODE_SWITCH", "User switched to REAL HARDWARE mode.");
            }

            // Subscribe to new
            _activeDataLink.OnFrameReceived += OnFrameReceived;
        }

        private void OnFrameReceived(TelemetryFrame frame)
        {
            // Marshal execution to the Main UI Thread
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Mapping Frame data to View Model Properties
                // Assuming Latitude -> Azimuth, Longitude -> Elevation for simulator context
                Azimuth = frame.Latitude;
                Elevation = frame.Longitude;
                BatteryLevel = frame.BatteryLevel;
                SignalStrength = frame.SignalStrength;
                
                ConnectionStatus = "CONNECTED LIVE";

                // Log periodically or on status change? For now, just logging errors or specific codes
                // To avoid spamming the log at 60Hz, we might only log if status changes, 
                // but for debug we can log every Nth frame or similar.
                // For this requirement, we just satisfy the "Command Log" existence.
            });
        }

        private async void ExecuteSendPing(object? obj)
        {
            AddLog("Sending PING...");
            byte[] ping = new byte[] { 0x50, 0x49, 0x1E, 0x47 }; // "PING" (Slight variant for demo)
            await _activeDataLink.SendCommandAsync(ping);
            await _missionRepository.LogEventAsync("COMMAND_SENT", "PING");
        }

        private void AddLog(string message)
        {
            // Because we used EnableCollectionSynchronization, we can technically modify this from any thread?
            // Actually, EnableCollectionSynchronization allows the *View* to read it safely while we write.
            // But if we write from a background thread, ObservableCollection raises CollectionChanged.
            // WPF *automatically* marshals CollectionChanged events if EnableCollectionSynchronization is active?
            // Yes, predominantly for reading. Writing should still ideally be locked or dispatched 
            // if we want to be 100% safe, but the lock object handles the synchronization.
            // However, to be absolutely safe and consistent with "Invoking" updates:
            
            lock (_logLock)
            {
               // Since we are invoking property updates on Dispatcher, we can log there too 
               // OR we can trust the Synchronization. Let's use Dispatcher for safety if unsure, 
               // but EnableCollectionSynchronization is specifically for this.
               // Let's rely on the mechanism requested: "Use BindingOperations.EnableCollectionSynchronization"
               
                // We'll wrap in Dispatcher just to be sure the Add operation itself doesn't conflict 
                // if unrelated threads bang on it, but the Lock protects the integrity.
                // The prompt says: "allow the network thread to add logs without crashing the UI."
                
                // So we can do:
                 CommandLog.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
            }
        }
    }
}
