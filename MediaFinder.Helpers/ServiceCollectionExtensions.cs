using CommunityToolkit.Mvvm.Messaging;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MediaFinder.Helpers
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMessenger<TMessenger>(this IServiceCollection services)
            where TMessenger : class, IMessenger
        {
            services.TryAddScoped<TMessenger>();
            services.TryAddScoped<IMessenger>(provider => provider.GetRequiredService<TMessenger>());
            return services;
        }
    }
}
