using Microsoft.EntityFrameworkCore;

namespace Dawe.Data
{
    public class DataContext : DbContext
    {
        public DbSet<Models.Movies>? Movies { get; set; }
        public string DBPath { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            DBPath = System.IO.Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Dawe.db");
            optionsBuilder.UseSqlite($"Data source={DBPath}");
        }
    }
}
