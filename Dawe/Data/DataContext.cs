using Microsoft.EntityFrameworkCore;

namespace Dawe.Data
{
    public class DataContext : DbContext
    {
        private readonly ILogger<DataContext> _logger;

        public DbSet<Models.Movies>? Movies { get; set; }
        public DbSet<Models.Show>? Shows { get; set; }
        public DbSet<Models.Episode>? Episodes { get; set;}
        public string DBPath { get; set; }

        public DataContext(ILogger<DataContext> logger1)
        {
            _logger = logger1;

            DBPath = System.IO.Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Dawe.db");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data source={DBPath}");
            _logger.LogInformation($"Database Loaded from {DBPath}");
        }
    }
}
