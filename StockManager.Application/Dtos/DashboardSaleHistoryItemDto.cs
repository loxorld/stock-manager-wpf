using System;
using System.Collections.Generic;
using System.Text;

namespace StockManager.Application.Dtos;

public class DashboardSaleHistoryItemDto
{

    public long Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public string SkuName { get; set; } = "";

    // cantidad vendida (positiva)
    public int Quantity { get; set; }

    // precio unitario al momento del movimiento (histórico)
    public decimal UnitPrice { get; set; }

    // total = Quantity * UnitPrice
    public decimal Total { get; set; }

    public string? Note { get; set; }
}
