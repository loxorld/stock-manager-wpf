using Microsoft.EntityFrameworkCore;
using StockManager.Application.Dtos;
using StockManager.Application.Services;
using StockManager.Infrastructure.Persistence;
using StockManager.Infrastructure.Time;

namespace StockManager.Infrastructure.Services;

public class StockMovementQueryService(StockDbContext db) : IStockMovementQueryService
{
    private readonly StockDbContext _db = db;

    public async Task<StockMovementListItemDto?> GetLastBySkuAsync(int skuId)
    {
        var item = await _db.StockMovements
            .AsNoTracking()
            .Where(x => x.SkuId == skuId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new StockMovementListItemDto
            {
                Id = x.Id,
                CreatedAt = x.CreatedAt,
                Type = x.Type.ToString(),
                TypeValue = x.Type,
                SignedQuantity = x.SignedQuantity,
                UnitPrice = x.UnitPrice,
                UnitCost = x.UnitCost,
                CaseStockKind = x.CaseStockKind,
                Note = x.Note
            })
            .FirstOrDefaultAsync();

        return item is null ? null : ConvertCreatedAt(item);
    }

    public async Task<List<StockMovementListItemDto>> GetBySkuAsync(int skuId)
    {
        var items = await _db.StockMovements
            .AsNoTracking()
            .Where(x => x.SkuId == skuId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new StockMovementListItemDto
            {
                Id = x.Id,
                CreatedAt = x.CreatedAt,      // UTC en DB
                Type = x.Type.ToString(),
                TypeValue = x.Type,
                SignedQuantity = x.SignedQuantity,
                UnitPrice = x.UnitPrice,
                UnitCost = x.UnitCost,
                CaseStockKind = x.CaseStockKind,
                Note = x.Note
            })
            .ToListAsync();

        for (int i = 0; i < items.Count; i++)
            items[i] = ConvertCreatedAt(items[i]);

        return items;
    }

    public Task<List<int>> GetSkuIdsWithSalesBetweenAsync(DateTime fromUtc, DateTime toUtc)
    {
        return _db.StockMovements
            .AsNoTracking()
            .Where(x => x.CreatedAt >= fromUtc && x.CreatedAt < toUtc)
            .Where(x => x.Type == StockManager.Domain.Enums.StockMovementType.Sale)
            .Select(x => x.SkuId)
            .Distinct()
            .ToListAsync();
    }

    private static StockMovementListItemDto ConvertCreatedAt(StockMovementListItemDto item)
    {
        item.CreatedAt = BusinessTime.ConvertUtcToBusinessLocal(item.CreatedAt);
        return item;
    }
}
