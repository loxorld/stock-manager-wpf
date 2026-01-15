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

        // Seed inicial
        

        modelBuilder.Entity<Sku>().HasData(
            new Sku
            {
                Id = 1,
                Category = ProductCategory.Case,
                Name = "Funda silicona Samsung A02",
                
                CaseType = CaseType.Silicone,
                ProtectorType = null,
                Stock = 10,
                Cost = 1500m,
                Price = 3000m,
                Active = true
            },
            new Sku
            {
                Id = 2,
                Category = ProductCategory.Case,
                Name = "Funda transparente Samsung A20",
                
                CaseType = CaseType.Transparent,
                ProtectorType = null,
                Stock = 7,
                Cost = 1400m,
                Price = 2800m,
                Active = true
            },
            new Sku
            {
                Id = 3,
                Category = ProductCategory.ScreenProtector,
                Name = "Templado reforzado Samsung A02",
               
                CaseType = null,
                ProtectorType = ProtectorType.Reinforced,
                Stock = 12,
                Cost = 1200m,
                Price = 2500m,
                Active = true
            },
            new Sku
            {
                Id = 4,
                Category = ProductCategory.ScreenProtector,
                Name = "Templado anti-espía Samsung A20",
                
                CaseType = null,
                ProtectorType = ProtectorType.Privacy,
                Stock = 5,
                Cost = 1800m,
                Price = 3500m,
                Active = true
            },
            new Sku
            {
                Id = 5,
                Category = ProductCategory.Accessory,
                Name = "Cargador 20W USB-C",
                
                CaseType = null,
                ProtectorType = null,
                Stock = 6,
                Cost = 4000m,
                Price = 7500m,
                Active = true
            }
        );

        modelBuilder.Entity<StockMovement>(e =>
        {
            e.ToTable("stock_movements");
            e.HasKey(x => x.Id);

            e.Property(x => x.Note).HasMaxLength(250);

            e.HasOne(x => x.Sku)
             .WithMany()
             .HasForeignKey(x => x.SkuId)
             .OnDelete(DeleteBehavior.Cascade); // ✅ antes estaba Restrict

            e.HasIndex(x => x.SkuId);
            e.HasIndex(x => x.CreatedAt);
        });

    }
}
