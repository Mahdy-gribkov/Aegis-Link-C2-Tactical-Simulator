using System;
using System.IO;
using System.Windows;
using AegisLink.App.ViewModels;

namespace AegisLink.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _isDarkTheme = true;
        private WindowState _previousWindowState;
        private WindowStyle _previousWindowStyle;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ToggleTheme_Click(object sender, RoutedEventArgs e)
        {
            var mergedDicts = Application.Current.Resources.MergedDictionaries;
            mergedDicts.Clear();

            if (_isDarkTheme)
            {
                mergedDicts.Add(new ResourceDictionary { Source = new Uri("Themes/Light.xaml", UriKind.Relative) });
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(245, 245, 245));
            }
            else
            {
                mergedDicts.Add(new ResourceDictionary { Source = new Uri("Themes/Dark.xaml", UriKind.Relative) });
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(10, 10, 10));
            }

            _isDarkTheme = !_isDarkTheme;
        }

        private void FullScreen_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized && WindowStyle == WindowStyle.None)
            {
                WindowState = _previousWindowState;
                WindowStyle = _previousWindowStyle;
            }
            else
            {
                _previousWindowState = WindowState;
                _previousWindowStyle = WindowStyle;
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
            }
        }

        private void ExportLogs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var vm = DataContext as MainViewModel;
                if (vm?.CommandLog == null) return;

                var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var filename = $"AegisLink_Log_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                var filepath = Path.Combine(desktopPath, filename);

                var csvContent = "Timestamp,Message\n";
                foreach (var entry in vm.CommandLog)
                {
                    csvContent += $"{DateTime.Now:yyyy-MM-dd HH:mm:ss},\"{entry}\"\n";
                }

                File.WriteAllText(filepath, csvContent);
                MessageBox.Show($"Logs exported to:\n{filepath}", "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
