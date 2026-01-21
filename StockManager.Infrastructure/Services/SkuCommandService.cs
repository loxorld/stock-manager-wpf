using Microsoft.EntityFrameworkCore;
using StockManager.Application.Dtos;
using StockManager.Application.Services;
using StockManager.Domain.Entities;
using StockManager.Domain.Enums;
using StockManager.Infrastructure.Persistence;

namespace StockManager.Infrastructure.Services;

public class SkuCommandService(StockDbContext db) : ISkuCommandService
{
    private readonly StockDbContext _db = db;

    public async Task<int> CreateAsync(UpsertSkuRequest r)
    {
        Validate(r);

        var sku = new Sku
        {
            Name = r.Name.Trim(),
            Category = r.Category,
            
            CaseType = r.CaseType,
            ProtectorType = r.ProtectorType,

            Stock = 0, //  stock inicial siempre 0, luego solo movimientos
            CaseStockWomen = 0,
            CaseStockMen = 0,


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
        if (r.Id is null)
            throw new ArgumentException("Id requerido para actualizar.");
        Validate(r);

        var sku = await _db.Skus.FirstOrDefaultAsync(x => x.Id == r.Id.Value)
            ?? throw new InvalidOperationException("SKU inexistente.");

        sku.Name = r.Name.Trim();
        sku.Category = r.Category;
        
        sku.CaseType = r.CaseType;
        sku.ProtectorType = r.ProtectorType;

        if (sku.Category != ProductCategory.Case)
        {
            sku.CaseStockWomen = 0;
            sku.CaseStockMen = 0;
        }

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
            
            r.CaseType = null;
            r.ProtectorType = null;
            return;
        }

        

        if (r.Category == ProductCategory.Case && r.CaseType is null)
            throw new ArgumentException("El tipo de funda es obligatorio.");

        if (r.Category == ProductCategory.ScreenProtector && r.ProtectorType is null)
            throw new ArgumentException("El tipo de templado es obligatorio.");
    }

    public async Task DeleteAsync(int id)
    {
        var sku = await _db.Skus.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new InvalidOperationException("SKU inexistente.");

        if (sku.Stock != 0)
            throw new InvalidOperationException(
                "No se puede eliminar un SKU con stock distinto de 0."
            );

        _db.Skus.Remove(sku);
        await _db.SaveChangesAsync();
    }


}
