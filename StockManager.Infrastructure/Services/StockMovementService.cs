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
            if (request.Quantity <= 0)
                throw new ArgumentException("La cantidad debe ser mayor a 0.");
        }
        else if (!request.SignedQuantity.HasValue || request.SignedQuantity.Value == 0)
        {
            throw new ArgumentException("En un ajuste, la cantidad firmada no puede ser 0.");
        }

        var sku = await _db.Skus
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.SkuId);

        if (sku == null)
            throw new InvalidOperationException("SKU inexistente.");

        var isTransparentCase = sku.Category == ProductCategory.Case && sku.CaseType == CaseType.Transparent;

        if (sku.Category == ProductCategory.Case && !isTransparentCase && request.CaseStockKind is null)
            throw new InvalidOperationException("Para fundas, se debe indicar si es de mujer u hombre.");

        if (sku.Category != ProductCategory.Case && request.CaseStockKind is not null)
            throw new InvalidOperationException("Este SKU no es una funda.");

        if (isTransparentCase && request.CaseStockKind is not null)
            throw new InvalidOperationException("Las fundas transparentes no llevan género.");

        var signedQty = request.Type switch
        {
            StockMovementType.PurchaseEntry => +request.Quantity,
            StockMovementType.Sale => -request.Quantity,
            StockMovementType.Shrinkage => -request.Quantity,
            StockMovementType.Adjustment => request.SignedQuantity!.Value,
            _ => throw new InvalidOperationException("Tipo de movimiento inválido.")
        };

        decimal? unitPrice = null;
        decimal? unitCost = null;

        switch (request.Type)
        {
            case StockMovementType.Sale:
                unitPrice = sku.Price;
                unitCost = sku.Cost;
                break;

            case StockMovementType.PurchaseEntry:
                unitCost = sku.Cost;
                unitPrice = sku.Price;
                break;

            case StockMovementType.Shrinkage:
            case StockMovementType.Adjustment:
                unitCost = sku.Cost;
                break;
        }

        using var tx = await _db.Database.BeginTransactionAsync();

        await ApplyStockChangeOrThrowAsync(
            sku,
            signedQty,
            request.CaseStockKind,
            "Stock insuficiente para realizar el movimiento.");

        _db.StockMovements.Add(new StockMovement
        {
            SkuId = sku.Id,
            Type = request.Type,
            PaymentMethod = request.Type == StockMovementType.Sale
                ? request.PaymentMethod ?? PaymentMethod.Cash
                : null,
            CaseStockKind = request.CaseStockKind,
            SignedQuantity = signedQty,
            UnitPrice = unitPrice,
            UnitCost = unitCost,
            Note = request.Note,
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        await tx.CommitAsync();
    }

    public async Task DeleteSaleAsync(long movementId)
    {
        var movement = await _db.StockMovements
            .AsNoTracking()
            .Include(m => m.Sku)
            .FirstOrDefaultAsync(m => m.Id == movementId);

        if (movement == null)
            throw new InvalidOperationException("La venta no existe.");

        if (movement.Type != StockMovementType.Sale)
            throw new InvalidOperationException("Solo se pueden eliminar ventas.");

        var sku = movement.Sku
            ?? await _db.Skus.AsNoTracking().FirstOrDefaultAsync(x => x.Id == movement.SkuId);

        if (sku == null)
            throw new InvalidOperationException("SKU inexistente.");

        var isTransparentCase = sku.Category == ProductCategory.Case && sku.CaseType == CaseType.Transparent;
        if (sku.Category == ProductCategory.Case && !isTransparentCase && movement.CaseStockKind is null)
            throw new InvalidOperationException("La venta no tiene género registrado.");

        using var tx = await _db.Database.BeginTransactionAsync();

        await ApplyStockChangeOrThrowAsync(
            sku,
            -movement.SignedQuantity,
            movement.CaseStockKind,
            "El stock resultante no puede ser negativo.");

        var deletedRows = await _db.StockMovements
            .Where(m => m.Id == movementId && m.Type == StockMovementType.Sale)
            .ExecuteDeleteAsync();

        if (deletedRows == 0)
            throw new InvalidOperationException("La venta ya no existe.");

        await tx.CommitAsync();
    }

    private async Task ApplyStockChangeOrThrowAsync(
        Sku sku,
        int signedQty,
        CaseStockKind? caseStockKind,
        string insufficientStockMessage)
    {
        var affectedRows = await ApplyStockChangeAsync(sku, signedQty, caseStockKind);
        if (affectedRows > 0)
            return;

        var skuExists = await _db.Skus.AnyAsync(x => x.Id == sku.Id);
        if (!skuExists)
            throw new InvalidOperationException("SKU inexistente.");

        throw new InvalidOperationException(insufficientStockMessage);
    }

    private Task<int> ApplyStockChangeAsync(Sku sku, int signedQty, CaseStockKind? caseStockKind)
    {
        var query = _db.Skus.Where(x => x.Id == sku.Id);
        var isTransparentCase = sku.Category == ProductCategory.Case && sku.CaseType == CaseType.Transparent;

        if (sku.Category == ProductCategory.Case && !isTransparentCase)
        {
            if (caseStockKind == CaseStockKind.Women)
            {
                return query
                    .Where(x => x.CaseStockWomen + signedQty >= 0)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(x => x.CaseStockWomen, x => x.CaseStockWomen + signedQty)
                        .SetProperty(x => x.Stock, x => x.Stock + signedQty));
            }

            return query
                .Where(x => x.CaseStockMen + signedQty >= 0)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.CaseStockMen, x => x.CaseStockMen + signedQty)
                    .SetProperty(x => x.Stock, x => x.Stock + signedQty));
        }

        return query
            .Where(x => x.Stock + signedQty >= 0)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Stock, x => x.Stock + signedQty));
    }
}
