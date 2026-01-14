using System;
using System.Collections.Generic;
using System.Text;

using StockManager.Domain.Enums;

namespace StockManager.Domain.Entities;

public class StockMovement
{
    public long Id { get; set; }

    public int SkuId { get; set; }
    public Sku? Sku { get; set; }

    public StockMovementType Type { get; set; }

    
    public int SignedQuantity { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Motivo o nota (ej: "rotura", "ajuste por conteo", "venta mostrador")
    public string? Note { get; set; }

    

}

