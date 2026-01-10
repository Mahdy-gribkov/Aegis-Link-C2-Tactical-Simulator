using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System;
using AegisLink.App.ViewModels;

namespace AegisLink.App.Views
{
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
            Loaded += DashboardView_Loaded;
            SizeChanged += DashboardView_SizeChanged;
        }

        private void DashboardView_Loaded(object sender, RoutedEventArgs e)
        {
            DrawMiniRadar();
        }

        private void DashboardView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawMiniRadar();
        }

        private void DrawMiniRadar()
        {
            MiniRadarCanvas.Children.Clear();
            var centerX = MiniRadarCanvas.ActualWidth / 2;
            var centerY = MiniRadarCanvas.ActualHeight / 2;
            var maxRadius = Math.Min(centerX, centerY) - 5;

            if (maxRadius <= 0) return;

            for (int i = 1; i <= 3; i++)
            {
                var radius = maxRadius * i / 3;
                var circle = new Ellipse
                {
                    Width = radius * 2,
                    Height = radius * 2,
                    Stroke = new SolidColorBrush(Color.FromArgb(60, 0, 255, 0)),
                    StrokeThickness = 1
                };
                Canvas.SetLeft(circle, centerX - radius);
                Canvas.SetTop(circle, centerY - radius);
                MiniRadarCanvas.Children.Add(circle);
            }

            var centerDot = new Ellipse { Width = 4, Height = 4, Fill = new SolidColorBrush(Colors.Cyan) };
            Canvas.SetLeft(centerDot, centerX - 2);
            Canvas.SetTop(centerDot, centerY - 2);
            MiniRadarCanvas.Children.Add(centerDot);
        }

        private void MiniRadar_Click(object sender, MouseButtonEventArgs e)
        {
            var vm = DataContext as MainViewModel;
            vm?.NavigateToRadar();
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
    }
}
