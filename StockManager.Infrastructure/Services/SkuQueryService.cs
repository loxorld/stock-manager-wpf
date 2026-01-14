using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using StockManager.Application.Dtos;
using StockManager.Application.Services;
using StockManager.Infrastructure.Persistence;

namespace StockManager.Infrastructure.Services;

public class SkuQueryService : ISkuQueryService
{
    private readonly StockDbContext _db;

    public SkuQueryService(StockDbContext db)
    {
        _db = db;
    }

    public async Task<List<SkuListItemDto>> GetAllAsync(string? search = null)
    {
        var q = _db.Skus
            .AsNoTracking()
            .Include(x => x.PhoneModel)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim().ToLower();

            q = q.Where(x =>
                x.Name.ToLower().Contains(search) ||
                (x.PhoneModel != null &&
                 (x.PhoneModel.Brand + " " + x.PhoneModel.ModelName).ToLower().Contains(search)));
        }

        var list = await q
            .OrderBy(x => x.Name)
            .Select(x => new SkuListItemDto
            {
                Id = x.Id,
                Name = x.Name,
                Category = x.Category.ToString(),
                PhoneModel = x.PhoneModel == null ? "" : (x.PhoneModel.Brand + " " + x.PhoneModel.ModelName),
                Stock = x.Stock,
                Price = x.Price
            })
            .ToListAsync();

        return list;
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

