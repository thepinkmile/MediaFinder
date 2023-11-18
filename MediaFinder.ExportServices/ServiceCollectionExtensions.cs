using MediaFinder.Services.Export;

using Microsoft.Extensions.DependencyInjection;

namespace MediaFinder.ExportServices
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddExportServices(this IServiceCollection services)
        {
            return services.AddTransient<ExportWorker>();
        }
    }
}
