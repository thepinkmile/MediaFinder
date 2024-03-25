using CommunityToolkit.Mvvm.Messaging;

using MediaFinder.Controls.Wpf;
using MediaFinder.DataAccessLayer;
using MediaFinder.DiscoveryServices;
using MediaFinder.ExportServices;
using MediaFinder.Helpers;
using MediaFinder.Logging;

using MediaFinder.Views;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Serilog;

using System.Windows;

namespace MediaFinder;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    [STAThread]
    private static void Main(string[] args)
    {
        MainAsync(args, CancellationToken.None).GetAwaiter().GetResult();
    }

    private static async Task MainAsync(string[] args, CancellationToken cancellationToken = default)
    {
        // ensure we get some logs for start-up in case CreateHostBuilder fails
        Log.Logger = ServiceCollectionExtensions.StartupLogger();

        try
        {
            using IHost host = CreateHostBuilder(args).Build();
            await host.StartAsync(cancellationToken).ConfigureAwait(true);

            var logger = host.Services.GetRequiredService<ILogger<App>>();
            AppDomain.CurrentDomain.UnhandledException += (sender, args) => logger.UnhandledException(args.ExceptionObject as Exception);

            host.Services.ApplyDatabaseMigrations();

            App app = new();
            app.InitializeComponent();
            app.DispatcherUnhandledException += (sender, args) => logger.UnhandledException(args.Exception);
            app.MainWindow = host.Services.GetRequiredService<MainWindow>();
            app.MainWindow.Visibility = Visibility.Visible;
            app.Run();

            await host.StopAsync(cancellationToken).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly.");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((hostBuilderContext, configurationBuilder)
            => configurationBuilder.AddUserSecrets(typeof(App).Assembly))
        .ConfigureServices((hostContext, services)
            => services
                .AddApplicationLogging(hostContext)
                .AddMediaFinderDatabase()
                .AddDiscoveryServices()
                .AddExportServices()
                .AddApplicationViews()
                .AddMessenger<WeakReferenceMessenger>()
                .AddSnackBarMessaging());
}
