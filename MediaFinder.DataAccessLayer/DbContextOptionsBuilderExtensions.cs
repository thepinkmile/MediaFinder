using Microsoft.EntityFrameworkCore;

namespace MediaFinder.DataAccessLayer
{
    public static class DbContextOptionsBuilderExtensions
    {
        public static DbContextOptionsBuilder<MediaFinderDbContext> ConfigureMediaFinderDatabase(
            this DbContextOptionsBuilder<MediaFinderDbContext> builder)
        {
            return builder
                    .UseSqlite(
                        $"foreign keys=true;Data Source=mediaFinder.db", // 'foreign keys = true' enforces cascade deletes in the database
                        dbOptions => dbOptions.MigrationsAssembly(typeof(MediaFinderDbContext).Assembly.FullName))
                    .UseLazyLoadingProxies();
        }

        public static DbContextOptionsBuilder ConfigureMediaFinderDatabase(
            this DbContextOptionsBuilder builder,
            string databaseName = "mediaFinder")
        {
            return builder
                    .UseSqlite(
                        $"foreign keys=true;Data Source={databaseName}.db", // 'foreign keys = true' enforces cascade deletes in the database
                        dbOptions => dbOptions.MigrationsAssembly(typeof(MediaFinderDbContext).Assembly.FullName))
                    .UseLazyLoadingProxies();
        }
    }
}
