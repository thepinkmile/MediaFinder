using System.Windows;
using System.Windows.Threading;

using MaterialDesignThemes.Wpf;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MediaFinder.Controls.Wpf
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSnackBarMessaging(this IServiceCollection services, TimeSpan? messageDuration = null)
        {
            messageDuration ??= TimeSpan.FromSeconds(3.0);
            services.TryAddScoped(_ => Application.Current.Dispatcher);
            services.TryAddTransient<ISnackbarMessageQueue>(provider =>
            {
                Dispatcher dispatcher = provider.GetRequiredService<Dispatcher>();
                return new SnackbarMessageQueue(messageDuration.Value, dispatcher);
            });
            return services;
        }
    }
}
