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

    // ✅ Para combos (SKU editor): SOLO activos
    public async Task<List<PhoneModelDto>> GetAllAsync()
    {
        return await _db.PhoneModels
            .AsNoTracking()
            .Where(x => x.Active)
            .OrderBy(x => x.Brand).ThenBy(x => x.ModelName)
            .Select(x => new PhoneModelDto
            {
                Id = x.Id,
                Display = x.Brand + " " + x.ModelName
            })
            .ToListAsync();
    }

    // ✅ Para admin : por UX también mostramos SOLO activos
    // (así cuando "eliminás" desaparece de la lista).
    public async Task<List<PhoneModelListItemDto>> GetAllForAdminAsync(string? search = null)
    {
        var q = _db.PhoneModels
            .AsNoTracking()
            .Where(x => x.Active);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();

            // ✅ Case-insensitive (evita el problema de "a" vs "A")
            q = q.Where(x =>
                x.Brand.ToLower().Contains(s) ||
                x.ModelName.ToLower().Contains(s));
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
