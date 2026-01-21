using StockManager.Domain.Enums;

namespace StockManager.Application.Dtos;

public class SkuListItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public ProductCategory CategoryValue { get; set; }
    public CaseType? CaseType { get; set; }

    public int Stock { get; set; }
    public int CaseStockWomen { get; set; }
    public int CaseStockMen { get; set; }
    public decimal Cost { get; set; }
    public decimal Price { get; set; }
}

