using Microsoft.EntityFrameworkCore;
using StockManager.Application.Dtos;
using StockManager.Application.Services;
using StockManager.Infrastructure.Persistence;

namespace StockManager.Infrastructure.Services;

public class StockMovementQueryService : IStockMovementQueryService
{
    private readonly StockDbContext _db;

    public StockMovementQueryService(StockDbContext db)
    {
        _db = db;
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
                CreatedAt = x.CreatedAt,
                Type = x.Type.ToString(),
                SignedQuantity = x.SignedQuantity,
                Note = x.Note
            })
            .ToListAsync();

        // Convierto a local time afuera del query 
        foreach (var it in items)
            it.CreatedAt = it.CreatedAt.ToLocalTime();

        return items;
    }
}


