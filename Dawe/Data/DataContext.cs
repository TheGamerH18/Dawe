using Microsoft.EntityFrameworkCore;

namespace Dawe.Data
{
    public class DataContext : DbContext
    {
        public DbSet<Models.Movies> Movies { get; set; }
        public DbSet<Models.MovieTag> MovieTag { get; set; }
        public DbSet<Models.Series> Series { get; set; }
        public DbSet<Models.Season> Seasons { get; set; }
        public DbSet<Models.SeriesTag> SeriesTag { get; set; }
        public DbSet<Models.Episode> Episodes { get; set; }
        public DbSet<Models.File> Files { get; set; }
        public DbSet<Models.FileCategory> FileCategories { get; set; }
        public string DBPath { get; set; }

        public DataContext()
        {
            DBPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Dawe.db");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data source={DBPath}");
        }
    }
}
