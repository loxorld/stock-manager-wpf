using Microsoft.EntityFrameworkCore;
using StockManager.Application.Dtos;
using StockManager.Application.Services;
using StockManager.Infrastructure.Persistence;

namespace StockManager.Infrastructure.Services;

public class StockMovementQueryService(StockDbContext db) : IStockMovementQueryService
{
    private readonly StockDbContext _db = db;

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
                SignedQuantity = x.SignedQuantity,
                UnitPrice = x.UnitPrice,
                UnitCost = x.UnitCost,
                CaseStockKind = x.CaseStockKind,
                Note = x.Note
            })
            .ToListAsync();

        // Convertimos a hora local fuera del query 
        for (int i = 0; i < items.Count; i++)
            items[i].CreatedAt = items[i].CreatedAt.ToLocalTime();

        return items;
    }
}
