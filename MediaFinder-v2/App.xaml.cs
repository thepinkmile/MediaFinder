using CommunityToolkit.Mvvm.Messaging;

using MaterialDesignThemes.Wpf;

using MediaFinder_v2.DataAccessLayer;
using MediaFinder_v2.DataAccessLayer.Models;
using MediaFinder_v2.Services;
using MediaFinder_v2.Views.Executors;
using MediaFinder_v2.Views.SearchSettings;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using System.Windows;
using System.Windows.Threading;

namespace MediaFinder_v2;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    [STAThread]
    public static void Main(string[] args)
    {
        using IHost host = CreateHostBuilder(args).Build();
        host.Start();

        using (var scope = host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
        using (var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>())
        {
            ctx.Database.Migrate();
        }

        App app = new();
        app.InitializeComponent();
        app.MainWindow = host.Services.GetRequiredService<MainWindow>();
        app.MainWindow.Visibility = Visibility.Visible;
        app.Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((hostBuilderContext, configurationBuilder)
            => configurationBuilder.AddUserSecrets(typeof(App).Assembly))
        .ConfigureServices((hostContext, services) =>
        {
            services.AddDbContext<AppDbContext>();

            services.AddScoped<MainWindow>();
            services.AddSingleton<MainWindowsViewModel>();
            services.AddSingleton<SearchSettingsViewModel>();
            services.AddSingleton<AddSearchSettingViewModel>();
            services.AddSingleton<SearchExecutorViewModel>();

            services.AddSingleton<IMediaDetector, ArchiveDetector>();
            services.AddSingleton<IMediaDetector, VideoDetector>();
            services.AddSingleton<IMediaDetector, ImageDetector>();
            services.AddSingleton<MediaLocator>();

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
