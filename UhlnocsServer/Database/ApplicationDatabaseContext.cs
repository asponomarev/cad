using Microsoft.EntityFrameworkCore;
using UhlnocsServer.Calculations;
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
        public DbSet<Launch> Launches { get; set; } = null!;
        public DbSet<Calculation> Calculations { get; set; } = null!;
        public DbSet<ParametersSet> ParametersSets { get; set; } = null!;
        public DbSet<CharacteristicsSet> CharacteristicsSets { get; set; } = null!;

        public ApplicationDatabaseContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(ConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Launch>()
                .HasOne(l => l.User)
                .WithMany(u => u.Launches)
                .HasForeignKey(l => l.UserId)
                .IsRequired(true);
            modelBuilder.Entity<Launch>()
                .Property(l => l.Status)
                .HasConversion<string>();

            modelBuilder.Entity<Calculation>()
                .HasNoKey();
            modelBuilder.Entity<Calculation>()
                .HasOne(c => c.Launch)
                .WithMany()
                .HasForeignKey(c => c.LaunchId)
                .IsRequired(true);
            modelBuilder.Entity<Calculation>()
                .HasOne(c => c.Model)
                .WithMany()
                .HasForeignKey(c => c.ModelId)
                .IsRequired(true);
            modelBuilder.Entity<Calculation>()
                .HasOne(c => c.ParametersSet)
                .WithMany()
                .HasForeignKey(c => c.ParametersHash)
                .IsRequired(true);
            modelBuilder.Entity<Calculation>()
                .HasOne(c => c.CharacteristicsSet)
                .WithMany()
                .HasForeignKey(c => c.CharacteristicsHash)
                .IsRequired(false);
            modelBuilder.Entity<Calculation>()
                .Property(c => c.Status)
                .HasConversion<string>();
        }
    }
}
