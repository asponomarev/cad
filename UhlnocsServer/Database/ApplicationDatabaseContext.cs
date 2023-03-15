using Microsoft.EntityFrameworkCore;
using UhlnocsServer.Models;
using UhlnocsServer.Users;
using static UhlnocsServer.Utils.PropertiesHolder;

namespace UhlnocsServer.Database
{
    public class ApplicationDatabaseContext : DbContext
    {
        private static readonly string ConnectionString = $"Host={DatabaseSettings["host"]};" +
                                                          $"Port={DatabaseSettings["port"]};" +
                                                          $"Database={DatabaseSettings["database"]};" +
                                                          $"Username={DatabaseSettings["username"]};" +
                                                          $"Password={DatabaseSettings["password"]}";
        public DbSet<User> Users { get; set; } = null!;

        public DbSet<Model> Models { get; set; } = null!;

        public ApplicationDatabaseContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(ConnectionString);
        }
    }
}
