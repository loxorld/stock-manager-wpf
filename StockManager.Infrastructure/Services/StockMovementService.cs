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
        if (request.Type != StockMovementType.Adjustment)
        {
            // Para Sale / PurchaseEntry / Shrinkage: Quantity debe ser > 0
            if (request.Quantity <= 0)
                throw new ArgumentException("La cantidad debe ser mayor a 0.");
        }
        else
        {
            // Para Adjustment: se usa SignedQuantity
            if (!request.SignedQuantity.HasValue || request.SignedQuantity.Value == 0)
                throw new ArgumentException("En un ajuste, la cantidad firmada no puede ser 0.");
        }

        var sku = await _db.Skus.FirstOrDefaultAsync(x => x.Id == request.SkuId);
        if (sku == null)
            throw new InvalidOperationException("SKU inexistente.");

        // Calcular signedQty real a aplicar al stock
        int signedQty = request.Type switch
        {
            StockMovementType.PurchaseEntry => +request.Quantity,
            StockMovementType.Sale => -request.Quantity,
            StockMovementType.Shrinkage => -request.Quantity,
            StockMovementType.Adjustment => request.SignedQuantity!.Value,
            _ => throw new InvalidOperationException("Tipo de movimiento inválido.")
        };

        var newStock = sku.Stock + signedQty;
        if (newStock < 0)
            throw new InvalidOperationException("Stock insuficiente para realizar el movimiento.");

        // Captura de valores “históricos” 
        decimal? unitPrice = null;
        decimal? unitCost = null;

        switch (request.Type)
        {
            case StockMovementType.Sale:
                unitPrice = sku.Price; // precio vigente en el momento de vender
                unitCost = sku.Cost;   // costo vigente (sirve para margen)
                break;

            case StockMovementType.PurchaseEntry:
                // Compra: el costo tiene sentido; precio es opcional
                unitCost = sku.Cost;
                unitPrice = sku.Price; // opcional, puede servir para análisis
                break;

            case StockMovementType.Shrinkage:
            case StockMovementType.Adjustment:
                // No “genera ingresos”, pero el costo sirve si después quiero valuación
                unitCost = sku.Cost;
                break;
        }

        using var tx = await _db.Database.BeginTransactionAsync();

        sku.Stock = newStock;

        _db.StockMovements.Add(new StockMovement
        {
            SkuId = sku.Id,
            Type = request.Type,
            PaymentMethod = request.Type == StockMovementType.Sale
                ? request.PaymentMethod ?? PaymentMethod.Cash
                : null,
            SignedQuantity = signedQty,
            UnitPrice = unitPrice,
            UnitCost = unitCost,
            Note = request.Note,
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        await tx.CommitAsync();
    }
}
