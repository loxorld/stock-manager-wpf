using Microsoft.EntityFrameworkCore;
using StockManager.Application.Dtos;
using StockManager.Application.Services;
using StockManager.Application.Text;
using StockManager.Domain.Enums;
using StockManager.Infrastructure.Persistence;
using System.Globalization;

namespace StockManager.Infrastructure.Services;

public class SkuQueryService(StockDbContext db) : ISkuQueryService
{
    private readonly StockDbContext _db = db;

    public async Task<List<SkuListItemDto>> GetAllAsync(
        string? searchText = null,
        ProductCategory? category = null,
        bool? active = null,
        int? stockMax = null)
    {
        var q = _db.Skus.AsNoTracking();

        if (category.HasValue)
            q = q.Where(x => x.Category == category.Value);

        if (active.HasValue)
            q = q.Where(x => x.Active == active.Value);

        if (stockMax.HasValue)
            q = q.Where(x => x.Stock <= stockMax.Value);

        var items = await q
            .OrderBy(x => x.Name)
            .Select(x => new SkuListItemDto
            {
                Id = x.Id,
                Name = x.Name,
                Category = x.Category.ToString(),
                CategoryValue = x.Category,
                CaseType = x.CaseType,
                ProtectorType = x.ProtectorType,
                Stock = x.Stock,
                CaseStockWomen = x.CaseStockWomen,
                CaseStockMen = x.CaseStockMen,
                Cost = x.Cost,
                Price = x.Price,
                Active = x.Active
            })
            .ToListAsync();

        if (string.IsNullOrWhiteSpace(searchText))
            return items;

        var normalizedSearch = SearchTextNormalizer.Normalize(searchText);
        var searchTerms = normalizedSearch
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return items
            .Where(x => MatchesSearch(x, normalizedSearch, searchTerms))
            .ToList();
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

    public async Task<List<SkuListItemDto>> GetCriticalStockAsync(int threshold, int take = 6)
    {
        var effectiveThreshold = threshold < 0 ? 0 : threshold;
        var effectiveTake = take <= 0 ? 6 : take;

        return await _db.Skus
            .AsNoTracking()
            .Where(x => x.Active)
            .Where(x => x.Stock <= effectiveThreshold)
            .OrderBy(x => x.Stock)
            .ThenBy(x => x.Name)
            .Take(effectiveTake)
            .Select(x => new SkuListItemDto
            {
                Id = x.Id,
                Name = x.Name,
                Category = x.Category.ToString(),
                CategoryValue = x.Category,
                CaseType = x.CaseType,
                ProtectorType = x.ProtectorType,
                Stock = x.Stock,
                CaseStockWomen = x.CaseStockWomen,
                CaseStockMen = x.CaseStockMen,
                Cost = x.Cost,
                Price = x.Price,
                Active = x.Active
            })
            .ToListAsync();
    }

    private static bool MatchesSearch(
        SkuListItemDto item,
        string normalizedSearch,
        string[] searchTerms)
    {
        if (normalizedSearch.Length == 0)
            return true;

        var searchable = SearchTextNormalizer.Normalize(BuildSearchText(item));
        if (searchable.Contains(normalizedSearch, StringComparison.Ordinal))
            return true;

        return searchTerms.All(term => searchable.Contains(term, StringComparison.Ordinal));
    }

    private static string BuildSearchText(SkuListItemDto item)
    {
        var fragments = new List<string>
        {
            item.Name,
            item.Category,
            item.Active ? "activo disponible" : "inactivo pausado",
            item.CategoryValue switch
            {
                ProductCategory.Case => "funda fundas",
                ProductCategory.ScreenProtector => "templado templados protector protectores vidrio",
                ProductCategory.Accessory => "accesorio accesorios",
                _ => string.Empty
            },
            DescribeCaseType(item.CaseType),
            DescribeProtectorType(item.ProtectorType),
            DescribeMoney(item.Price),
            DescribeMoney(item.Cost),
            item.Stock.ToString(CultureInfo.InvariantCulture),
            item.CaseStockWomen.ToString(CultureInfo.InvariantCulture),
            item.CaseStockMen.ToString(CultureInfo.InvariantCulture)
        };

        return string.Join(' ', fragments.Where(x => !string.IsNullOrWhiteSpace(x)));
    }

    private static string DescribeCaseType(CaseType? caseType) => caseType switch
    {
        CaseType.Transparent => "transparente transparente crystal clear",
        CaseType.Silicone => "silicone silicona silicon",
        CaseType.Design => "design diseno diseño estampada estampado",
        CaseType.Rugged => "rugged reforzada reforzado resistente armor",
        _ => string.Empty
    };

    private static string DescribeProtectorType(ProtectorType? protectorType) => protectorType switch
    {
        ProtectorType.Common => "comun común normal basico básico standard",
        ProtectorType.Reinforced => "reforzado reforzada reinforced premium",
        ProtectorType.Privacy => "privacy privado privacidad anti espia anti espía antiespia",
        _ => string.Empty
    };

    private static string DescribeMoney(decimal amount)
    {
        var invariant = amount.ToString("0.##", CultureInfo.InvariantCulture);
        var current = amount.ToString("0.##", CultureInfo.CurrentCulture);
        return $"{invariant} {current}";
    }
}
