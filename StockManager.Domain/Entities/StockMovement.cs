using System;
using StockManager.Domain.Enums;

namespace StockManager.Domain.Entities;

public class StockMovement
{
    public long Id { get; set; }

    public int SkuId { get; set; }
    public Sku? Sku { get; set; }

    public StockMovementType Type { get; set; }

    public PaymentMethod? PaymentMethod { get; set; }

    // Cantidad firmada: + entra, - sale
    public int SignedQuantity { get; set; }

    // Precio/costo unitario al momento del movimiento 
    public decimal? UnitPrice { get; set; }
    public decimal? UnitCost { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Motivo o nota (ej: "rotura", "ajuste por conteo", "venta mostrador")
    public string? Note { get; set; }
}
