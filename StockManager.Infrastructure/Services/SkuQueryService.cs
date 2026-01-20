using Microsoft.EntityFrameworkCore;
using StockManager.Application.Dtos;
using StockManager.Application.Services;
using StockManager.Infrastructure.Persistence;
using StockManager.Domain.Enums;

namespace StockManager.Infrastructure.Services;

public class SkuQueryService : ISkuQueryService
{
    private readonly StockDbContext _db;

    public SkuQueryService(StockDbContext db)
    {
        _db = db;
    }

    public async Task<List<SkuListItemDto>> GetAllAsync(
        string? searchText = null,
        ProductCategory? category = null,
        bool? active = null,
        int? stockMax = null)
    {
        var q = _db.Skus.AsNoTracking();

        //  búsqueda case-insensitive por Name
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var s = searchText.Trim();
            q = q.Where(x => EF.Functions.Like(x.Name, $"%{s}%"));
        }

        if (category.HasValue)
            q = q.Where(x => x.Category == category.Value);

        if (active.HasValue)
            q = q.Where(x => x.Active == active.Value);

        if (stockMax.HasValue)
            q = q.Where(x => x.Stock <= stockMax.Value);

        return await q
            .OrderBy(x => x.Name)
            .Select(x => new SkuListItemDto
            {
                Id = x.Id,
                Name = x.Name,
                Category = x.Category.ToString(),
                CategoryValue = x.Category,
                Stock = x.Stock,
                CaseStockWomen = x.CaseStockWomen,
                CaseStockMen = x.CaseStockMen,
                Price = x.Price
            })
            .ToListAsync();
    }

    public async Task<SkuDetailDto?> GetByIdAsync(int id)
    {
        return await _db.Skus
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new SkuDetailDto
            {
                Id = x.Id,
                Name = x.Name,
                Category = x.Category,
                CaseType = x.CaseType,
                ProtectorType = x.ProtectorType,
                Stock = x.Stock,
                CaseStockWomen = x.CaseStockWomen,
                CaseStockMen = x.CaseStockMen,
                Cost = x.Cost,
                Price = x.Price,
                Active = x.Active
            })
            .FirstOrDefaultAsync();
    }
}
