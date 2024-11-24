using Microsoft.EntityFrameworkCore;
using OrderRequestMicroservice.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace OrderRequestMicroservice.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }
        public DbSet<OrderRequest> OrderRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<OrderRequest>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ProductId).IsRequired();
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.Quantity).IsRequired();
                entity.Property(e => e.TotalPrice).HasPrecision(10, 2);
                entity.Property(e => e.ProductName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CreatedAt).IsRequired();
            });
        }
    }
}