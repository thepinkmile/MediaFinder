using MediaFinder.Services.Search;

using Microsoft.Extensions.DependencyInjection;

using NReco.VideoInfo;

namespace MediaFinder.DiscoveryServices
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDiscoveryServices(this IServiceCollection services)
        {
            return services
                .AddTransient<SearchStageOneWorker>()
                .AddTransient<SearchStageTwoWorker>()
                .AddTransient<SearchStageThreeWorker>()
                .AddTransient<FFProbe>();
        }
    }
}
