using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MediaFinder.DataAccessLayer
{
    public class MediaFinderContextFactory : IDesignTimeDbContextFactory<MediaFinderDbContext>
    {
        public MediaFinderDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<MediaFinderDbContext>()
                .ConfigureMediaFinderDatabase();
            return new MediaFinderDbContext(optionsBuilder.Options);
        }
    }
}
