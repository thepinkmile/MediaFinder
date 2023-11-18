using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MediaFinder.DataAccessLayer
{
    public static class ServiceProviderExtensions
    {
        public static void ApplyDatabaseMigrations(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
            {
                scope.ServiceProvider.GetRequiredService<MediaFinderDbContext>().Database.Migrate();
            }
        }
    }
}
