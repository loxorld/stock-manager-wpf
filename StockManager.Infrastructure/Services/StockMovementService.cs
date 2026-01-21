using Microsoft.EntityFrameworkCore;
using StockManager.Application.Dtos;
using StockManager.Application.Services;
using StockManager.Domain.Entities;
using StockManager.Domain.Enums;
using StockManager.Infrastructure.Persistence;

namespace StockManager.Infrastructure.Services;

public class StockMovementService(StockDbContext db) : IStockMovementService
{
    private readonly StockDbContext _db = db;

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

        var isTransparentCase = sku.Category == ProductCategory.Case && sku.CaseType == CaseType.Transparent;

        if (sku.Category == ProductCategory.Case && !isTransparentCase && request.CaseStockKind is null)
            throw new InvalidOperationException("Para fundas, se debe indicar si es de mujer u hombre.");

        if (sku.Category != ProductCategory.Case && request.CaseStockKind is not null)
            throw new InvalidOperationException("Este SKU no es una funda.");

        if (isTransparentCase && request.CaseStockKind is not null)
            throw new InvalidOperationException("Las fundas transparentes no llevan género.");

        // Calcular signedQty real a aplicar al stock
        int signedQty = request.Type switch
        {
            StockMovementType.PurchaseEntry => +request.Quantity,
            StockMovementType.Sale => -request.Quantity,
            StockMovementType.Shrinkage => -request.Quantity,
            StockMovementType.Adjustment => request.SignedQuantity!.Value,
            _ => throw new InvalidOperationException("Tipo de movimiento inválido.")
        };

        if (sku.Category == ProductCategory.Case && !isTransparentCase)
        {
            var newCaseStock = request.CaseStockKind == CaseStockKind.Women
                ? sku.CaseStockWomen + signedQty
                : sku.CaseStockMen + signedQty;

            if (newCaseStock < 0)
                throw new InvalidOperationException("Stock insuficiente para realizar el movimiento.");

            if (request.CaseStockKind == CaseStockKind.Women)
                sku.CaseStockWomen = newCaseStock;
            else
                sku.CaseStockMen = newCaseStock;

            sku.Stock = sku.CaseStockWomen + sku.CaseStockMen;
        }
        else
        {
            

            var newStock = sku.Stock + signedQty;
            if (newStock < 0)
                throw new InvalidOperationException("Stock insuficiente para realizar el movimiento.");

            sku.Stock = newStock;
        }

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
