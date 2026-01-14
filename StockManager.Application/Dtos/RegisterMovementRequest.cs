using System;
using System.Collections.Generic;
using System.Text;
using StockManager.Domain.Enums;

namespace StockManager.Application.Dtos;

public class RegisterMovementRequest
{
    public int SkuId { get; set; }
    public StockMovementType Type { get; set; }
    public int Quantity { get; set; } // positivo

    public int? SignedQuantity { get; set; } // solo para Adjustment
    public string? Note { get; set; }
}

