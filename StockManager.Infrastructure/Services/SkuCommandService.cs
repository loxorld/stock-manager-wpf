using Microsoft.EntityFrameworkCore;
using StockManager.Application.Dtos;
using StockManager.Application.Services;
using StockManager.Domain.Entities;
using StockManager.Domain.Enums;
using StockManager.Infrastructure.Persistence;

namespace StockManager.Infrastructure.Services;

public class SkuCommandService : ISkuCommandService
{
    private readonly StockDbContext _db;

    public SkuCommandService(StockDbContext db)
    {
        _db = db;
    }

    public async Task<int> CreateAsync(UpsertSkuRequest r)
    {
        Validate(r);

        var sku = new Sku
        {
            Name = r.Name.Trim(),
            Category = r.Category,
            PhoneModelId = r.PhoneModelId,
            CaseType = r.CaseType,
            ProtectorType = r.ProtectorType,

            Stock = 0, // ✅ stock inicial siempre 0, luego solo movimientos

            Cost = r.Cost,
            Price = r.Price,
            Active = r.Active
        };

        _db.Skus.Add(sku);
        await _db.SaveChangesAsync();
        return sku.Id;
    }

    public async Task UpdateAsync(UpsertSkuRequest r)
    {
        if (r.Id is null) throw new ArgumentException("Id requerido para actualizar.");
        Validate(r);

        var sku = await _db.Skus.FirstOrDefaultAsync(x => x.Id == r.Id.Value);
        if (sku == null) throw new InvalidOperationException("SKU inexistente.");

        sku.Name = r.Name.Trim();
        sku.Category = r.Category;
        sku.PhoneModelId = r.PhoneModelId;
        sku.CaseType = r.CaseType;
        sku.ProtectorType = r.ProtectorType;

        // ✅ NO tocamos sku.Stock

        sku.Cost = r.Cost;
        sku.Price = r.Price;
        sku.Active = r.Active;

        await _db.SaveChangesAsync();
    }

    private static void Validate(UpsertSkuRequest r)
    {
        if (string.IsNullOrWhiteSpace(r.Name))
            throw new ArgumentException("El nombre es obligatorio.");

        if (r.Cost < 0 || r.Price < 0)
            throw new ArgumentException("Costo y precio no pueden ser negativos.");

        if (r.Category == ProductCategory.Accessory)
        {
            r.PhoneModelId = null;
            r.CaseType = null;
            r.ProtectorType = null;
            return;
        }

        if (r.PhoneModelId is null)
            throw new ArgumentException("El modelo de celular es obligatorio.");

        if (r.Category == ProductCategory.Case && r.CaseType is null)
            throw new ArgumentException("El tipo de funda es obligatorio.");

        if (r.Category == ProductCategory.ScreenProtector && r.ProtectorType is null)
            throw new ArgumentException("El tipo de templado es obligatorio.");
    }
}
