using System;
using System.Collections.Generic;
using System.Text;

namespace StockManager.Application.Dtos;

public class SkuListItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string PhoneModel { get; set; } = ""; // "Samsung A02" o ""
    public int Stock { get; set; }
    public decimal Price { get; set; }
}

