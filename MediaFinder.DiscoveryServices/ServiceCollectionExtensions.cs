using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NReco.VideoInfo;

namespace MediaFinder.DiscoveryServices
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDiscoveryServices(this IServiceCollection services, IConfiguration configuration)
            => services!
                .AddTransient<DiscoveryRunnerService>()
                .Configure<KnownFalseArchiveExtensions>(configuration.GetSection(nameof(KnownFalseArchiveExtensions)))
                .AddTransient<DirectoryIteratorService>()
                .Configure<KnownVideoFileExtensions>(configuration.GetSection(nameof(KnownVideoFileExtensions)))
                .Configure<KnownImageFileExtensions>(configuration.GetSection(nameof(KnownImageFileExtensions)))
                .AddTransient<FileAnalyserService>()
                .AddTransient<MediaFilteringService>()
                .AddTransient<FFProbe>();
    }
}
