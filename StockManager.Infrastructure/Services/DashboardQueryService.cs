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
        var q = _db.StockMovements.AsNoTracking()
            .Where(m => m.CreatedAt >= fromUtc && m.CreatedAt < toUtc)
            .Where(m => m.Type == StockMovementType.Sale);

        // SignedQuantity en venta es negativo. Para unidades/revenue usamos ABS.
        var unitsSold = await q.SumAsync(m => (int?)Math.Abs(m.SignedQuantity)) ?? 0;

        var revenue = await q.SumAsync(m =>
            (decimal?)(Math.Abs(m.SignedQuantity) * (m.UnitPrice ?? (m.Sku != null ? m.Sku.Price : 0m)))
        ) ?? 0m;

        var salesCount = await q.CountAsync();

        return new DashboardSummaryDto
        {
            UnitsSold = unitsSold,
            Revenue = revenue,
            SalesCount = salesCount
        };
    }

    public async Task<List<DashboardTopItemDto>> GetTopByUnitsAsync(DateTime fromUtc, DateTime toUtc, int take = 5)
    {
        var data = await _db.StockMovements.AsNoTracking()
            .Where(m => m.CreatedAt >= fromUtc && m.CreatedAt < toUtc)
            .Where(m => m.Type == StockMovementType.Sale)
            .GroupBy(m => new { m.SkuId, Name = m.Sku!.Name })
            .Select(g => new DashboardTopItemDto
            {
                SkuId = g.Key.SkuId,
                Name = g.Key.Name,
                Units = g.Sum(x => Math.Abs(x.SignedQuantity)),
                Revenue = g.Sum(x => (x.UnitPrice ?? (x.Sku != null ? x.Sku.Price : 0m)) * Math.Abs(x.SignedQuantity))
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
            .GroupBy(m => new { m.SkuId, Name = m.Sku!.Name })
            .Select(g => new DashboardTopItemDto
            {
                SkuId = g.Key.SkuId,
                Name = g.Key.Name,
                Units = g.Sum(x => Math.Abs(x.SignedQuantity)),
                Revenue = g.Sum(x => (x.UnitPrice ?? (x.Sku != null ? x.Sku.Price : 0m)) * Math.Abs(x.SignedQuantity))
            })
            .OrderByDescending(x => x.Revenue)
            .ThenByDescending(x => x.Units)
            .Take(take)
            .ToListAsync();

        return data;
    }

    public async Task<List<DashboardSaleHistoryItemDto>> GetSalesHistoryAsync(DateTime fromUtc, DateTime toUtc)
    {
        var q = _db.StockMovements
            .AsNoTracking()
            .Where(m => m.CreatedAt >= fromUtc && m.CreatedAt < toUtc)
            .Where(m => m.Type == StockMovementType.Sale)
            .OrderByDescending(m => m.CreatedAt);

        

        var items = await q
            .Select(m => new DashboardSaleHistoryItemDto
            {
                CreatedAt = m.CreatedAt,
                SkuName = m.Sku != null ? m.Sku.Name : ("SKU #" + m.SkuId),

                // venta => SignedQuantity es negativo
                Quantity = -m.SignedQuantity,

                // precio histórico si existe, sino fallback al precio actual
                UnitPrice = m.UnitPrice ?? (m.Sku != null ? m.Sku.Price : 0m),

                Total = (-m.SignedQuantity) * (m.UnitPrice ?? (m.Sku != null ? m.Sku.Price : 0m)),
                Note = m.Note
            })
            .ToListAsync();

        foreach (var it in items)
            it.CreatedAt = it.CreatedAt.ToLocalTime();

        return items;
    }
}
