using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using StockManager.Domain.Entities;
using StockManager.Domain.Enums;

namespace StockManager.Infrastructure.Persistence;

public class StockDbContext : DbContext
{
    public DbSet<PhoneModel> PhoneModels => Set<PhoneModel>();
    public DbSet<Sku> Skus => Set<Sku>();

    public DbSet<StockMovement> StockMovements => Set<StockMovement>();


    public StockDbContext(DbContextOptions<StockDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // PhoneModel
        modelBuilder.Entity<PhoneModel>(e =>
        {
            e.ToTable("phone_models");
            e.HasKey(x => x.Id);
            e.Property(x => x.Brand).IsRequired().HasMaxLength(80);
            e.Property(x => x.ModelName).IsRequired().HasMaxLength(80);
            e.HasIndex(x => new { x.Brand, x.ModelName }).IsUnique();
        });

        // Sku
        modelBuilder.Entity<Sku>(e =>
        {
            e.ToTable("skus");
            e.HasKey(x => x.Id);

            e.Property(x => x.Name).IsRequired().HasMaxLength(120);

            e.Property(x => x.Cost).HasColumnType("decimal(12,2)");
            e.Property(x => x.Price).HasColumnType("decimal(12,2)");

            e.HasOne(x => x.PhoneModel)
             .WithMany()
             .HasForeignKey(x => x.PhoneModelId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // Seed inicial 
        modelBuilder.Entity<PhoneModel>().HasData(
            new PhoneModel { Id = 1, Brand = "Samsung", ModelName = "A02" },
            new PhoneModel { Id = 2, Brand = "Samsung", ModelName = "A20" }
        );

        modelBuilder.Entity<Sku>().HasData(
            // Fundas (modelo + tipo de funda)
            new Sku
            {
                Id = 1,
                Category = ProductCategory.Case,
                Name = "Funda silicona Samsung A02",
                PhoneModelId = 1,
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
                PhoneModelId = 2,
                CaseType = CaseType.Transparent,
                ProtectorType = null,
                Stock = 7,
                Cost = 1400m,
                Price = 2800m,
                Active = true
            },

            // Templados (modelo + tipo de templado)
            new Sku
            {
                Id = 3,
                Category = ProductCategory.ScreenProtector,
                Name = "Templado reforzado Samsung A02",
                PhoneModelId = 1,
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
                PhoneModelId = 2,
                CaseType = null,
                ProtectorType = ProtectorType.Privacy,
                Stock = 5,
                Cost = 1800m,
                Price = 3500m,
                Active = true
            },

            // Accesorios (sin modelo)
            new Sku
            {
                Id = 5,
                Category = ProductCategory.Accessory,
                Name = "Cargador 20W USB-C",
                PhoneModelId = null,
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
             .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => x.SkuId);
            e.HasIndex(x => x.CreatedAt);
        });



    }
}
