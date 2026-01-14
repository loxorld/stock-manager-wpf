using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using StockManager.Application.Dtos;
using StockManager.Application.Services;
using StockManager.Infrastructure.Persistence;

namespace StockManager.Infrastructure.Services;

public class PhoneModelQueryService : IPhoneModelQueryService
{
    private readonly StockDbContext _db;

    public PhoneModelQueryService(StockDbContext db)
    {
        _db = db;
    }

    public async Task<List<PhoneModelDto>> GetAllAsync()
    {
        return await _db.PhoneModels
            .AsNoTracking()
            .OrderBy(x => x.Brand).ThenBy(x => x.ModelName)
            .Select(x => new PhoneModelDto
            {
                Id = x.Id,
                Display = x.Brand + " " + x.ModelName
            })
            .ToListAsync();
    }

    public async Task<List<PhoneModelListItemDto>> GetAllForAdminAsync(string? search = null)
    {
        var q = _db.PhoneModels.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(x => x.Brand.Contains(s) || x.ModelName.Contains(s));
        }

        return await q
            .OrderBy(x => x.Brand).ThenBy(x => x.ModelName)
            .Select(x => new PhoneModelListItemDto
            {
                Id = x.Id,
                Brand = x.Brand,
                ModelName = x.ModelName
            })
            .ToListAsync();
    }
}
