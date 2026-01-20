using System;
using StockManager.Domain.Enums;

namespace StockManager.Application.Dtos;

public class StockMovementListItemDto
{
    public long Id { get; set; }

    // Fecha local (ya convertida en el query)
    public DateTime CreatedAt { get; set; }

    // Tipo legible (Sale, PurchaseEntry, etc.)
    public string Type { get; set; } = "";

    // Cantidad con signo (+ entra, - sale)
    public int SignedQuantity { get; set; }

    // Precio/costo unitario al momento del movimiento (pueden ser null)
    public decimal? UnitPrice { get; set; }
    public decimal? UnitCost { get; set; }
    public CaseStockKind? CaseStockKind { get; set; }
    public string? Note { get; set; }

    // Helpers útiles 
    public int AbsQuantity => Math.Abs(SignedQuantity);

    public decimal? TotalAmount =>
        UnitPrice.HasValue
            ? UnitPrice.Value * Math.Abs(SignedQuantity)
            : null;

    public string? CaseStockKindLabel => CaseStockKind switch
    {
        StockManager.Domain.Enums.CaseStockKind.Women => "Mujer",
        StockManager.Domain.Enums.CaseStockKind.Men => "Hombre",
        _ => null
    };
}




