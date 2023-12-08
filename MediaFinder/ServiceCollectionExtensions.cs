using MediaFinder.Views.Discovery;
using MediaFinder.Views.Export;
using MediaFinder.Views.Status;
using MediaFinder.Views;

using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Microsoft.Extensions.Hosting;
using Serilog.Core;

namespace MediaFinder
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationLogging(this IServiceCollection services, HostBuilderContext hostContext)
        {
            var log = DefaultLoggerConfiguration()
                .ReadFrom.Configuration(hostContext.Configuration)
                .CreateLogger();
            
            return services.AddSerilog(log);
        }

        public static IServiceCollection AddApplicationViews(this IServiceCollection services)
        {
            return services
                .AddSingleton<MainWindow>()
                .AddSingleton<MainWindowViewModel>()
                .AddTransient<AddSearchSettingView>()
                .AddTransient<AddSearchSettingViewModel>()
                .AddTransient<EditSearchSettingView>()
                .AddSingleton<EditSearchSettingViewModel>()
                .AddSingleton<DiscoveryViewModel>()
                .AddSingleton<ExportViewModel>()
                .AddSingleton<ProcessCompletedViewModel>()
                ;
        }

        public static Logger StartupLogger()
            => DefaultLoggerConfiguration().CreateLogger();

        private static LoggerConfiguration DefaultLoggerConfiguration()
        {
            return new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(
                    path: "logs/mediaFinder-.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 15,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}|{Level:u3}|{SourceContext}|{Message:lj}{NewLine}{Exception}");
        }
    }
}
