using AegisLink.App.Services;
using AegisLink.App.ViewModels;
using AegisLink.Persistence;
using System.Windows;

namespace AegisLink.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public const string Version = "1.1.0.0";
        
        private UdpLinkService? _udpLinkService;
        private MissionRepository? _missionRepository;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Dependency Injection (Manual Composition Root)
            _udpLinkService = new UdpLinkService();
            _missionRepository = new MissionRepository();
            var mainViewModel = new MainViewModel(_udpLinkService, _missionRepository);

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
