using Microsoft.EntityFrameworkCore;
using InventoryMicroservice.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace InventoryMicroservice.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Product> Products { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(500);
                entity.Property(e => e.Price)
                    .IsRequired()
                    .HasPrecision(10, 2);
                entity.Property(e => e.Quantity)
                    .IsRequired();
                entity.Property(e => e.CreatedAt)
                    .IsRequired();
                entity.Property(e => e.UpdatedAt)
                    .IsRequired();
            });
        }
    }
}