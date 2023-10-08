using MediaFinder_v2.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace MediaFinder_v2.DataAccessLayer
{
    public class AppDbContext : DbContext
    {
        public DbSet<AppSettingValue> AppSettings => Set<AppSettingValue>();
        public DbSet<SearchSettings> SearchSettings => Set<SearchSettings>();
        public DbSet<FileDetails> FileDetails => Set<FileDetails>();
        public DbSet<FileProperty> FileProperties => Set<FileProperty>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("foreign keys=true;Data Source=mediaFinder.db");
            optionsBuilder.UseLazyLoadingProxies();
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SearchSettings>(e => e.HasData(
                    new SearchSettings
                    {
                        Id = 1,
                        Name = "Default",
                        Recursive = true,
                        ExtractArchives = false,
                        ExtractionDepth = null,
                        PerformDeepAnalysis = false
                    },
                    new SearchSettings
                    {
                        Id = 2,
                        Name = "Testing",
                        Recursive = true,
                        ExtractArchives = true,
                        ExtractionDepth = 5,
                        PerformDeepAnalysis = true
                    }
                    ));

            modelBuilder.Entity<SearchDirectory>(e => e.HasData(
                    new SearchDirectory
                    {
                        Id = 1,
                        Path = "C:\\Users\\User\\Pictures",
                        SettingsId = 1,
                    },
                    new SearchDirectory
                    {
                        Id = 2,
                        Path = "C:\\TEMP\\Source",
                        SettingsId = 2
                    }
                    ));
        }
    }
}
