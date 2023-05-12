using Microsoft.EntityFrameworkCore;
using UhlnocsServer.Calculations;
using UhlnocsServer.Models;
using UhlnocsServer.Users;
using UhlnocsServer.Utils;

namespace UhlnocsServer.Database
{
    public class ApplicationDatabaseContext : DbContext
    {
        private static readonly string ConnectionString = $"Host={PropertiesHolder.DatabaseSettings["host"]};" +
                                                          $"Port={PropertiesHolder.DatabaseSettings["port"]};" +
                                                          $"Database={PropertiesHolder.DatabaseSettings["database"]};" +
                                                          $"Username={PropertiesHolder.DatabaseSettings["username"]};" +
                                                          $"Password={PropertiesHolder.DatabaseSettings["password"]}";
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
            // configuring dependencies between entities in db and conversion of enum fields in classes to strings in db
            modelBuilder.Entity<Launch>()
                .HasOne(l => l.User)
                .WithMany(u => u.Launches)
                .HasForeignKey(l => l.UserId)
                .IsRequired(true);
            modelBuilder.Entity<Launch>()
                .Property(l => l.Status)
                .HasConversion<string>();

            modelBuilder.Entity<Calculation>()
                .HasOne(c => c.Launch)
                .WithMany(l => l.Calculations)
                .HasForeignKey(c => c.LaunchId)
                .IsRequired(true);
            modelBuilder.Entity<Calculation>()
                .HasOne(c => c.Model)
                .WithMany(m => m.Calculations)
                .HasForeignKey(c => c.ModelId)
                .IsRequired(true);
            modelBuilder.Entity<Calculation>()
                .HasOne(c => c.ParametersSet)
                .WithMany(ps => ps.Calculations)
                .HasForeignKey(c => c.ParametersHash)
                .IsRequired(true);
            modelBuilder.Entity<Calculation>()
                .HasOne(c => c.CharacteristicsSet)
                .WithMany(cs => cs.Calculations)
                .HasForeignKey(c => c.CharacteristicsId)
                .IsRequired(false);
            modelBuilder.Entity<Calculation>()
                .Property(c => c.Status)
                .HasConversion<string>();
        }
    }
}
