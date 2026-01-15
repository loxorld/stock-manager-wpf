using Microsoft.EntityFrameworkCore;
using StockManager.Application.Dtos;
using StockManager.Application.Services;
using StockManager.Domain.Entities;
using StockManager.Infrastructure.Persistence;

namespace StockManager.Infrastructure.Services;

public class PhoneModelCommandService : IPhoneModelCommandService
{
    private readonly StockDbContext _db;

    public PhoneModelCommandService(StockDbContext db)
    {
        _db = db;
    }

    public async Task<int> CreateAsync(UpsertPhoneModelRequest r)
    {
        Validate(r);

        var brand = r.Brand.Trim();
        var model = r.ModelName.Trim();

        // ✅ Si existe (activo o inactivo), no duplicamos (por el índice único).
        // Si está inactivo, lo reactivamos.
        var existing = await _db.PhoneModels
            .FirstOrDefaultAsync(x => x.Brand == brand && x.ModelName == model);

        if (existing != null)
        {
            if (existing.Active)
                throw new InvalidOperationException("Ese modelo ya existe.");

            existing.Active = true;
            await _db.SaveChangesAsync();
            return existing.Id;
        }

        var entity = new PhoneModel
        {
            Brand = brand,
            ModelName = model,
            Active = true
        };

        _db.PhoneModels.Add(entity);
        await _db.SaveChangesAsync();
        return entity.Id;
    }

    public async Task UpdateAsync(UpsertPhoneModelRequest r)
    {
        if (r.Id is null) throw new ArgumentException("Id requerido para actualizar.");
        Validate(r);

        var entity = await _db.PhoneModels.FirstOrDefaultAsync(x => x.Id == r.Id.Value);
        if (entity == null) throw new InvalidOperationException("Modelo inexistente.");

        var brand = r.Brand.Trim();
        var model = r.ModelName.Trim();

        // ✅ No permitir duplicado con otro registro (aunque esté inactivo),
        
        var exists = await _db.PhoneModels
            .AnyAsync(x => x.Id != entity.Id && x.Brand == brand && x.ModelName == model);

        if (exists)
            throw new InvalidOperationException("Ese modelo ya existe.");

        entity.Brand = brand;
        entity.ModelName = model;

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _db.PhoneModels.FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null)
            throw new InvalidOperationException("Modelo inexistente.");

        // ✅ Eliminación lógica: aunque tenga SKUs asociados, no rompe nada.
        if (!entity.Active)
            return; // idempotente

        entity.Active = false;
        await _db.SaveChangesAsync();
    }

    private static void Validate(UpsertPhoneModelRequest r)
    {
        if (string.IsNullOrWhiteSpace(r.Brand))
            throw new ArgumentException("La marca es obligatoria.");

        if (string.IsNullOrWhiteSpace(r.ModelName))
            throw new ArgumentException("El nombre del modelo es obligatorio.");
    }
}
