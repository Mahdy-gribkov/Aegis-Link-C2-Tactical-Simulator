using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.IO;
using AegisLink.App.ViewModels;

namespace AegisLink.App
{
    public partial class MainWindow : Window
    {
        private bool _isDarkTheme = true;
        private WindowState _previousWindowState;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DrawRadarGrid();
        }

        private void DrawRadarGrid()
        {
            RadarCanvas.Children.Clear();
            var centerX = RadarCanvas.ActualWidth / 2;
            var centerY = RadarCanvas.ActualHeight / 2;
            var maxRadius = Math.Min(centerX, centerY) - 10;

            if (maxRadius <= 0) return;

            // Concentric circles
            for (int i = 1; i <= 4; i++)
            {
                var radius = maxRadius * i / 4;
                var circle = new Ellipse
                {
                    Width = radius * 2,
                    Height = radius * 2,
                    Stroke = new SolidColorBrush(Color.FromArgb(80, 0, 255, 0)),
                    StrokeThickness = 1
                };
                System.Windows.Controls.Canvas.SetLeft(circle, centerX - radius);
                System.Windows.Controls.Canvas.SetTop(circle, centerY - radius);
                RadarCanvas.Children.Add(circle);
            }

            // Crosshairs
            var hLine = new Line
            {
                X1 = 10, Y1 = centerY,
                X2 = RadarCanvas.ActualWidth - 10, Y2 = centerY,
                Stroke = new SolidColorBrush(Color.FromArgb(60, 0, 255, 0)),
                StrokeThickness = 1
            };
            var vLine = new Line
            {
                X1 = centerX, Y1 = 10,
                X2 = centerX, Y2 = RadarCanvas.ActualHeight - 10,
                Stroke = new SolidColorBrush(Color.FromArgb(60, 0, 255, 0)),
                StrokeThickness = 1
            };
            RadarCanvas.Children.Add(hLine);
            RadarCanvas.Children.Add(vLine);

            // Center dot
            var centerDot = new Ellipse
            {
                Width = 6, Height = 6,
                Fill = new SolidColorBrush(Color.FromRgb(0, 255, 255))
            };
            System.Windows.Controls.Canvas.SetLeft(centerDot, centerX - 3);
            System.Windows.Controls.Canvas.SetTop(centerDot, centerY - 3);
            RadarCanvas.Children.Add(centerDot);
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MasterArm_Checked(object sender, RoutedEventArgs e)
        {
            MasterArmToggle.Content = "◉ ARMED";
            MasterArmToggle.Foreground = new SolidColorBrush(Colors.Red);
            FireButton.IsEnabled = true;
            FireButton.Background = new SolidColorBrush(Color.FromRgb(100, 0, 0));

            var vm = DataContext as MainViewModel;
            vm?.AddLogEntry("[SAFETY] MASTER ARM ENABLED - WEAPONS HOT");
        }

        private void MasterArm_Unchecked(object sender, RoutedEventArgs e)
        {
            MasterArmToggle.Content = "◉ SAFE";
            MasterArmToggle.Foreground = new SolidColorBrush(Color.FromRgb(255, 68, 68));
            FireButton.IsEnabled = false;
            FireButton.Background = new SolidColorBrush(Color.FromRgb(68, 0, 0));

            var vm = DataContext as MainViewModel;
            vm?.AddLogEntry("[SAFETY] MASTER ARM DISABLED - WEAPONS SAFE");
        }

        private void Fire_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as MainViewModel;
            vm?.AddLogEntry("[FIRE CONTROL] *** ENGAGE COMMAND TRANSMITTED ***");
            System.Media.SystemSounds.Exclamation.Play();
        }

        private void Terminal_Checked(object sender, RoutedEventArgs e)
        {
            TerminalToggle.Content = "▲ COMMAND TERMINAL";
        }

        private void Terminal_Unchecked(object sender, RoutedEventArgs e)
        {
            TerminalToggle.Content = "▼ COMMAND TERMINAL";
        }

        private void CommandInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var command = CommandInput.Text.Trim().ToUpper();
                var vm = DataContext as MainViewModel;

                if (!string.IsNullOrEmpty(command))
                {
                    vm?.AddLogEntry($"AEGIS> {command}");
                    ExecuteCommand(command, vm);
                    CommandInput.Clear();
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
                    vm?.AddLogEntry($"[STATUS] Battery: {vm.BatteryLevel}% | Signal: {vm.SignalStrength:N1} dB");
                    break;
                case "HELP":
                    vm?.AddLogEntry("[HELP] Commands: PING, CLEAR, STATUS, EXIT, HELP");
                    break;
                default:
                    vm?.AddLogEntry($"[ERROR] Unknown command: {command}");
                    break;
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            DrawRadarGrid();
        }
    }
}
