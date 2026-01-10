using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using AegisLink.App.ViewModels;

namespace AegisLink.App
{
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer _scanlineTimer;
        private readonly DispatcherTimer _toastTimer;
        private readonly List<string> _commandHistory = new();
        private int _historyIndex = -1;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;

            // Scanline animation
            _scanlineTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(30) };
            _scanlineTimer.Tick += Scanline_Tick;
            _scanlineTimer.Start();

            // Toast timer
            _toastTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            _toastTimer.Tick += (s, e) => { ToastBorder.Visibility = Visibility.Collapsed; _toastTimer.Stop(); };
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as MainViewModel;
            if (vm != null)
            {
                vm.ToastRequested += ShowToast;
                vm.RxFlashRequested += FlashRxIndicator;
            }
        }

        private double _scanlineY = 0;
        private void Scanline_Tick(object? sender, EventArgs e)
        {
            _scanlineY += 2;
            if (_scanlineY > ActualHeight) _scanlineY = 0;
            System.Windows.Controls.Canvas.SetTop(ScanlineBar, _scanlineY);
        }

        public void ShowToast(string message)
        {
            ToastText.Text = message;
            ToastBorder.Visibility = Visibility.Visible;
            _toastTimer.Stop();
            _toastTimer.Start();
        }

        private void FlashRxIndicator()
        {
            RxIndicator.Fill = new SolidColorBrush(Colors.Lime);
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            timer.Tick += (s, e) => { RxIndicator.Fill = new SolidColorBrush(Color.FromRgb(51, 51, 51)); timer.Stop(); };
            timer.Start();
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (e.ClickCount == 2)
                {
                    WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
                }
                else
                {
                    DragMove();
                }
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Intentionally empty - drag handled by title bar
        }

        private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void Maximize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        private void Close_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            var vm = DataContext as MainViewModel;

            switch (e.Key)
            {
                case Key.F11:
                    WindowStyle = WindowStyle == WindowStyle.None && WindowState == WindowState.Maximized 
                        ? WindowStyle.SingleBorderWindow 
                        : WindowStyle.None;
                    WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
                    break;

                case Key.Oem3: // Tilde ~
                    vm?.ToggleTerminal();
                    if (vm?.TerminalVisibility == Visibility.Visible)
                        CommandInput.Focus();
                    break;

                case Key.S when Keyboard.Modifiers == ModifierKeys.Control:
                    ExportSnapshot(vm);
                    break;

                case Key.Escape:
                    vm?.NavigateToDashboard();
                    break;
            }
        }

        private void ExportSnapshot(MainViewModel? vm)
        {
            if (vm == null) return;
            try
            {
                var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    $"AegisLink_Snapshot_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
                var csv = $"Timestamp,Azimuth,Distance,Battery,Signal\n{DateTime.Now:O},{vm.Azimuth},{vm.Distance},{vm.BatteryLevel},{vm.SignalStrength}";
                File.WriteAllText(path, csv);
                ShowToast($"Snapshot saved: {Path.GetFileName(path)}");
                vm.AddLogEntry($"[EXPORT] Snapshot saved to Desktop");
            }
            catch (Exception ex)
            {
                ShowToast($"Export failed: {ex.Message}");
            }
        }

        private void CommandInput_KeyDown(object sender, KeyEventArgs e)
        {
            var vm = DataContext as MainViewModel;

            if (e.Key == Key.Enter)
            {
                var command = CommandInput.Text.Trim();
                if (!string.IsNullOrEmpty(command))
                {
                    if (!Regex.IsMatch(command, @"^[a-zA-Z0-9 .]*$"))
                    {
                        vm?.AddLogEntry("[ERROR] Invalid characters in command");
                        return;
                    }

                    _commandHistory.Add(command);
                    _historyIndex = _commandHistory.Count;
                    vm?.AddLogEntry($"AEGIS> {command.ToUpper()}");
                    ExecuteCommand(command.ToUpper(), vm);
                    CommandInput.Clear();
                }
            }
            else if (e.Key == Key.Up && _commandHistory.Count > 0)
            {
                if (_historyIndex > 0) _historyIndex--;
                CommandInput.Text = _commandHistory[_historyIndex];
                CommandInput.CaretIndex = CommandInput.Text.Length;
            }
            else if (e.Key == Key.Down && _commandHistory.Count > 0)
            {
                if (_historyIndex < _commandHistory.Count - 1)
                {
                    _historyIndex++;
                    CommandInput.Text = _commandHistory[_historyIndex];
                }
                else
                {
                    CommandInput.Clear();
                    _historyIndex = _commandHistory.Count;
                }
            }
        }

        private void ExecuteCommand(string command, MainViewModel? vm)
        {
            switch (command)
            {
                case "PING":
                    vm?.SendPingCommand.Execute(null);
                    break;
                case "CLEAR":
                    vm?.CommandLog.Clear();
                    vm?.AddLogEntry("[SYSTEM] Log cleared");
                    break;
                case "EXIT":
                    Application.Current.Shutdown();
                    break;
                case "STATUS":
                    vm?.AddLogEntry($"[STATUS] AZ:{vm.Azimuth:N1}° DIST:{vm.Distance:N1}km BAT:{vm.BatteryLevel}% SIG:{vm.SignalStrength:N1}dB");
                    break;
                case "RADAR":
                    vm?.NavigateToRadar();
                    break;
                case "HOME":
                    vm?.NavigateToDashboard();
                    break;
                case "HELP":
                    vm?.AddLogEntry("[HELP] PING, CLEAR, STATUS, RADAR, HOME, EXIT, HELP");
                    break;
                default:
                    vm?.AddLogEntry($"[ERROR] Unknown command: {command}");
                    break;
            }
        }
    }
}
