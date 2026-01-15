using System;
using System.Collections.Generic;
using System.Text;
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

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var s = searchText.Trim().ToLower();

            q = q.Where(x =>
                x.Name.ToLower().Contains(s)
                || (x.PhoneModel != null && (x.PhoneModel.Brand + " " + x.PhoneModel.ModelName).ToLower().Contains(s))
            );

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
                PhoneModel = x.PhoneModelId == null ? "" : (x.PhoneModel!.Brand + " " + x.PhoneModel!.ModelName),
                Stock = x.Stock,
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
                PhoneModelId = x.PhoneModelId,
                CaseType = x.CaseType,
                ProtectorType = x.ProtectorType,
                Cost = x.Cost,
                Price = x.Price,
                Active = x.Active
            })
            .FirstOrDefaultAsync();
    }
}


