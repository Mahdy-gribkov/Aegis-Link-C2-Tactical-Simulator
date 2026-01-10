using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using AegisLink.App.ViewModels;

namespace AegisLink.App.Views
{
    public partial class RadarView : UserControl
    {
        private readonly DispatcherTimer _blipTimer;

        public RadarView()
        {
            InitializeComponent();
            Loaded += RadarView_Loaded;
            SizeChanged += RadarView_SizeChanged;

            _blipTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
            _blipTimer.Tick += BlipTimer_Tick;
            _blipTimer.Start();
        }

        private void RadarView_Loaded(object sender, RoutedEventArgs e)
        {
            DrawFullRadar();
            Focus();
        }

        private void RadarView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawFullRadar();
        }

        private void DrawFullRadar()
        {
            FullRadarCanvas.Children.Clear();
            var centerX = FullRadarCanvas.ActualWidth / 2;
            var centerY = FullRadarCanvas.ActualHeight / 2;
            var maxRadius = Math.Min(centerX, centerY) - 20;

            if (maxRadius <= 0) return;

            // Concentric circles
            for (int i = 1; i <= 5; i++)
            {
                var radius = maxRadius * i / 5;
                var circle = new Ellipse
                {
                    Width = radius * 2,
                    Height = radius * 2,
                    Stroke = new SolidColorBrush(Color.FromArgb(100, 0, 255, 0)),
                    StrokeThickness = 1
                };
                Canvas.SetLeft(circle, centerX - radius);
                Canvas.SetTop(circle, centerY - radius);
                FullRadarCanvas.Children.Add(circle);

                // Range labels
                var label = new TextBlock
                {
                    Text = $"{i * 20}km",
                    FontSize = 9,
                    Foreground = new SolidColorBrush(Color.FromArgb(120, 0, 255, 0)),
                    FontFamily = new FontFamily("Consolas")
                };
                Canvas.SetLeft(label, centerX + radius + 5);
                Canvas.SetTop(label, centerY - 7);
                FullRadarCanvas.Children.Add(label);
            }

            // Crosshairs
            var hLine = new Line
            {
                X1 = 20, Y1 = centerY,
                X2 = FullRadarCanvas.ActualWidth - 20, Y2 = centerY,
                Stroke = new SolidColorBrush(Color.FromArgb(80, 0, 255, 0)),
                StrokeThickness = 1
            };
            var vLine = new Line
            {
                X1 = centerX, Y1 = 20,
                X2 = centerX, Y2 = FullRadarCanvas.ActualHeight - 20,
                Stroke = new SolidColorBrush(Color.FromArgb(80, 0, 255, 0)),
                StrokeThickness = 1
            };
            FullRadarCanvas.Children.Add(hLine);
            FullRadarCanvas.Children.Add(vLine);

            // Diagonal lines
            for (int angle = 45; angle < 360; angle += 45)
            {
                if (angle == 90 || angle == 180 || angle == 270) continue;
                var rad = angle * Math.PI / 180;
                var line = new Line
                {
                    X1 = centerX, Y1 = centerY,
                    X2 = centerX + maxRadius * Math.Cos(rad),
                    Y2 = centerY + maxRadius * Math.Sin(rad),
                    Stroke = new SolidColorBrush(Color.FromArgb(40, 0, 255, 0)),
                    StrokeThickness = 1
                };
                FullRadarCanvas.Children.Add(line);
            }

            // Center dot
            var centerDot = new Ellipse { Width = 8, Height = 8, Fill = new SolidColorBrush(Colors.Cyan) };
            Canvas.SetLeft(centerDot, centerX - 4);
            Canvas.SetTop(centerDot, centerY - 4);
            FullRadarCanvas.Children.Add(centerDot);
        }

        private void BlipTimer_Tick(object? sender, EventArgs e)
        {
            var vm = DataContext as MainViewModel;
            if (vm == null || !vm.IsSimulationActive) 
            {
                RadarBlip.Visibility = Visibility.Collapsed;
                return;
            }

            RadarBlip.Visibility = Visibility.Visible;

            var centerX = FullRadarCanvas.ActualWidth / 2;
            var centerY = FullRadarCanvas.ActualHeight / 2;
            var maxRadius = Math.Min(centerX, centerY) - 30;

            var azimuthRad = vm.Azimuth * Math.PI / 180;
            var distance = vm.Distance / 100.0 * maxRadius;

            var blipX = centerX + distance * Math.Cos(azimuthRad - Math.PI / 2);
            var blipY = centerY + distance * Math.Sin(azimuthRad - Math.PI / 2);

            Canvas.SetLeft(RadarBlip, blipX - 6);
            Canvas.SetTop(RadarBlip, blipY - 6);
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as MainViewModel;
            vm?.NavigateToDashboard();
        }

        private void RadarView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                var vm = DataContext as MainViewModel;
                vm?.NavigateToDashboard();
            }
        }
    }
}
