using Microsoft.EntityFrameworkCore;
using StockManager.Application.Dtos;
using StockManager.Application.Services;
using StockManager.Domain.Entities;
using StockManager.Domain.Enums;
using StockManager.Infrastructure.Persistence;

namespace StockManager.Infrastructure.Services;

public class StockMovementService : IStockMovementService
{
    private readonly StockDbContext _db;

    public StockMovementService(StockDbContext db)
    {
        _db = db;
    }

    public async Task RegisterAsync(RegisterMovementRequest request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        var sku = await _db.Skus.FirstOrDefaultAsync(x => x.Id == request.SkuId);
        if (sku == null)
            throw new InvalidOperationException("SKU inexistente.");

        // Nota normalizada
        var note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();

        // Validaciones por tipo (reglas del negocio)
        if (request.Type == StockMovementType.Shrinkage && note == null)
            throw new ArgumentException("La nota es obligatoria para una merma.");

        if (request.Type == StockMovementType.Adjustment && note == null)
            throw new ArgumentException("La nota es obligatoria para un ajuste.");

        // Calcular cantidad con signo (la verdad absoluta)
        int signedQty = request.Type switch
        {
            StockMovementType.PurchaseEntry => ValidatePositive(request.Quantity, "La cantidad debe ser mayor a 0."),
            StockMovementType.Sale => -ValidatePositive(request.Quantity, "La cantidad debe ser mayor a 0."),
            StockMovementType.Shrinkage => -ValidatePositive(request.Quantity, "La cantidad debe ser mayor a 0."),
            StockMovementType.Adjustment => ValidateNonZeroSigned(request.SignedQuantity, "El ajuste debe ser distinto de 0."),
            _ => throw new InvalidOperationException("Tipo de movimiento inválido.")
        };

        var newStock = sku.Stock + signedQty;
        if (newStock < 0)
            throw new InvalidOperationException("Stock insuficiente para realizar el movimiento.");

        using var tx = await _db.Database.BeginTransactionAsync();

        sku.Stock = newStock;

        _db.StockMovements.Add(new StockMovement
        {
            SkuId = sku.Id,
            Type = request.Type,
            SignedQuantity = signedQty,
            Note = note,
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        await tx.CommitAsync();
    }

    private static int ValidatePositive(int value, string message)
    {
        if (value <= 0) throw new ArgumentException(message);
        return value;
    }

    private static int ValidateNonZeroSigned(int? value, string message)
    {
        if (value is null || value.Value == 0) throw new ArgumentException(message);
        return value.Value;
    }
}
