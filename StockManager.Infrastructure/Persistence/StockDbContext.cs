using Microsoft.EntityFrameworkCore;
using StockManager.Domain.Entities;
using StockManager.Domain.Enums;

namespace StockManager.Infrastructure.Persistence;

public class StockDbContext : DbContext
{
    
    public DbSet<Sku> Skus => Set<Sku>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    public StockDbContext(DbContextOptions<StockDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        

        // Sku
        modelBuilder.Entity<Sku>(e =>
        {
            e.ToTable("skus");
            e.HasKey(x => x.Id);

            e.Property(x => x.Name).IsRequired().HasMaxLength(120);

            e.Property(x => x.Cost).HasColumnType("decimal(12,2)");
            e.Property(x => x.Price).HasColumnType("decimal(12,2)");

            
        });

        

        modelBuilder.Entity<StockMovement>(e =>
        {
            e.ToTable("stock_movements");
            e.HasKey(x => x.Id);

            e.Property(x => x.Note).HasMaxLength(250);

            e.HasOne(x => x.Sku)
             .WithMany()
             .HasForeignKey(x => x.SkuId)
             .OnDelete(DeleteBehavior.Cascade); //  antes estaba Restrict

            e.HasIndex(x => x.SkuId);
            e.HasIndex(x => x.CreatedAt);
        });

    }
}
