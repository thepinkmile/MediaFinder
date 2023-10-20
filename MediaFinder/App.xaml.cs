using System.Windows;

using MediaFinder_v2.DataAccessLayer;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Serilog.Events;

using Serilog;
using CommunityToolkit.Mvvm.Messaging;
using MediaFinder_v2.Views.Executors;
using MaterialDesignThemes.Wpf;
using System.Windows.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using NReco.VideoInfo;
using MediaFinder_v2.Services.Search;
using MediaFinder_v2.Services.Export;
using MediaFinder_v2.Views;

namespace MediaFinder_v2;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{

    [STAThread]
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(
                path: "logs/mediaFinder-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 15,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}|{Level:u3}|{SourceContext}|{Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        try
        {
            using IHost host = CreateHostBuilder(args).Build();
            host.Start();

            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                var logger = host.Services.GetRequiredService<ILogger<App>>();
                logger.LogError(args.ExceptionObject as Exception, "An unexpected exception occured.");
            };

            using var scope = host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
            {
                scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
            }

            App app = new();
            app.InitializeComponent();
            app.DispatcherUnhandledException += (sender, args) =>
            {
                var logger = host.Services.GetRequiredService<ILogger<App>>();
                logger.LogError(args.Exception, "An unexpected exception occured.");
            };
            app.MainWindow = host.Services.GetRequiredService<MainWindow>();
            app.MainWindow.Visibility = Visibility.Visible;
            app.Run();
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
        .ConfigureServices((hostContext, services) =>
        {
            var log = new LoggerConfiguration()
                .ReadFrom.Configuration(hostContext.Configuration)
                .CreateLogger();
            services.AddSerilog(log);

            services.AddDbContext<AppDbContext>(ServiceLifetime.Singleton);

            services.AddTransient<SearchStageOneWorker>();
            services.AddTransient<SearchStageTwoWorker>();
            services.AddTransient<SearchStageThreeWorker>();
            services.AddTransient<ExportWorker>();

            services.AddTransient<FFProbe>();

            services.AddSingleton<MainWindow>();
            services.AddSingleton<MainWindowsViewModel>();
            services.AddSingleton<AddSearchSettingViewModel>();
            services.AddSingleton<SearchExecutorViewModel>();

            services.AddScoped<WeakReferenceMessenger>();
            services.AddScoped<IMessenger, WeakReferenceMessenger>(provider => provider.GetRequiredService<WeakReferenceMessenger>());

            services.AddScoped(_ => Current.Dispatcher);

            services.AddTransient<ISnackbarMessageQueue>(provider =>
            {
                Dispatcher dispatcher = provider.GetRequiredService<Dispatcher>();
                return new SnackbarMessageQueue(TimeSpan.FromSeconds(3.0), dispatcher);
            });
        });
}
