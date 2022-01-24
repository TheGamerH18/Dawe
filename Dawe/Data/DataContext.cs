using Microsoft.EntityFrameworkCore;

namespace Dawe.Data
{
    public class DataContext : DbContext
    {
        public DbSet<Models.Movies>? Movies { get; set; }
        public DbSet<Models.MovieTags>? Tags { get; set; }
        public DbSet<Models.Show>? Shows { get; set; }
        public DbSet<Models.Episode>? Episodes { get; set;}
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
