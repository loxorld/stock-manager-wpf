using System;
using System.Collections.Generic;
using System.Text;
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

        // Evitar duplicados (Brand + ModelName)
        var exists = await _db.PhoneModels
            .AnyAsync(x => x.Brand == r.Brand.Trim() && x.ModelName == r.ModelName.Trim());

        if (exists)
            throw new InvalidOperationException("Ese modelo ya existe.");

        var entity = new PhoneModel
        {
            Brand = r.Brand.Trim(),
            ModelName = r.ModelName.Trim()
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

        var exists = await _db.PhoneModels
            .AnyAsync(x => x.Id != entity.Id && x.Brand == brand && x.ModelName == model);

        if (exists)
            throw new InvalidOperationException("Ese modelo ya existe.");

        entity.Brand = brand;
        entity.ModelName = model;

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

