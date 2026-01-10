using AegisLink.App.Services;
using AegisLink.App.ViewModels;
using System.Windows;

namespace AegisLink.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private UdpLinkService? _udpLinkService;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Dependency Injection (Manual Composition Root)
            _udpLinkService = new UdpLinkService();
            var mainViewModel = new MainViewModel(_udpLinkService);

            var mainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };

            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _udpLinkService?.Dispose();
            base.OnExit(e);
        }
    }
}
