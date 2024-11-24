using Microsoft.EntityFrameworkCore;
using UsersMicroservice.Models;

namespace UsersMicroservice.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(e => e.Password)
                    .IsRequired();
                entity.Property(e => e.Role)
                    .IsRequired()
                    .HasDefaultValue("USER");

                entity.HasIndex(e => e.Email)
                    .IsUnique();
            });
        }
    }
}