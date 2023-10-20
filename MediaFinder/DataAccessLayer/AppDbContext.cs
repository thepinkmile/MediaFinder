using MediaFinder_v2.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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

            // Handle datetimes in SQLite src: https://blog.dangl.me/archive/handling-datetimeoffset-in-sqlite-with-entity-framework-core/
            if (this.Database.IsSqlite())
            {
                // SQLite does not have proper support for DateTimeOffset via Entity Framework Core, see the limitations
                // here: https://docs.microsoft.com/en-us/ef/core/providers/sqlite/limitations#query-limitations
                // To work around this, when the Sqlite database provider is used, all model properties of type DateTimeOffset
                // use the DateTimeOffsetToBinaryConverter
                // Based on: https://github.com/aspnet/EntityFrameworkCore/issues/10784#issuecomment-415769754
                // This only supports millisecond precision, but should be sufficient for most use cases.
                foreach (var entityType in modelBuilder.Model.GetEntityTypes())
                {
                    var properties = entityType.ClrType.GetProperties().Where(p => p.PropertyType == typeof(DateTimeOffset)
                                                                                || p.PropertyType == typeof(DateTimeOffset?));
                    foreach (var property in properties)
                    {
                        modelBuilder
                            .Entity(entityType.Name)
                            .Property(property.Name)
                            .HasConversion(new DateTimeOffsetToBinaryConverter()); // The converter!
                    }
                }
            }

            modelBuilder.Entity<SearchSettings>(e => e.HasData(
                    new SearchSettings
                    {
                        Id = 1,
                        Name = "Default",
                        Recursive = true,
                        ExtractArchives = false,
                        ExtractionDepth = null,
                        PerformDeepAnalysis = false,
                    },
                    new SearchSettings
                    {
                        Id = 2,
                        Name = "Testing",
                        Recursive = true,
                        ExtractArchives = true,
                        ExtractionDepth = 5,
                        PerformDeepAnalysis = true,
                        MinImageWidth = 200,
                        MinImageHeight = 200,
                        MinVideoWidth = 600,
                        MinVideoHeight = 300
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
