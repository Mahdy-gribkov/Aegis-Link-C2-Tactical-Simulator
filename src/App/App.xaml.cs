using AegisLink.App.Services;
using AegisLink.App.ViewModels;
using AegisLink.App.Views;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace AegisLink.App;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;
    private ILogger? _logger;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Global Exception Handlers
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        DispatcherUnhandledException += OnDispatcherException;

        // Configure DI
        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();

        _logger = Services.GetRequiredService<ILogger>();
        _logger.Info("[BOOT] Dependency Injection configured");

        // Launch Shell
        var shell = Services.GetRequiredService<ShellView>();
        shell.Show();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Services (Singletons)
        services.AddSingleton<ILogger, FileLogger>();
        services.AddSingleton<IConfigService, ConfigService>();
        services.AddSingleton<ISoundService, SoundService>();
        services.AddSingleton<IScenarioService, ScenarioService>();

        // ViewModels (Transient)
        services.AddTransient<ShellViewModel>();

        // Views
        services.AddTransient<ShellView>();
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var ex = e.ExceptionObject as Exception;
        _logger?.Error("[FATAL] Unhandled exception", ex);
        WriteCrashLog(ex);
        ShowCrashDialog(ex);
    }

    private void OnDispatcherException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        _logger?.Error("[FATAL] Dispatcher exception", e.Exception);
        WriteCrashLog(e.Exception);
        ShowCrashDialog(e.Exception);
        e.Handled = true;
    }

    private void WriteCrashLog(Exception? ex)
    {
        try
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var crashPath = Path.Combine(appData, "AegisLink", "logs", "crash.log");
            Directory.CreateDirectory(Path.GetDirectoryName(crashPath)!);
            File.AppendAllText(crashPath, $"\n[{DateTime.Now:O}] CRASH\n{ex}\n");
        }
        catch { }
    }

    private void ShowCrashDialog(Exception? ex)
    {
        MessageBox.Show(
            $"SYSTEM ERROR\n\nAegis-Link encountered a critical failure.\n\nDetails:\n{ex?.Message ?? "Unknown error"}\n\nA crash log has been saved.",
            "Aegis-Link - Fatal Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _logger?.Info("[SHUTDOWN] Application exiting");
        base.OnExit(e);
    }
}
