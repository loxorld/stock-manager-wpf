using Microsoft.EntityFrameworkCore;
using StockManager.Application.Dtos;
using StockManager.Application.Services;
using StockManager.Domain.Enums;
using StockManager.Infrastructure.Persistence;
using StockManager.Infrastructure.Time;

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

        var cashRevenue = await q
            .Where(m => m.PaymentMethod == PaymentMethod.Cash)
            .SumAsync(m =>
                (decimal?)(Math.Abs(m.SignedQuantity) * (m.UnitPrice ?? (m.Sku != null ? m.Sku.Price : 0m)))
            ) ?? 0m;

        var cardRevenue = await q
            .Where(m => m.PaymentMethod == PaymentMethod.MercadoPago)
            .SumAsync(m =>
                (decimal?)(Math.Abs(m.SignedQuantity) * (m.UnitPrice ?? (m.Sku != null ? m.Sku.Price : 0m)))
            ) ?? 0m;

        var estimatedMargin = await q.SumAsync(m =>
            (decimal?)(Math.Abs(m.SignedQuantity) *
                ((m.UnitPrice ?? (m.Sku != null ? m.Sku.Price : 0m))
                - (m.UnitCost ?? (m.Sku != null ? m.Sku.Cost : 0m))))
        ) ?? 0m;

        var salesCount = await q.CountAsync();
        var inventoryCost = await _db.Skus
            .AsNoTracking()
            .Where(s => s.Stock > 0)
            .SumAsync(s => (decimal?)(s.Cost * s.Stock)) ?? 0m;

        var productsWithoutMovement = await _db.Skus
            .AsNoTracking()
            .Where(s => s.Active)
            .CountAsync(s => !_db.StockMovements.Any(m =>
                m.SkuId == s.Id &&
                m.CreatedAt >= fromUtc &&
                m.CreatedAt < toUtc));

        return new DashboardSummaryDto
        {
            UnitsSold = unitsSold,
            Revenue = revenue,
            CashRevenue = cashRevenue,
            CardRevenue = cardRevenue,
            EstimatedMargin = estimatedMargin,
            InventoryCost = inventoryCost,
            ProductsWithoutMovement = productsWithoutMovement,
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
                Revenue = g.Sum(x => (x.UnitPrice ?? (x.Sku != null ? x.Sku.Price : 0m)) * Math.Abs(x.SignedQuantity)),
                Margin = g.Sum(x =>
                    ((x.UnitPrice ?? (x.Sku != null ? x.Sku.Price : 0m))
                    - (x.UnitCost ?? (x.Sku != null ? x.Sku.Cost : 0m)))
                    * Math.Abs(x.SignedQuantity))
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
                Revenue = g.Sum(x => (x.UnitPrice ?? (x.Sku != null ? x.Sku.Price : 0m)) * Math.Abs(x.SignedQuantity)),
                Margin = g.Sum(x =>
                    ((x.UnitPrice ?? (x.Sku != null ? x.Sku.Price : 0m))
                    - (x.UnitCost ?? (x.Sku != null ? x.Sku.Cost : 0m)))
                    * Math.Abs(x.SignedQuantity))
            })
            .OrderByDescending(x => x.Revenue)
            .ThenByDescending(x => x.Units)
            .Take(take)
            .ToListAsync();

        return data;
    }

    public async Task<List<DashboardTopItemDto>> GetTopByMarginAsync(DateTime fromUtc, DateTime toUtc, int take = 5)
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
                Revenue = g.Sum(x => (x.UnitPrice ?? (x.Sku != null ? x.Sku.Price : 0m)) * Math.Abs(x.SignedQuantity)),
                Margin = g.Sum(x =>
                    ((x.UnitPrice ?? (x.Sku != null ? x.Sku.Price : 0m))
                    - (x.UnitCost ?? (x.Sku != null ? x.Sku.Cost : 0m)))
                    * Math.Abs(x.SignedQuantity))
            })
            .OrderByDescending(x => x.Margin)
            .ThenByDescending(x => x.Revenue)
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
                Id = m.Id,
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
            it.CreatedAt = BusinessTime.ConvertUtcToBusinessLocal(it.CreatedAt);

        return items;
    }

    public async Task<List<DashboardDailySalesDto>> GetDailySalesAsync(DateTime fromUtc, DateTime toUtc)
    {
        var q = _db.StockMovements
            .AsNoTracking()
            .Where(m => m.CreatedAt >= fromUtc && m.CreatedAt < toUtc)
            .Where(m => m.Type == StockMovementType.Sale)
            .OrderBy(m => m.CreatedAt);

        var items = await q
            .Select(m => new
            {
                m.CreatedAt,
                m.SignedQuantity,
                UnitPrice = m.UnitPrice ?? (m.Sku != null ? m.Sku.Price : 0m)
            })
            .ToListAsync();

        return items
            .GroupBy(x => BusinessTime.ConvertUtcToBusinessLocal(x.CreatedAt).Date)
            .Select(g => new DashboardDailySalesDto
            {
                Date = g.Key,
                Units = g.Sum(x => Math.Abs(x.SignedQuantity)),
                Revenue = g.Sum(x => x.UnitPrice * Math.Abs(x.SignedQuantity))
            })
            .OrderBy(x => x.Date)
            .ToList();
    }
}
