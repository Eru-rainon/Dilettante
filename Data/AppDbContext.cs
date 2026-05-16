using Microsoft.EntityFrameworkCore;
using Dilettante.Models;
using System.IO;

namespace Dilettante.Data
{
    class AppDbContext : DbContext
    {
        public DbSet<Game> Games { get; set; }
        public DbSet<Achievement> Achievements { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Dilettante.db");
            options.UseSqlite($"Data Source={dbPath}");
        }
    }
}