using Microsoft.EntityFrameworkCore;
using StockManager.Application.Dtos;
using StockManager.Application.Services;
using StockManager.Domain.Enums;
using StockManager.Infrastructure.Persistence;

namespace StockManager.Infrastructure.Services;

public class DashboardQueryService : IDashboardQueryService
{
    private readonly StockDbContext _db;

    public DashboardQueryService(StockDbContext db)
    {
        _db = db;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(DateTime fromUtc, DateTime toUtc)
    {
        // Ventas reales: Type = Sale, UnitPrice no null
        var q = _db.StockMovements.AsNoTracking()
            .Where(m => m.CreatedAt >= fromUtc && m.CreatedAt < toUtc)
            .Where(m => m.Type == StockMovementType.Sale)
            .Where(m => m.UnitPrice != null);

        var sales = await q.Select(m => new
        {
            Qty = m.SignedQuantity,      // negativo
            Price = m.UnitPrice!.Value
        }).ToListAsync();

        var units = sales.Sum(x => Math.Abs(x.Qty));
        var revenue = sales.Sum(x => x.Price * Math.Abs(x.Qty));
        var count = sales.Count;

        return new DashboardSummaryDto
        {
            UnitsSold = units,
            Revenue = revenue,
            SalesCount = count
        };
    }

    public async Task<List<DashboardTopItemDto>> GetTopByUnitsAsync(DateTime fromUtc, DateTime toUtc, int take = 5)
    {
        var data = await _db.StockMovements.AsNoTracking()
            .Where(m => m.CreatedAt >= fromUtc && m.CreatedAt < toUtc)
            .Where(m => m.Type == StockMovementType.Sale)
            .Where(m => m.UnitPrice != null)
            .GroupBy(m => new { m.SkuId, m.Sku!.Name })
            .Select(g => new DashboardTopItemDto
            {
                SkuId = g.Key.SkuId,
                Name = g.Key.Name,
                Units = g.Sum(x => Math.Abs(x.SignedQuantity)),
                Revenue = g.Sum(x => x.UnitPrice!.Value * Math.Abs(x.SignedQuantity))
            })
            .OrderByDescending(x => x.Units)
            .ThenByDescending(x => x.Revenue)
            .Take(take)
            .ToListAsync();

        return data;
    }

    public async Task<List<DashboardTopItemDto>> GetTopByRevenueAsync(DateTime fromUtc, DateTime toUtc, int take = 5)
    {
        var data = await _db.StockMovements.AsNoTracking()
            .Where(m => m.CreatedAt >= fromUtc && m.CreatedAt < toUtc)
            .Where(m => m.Type == StockMovementType.Sale)
            .Where(m => m.UnitPrice != null)
            .GroupBy(m => new { m.SkuId, m.Sku!.Name })
            .Select(g => new DashboardTopItemDto
            {
                SkuId = g.Key.SkuId,
                Name = g.Key.Name,
                Units = g.Sum(x => Math.Abs(x.SignedQuantity)),
                Revenue = g.Sum(x => x.UnitPrice!.Value * Math.Abs(x.SignedQuantity))
            })
            .OrderByDescending(x => x.Revenue)
            .ThenByDescending(x => x.Units)
            .Take(take)
            .ToListAsync();

        return data;
    }
}
