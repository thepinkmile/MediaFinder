using Microsoft.Extensions.DependencyInjection;

namespace MediaFinder.DataAccessLayer
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMediaFinderDatabase(this IServiceCollection services, string databaseName = "mediaFinder")
        {
            return services.AddDbContext<MediaFinderDbContext>(
                optionsBuilder => optionsBuilder.ConfigureMediaFinderDatabase(databaseName));
        }


    }
}
