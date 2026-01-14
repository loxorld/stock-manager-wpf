using System;
using System.Collections.Generic;
using System.Text;

namespace StockManager.Application.Dtos;

public class StockMovementListItemDto
{
    public long Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Type { get; set; } = "";
    public int SignedQuantity { get; set; }
    public string? Note { get; set; }
}

