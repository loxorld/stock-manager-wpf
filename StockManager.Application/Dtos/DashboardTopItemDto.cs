using System;
using System.Collections.Generic;
using System.Text;

namespace StockManager.Application.Dtos;

public class DashboardTopItemDto
{
    public int SkuId { get; set; }
    public string Name { get; set; } = "";
    public int Units { get; set; }
    public decimal Revenue { get; set; }
}

