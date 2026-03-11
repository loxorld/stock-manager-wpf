using StockManager.Domain.Enums;

namespace StockManager.Application.Dtos;

public class SkuListItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public ProductCategory CategoryValue { get; set; }
    public CaseType? CaseType { get; set; }
    public ProtectorType? ProtectorType { get; set; }

    public int Stock { get; set; }
    public int CaseStockWomen { get; set; }
    public int CaseStockMen { get; set; }
    public decimal Cost { get; set; }
    public decimal Price { get; set; }
    public bool Active { get; set; }

    public string StatusLabel => Active ? "Activo" : "Inactivo";

    public string VariantLabel => CategoryValue switch
    {
        ProductCategory.Case => CaseType switch
        {
            StockManager.Domain.Enums.CaseType.Transparent => "Funda transparente",
            StockManager.Domain.Enums.CaseType.Silicone => "Funda silicona",
            StockManager.Domain.Enums.CaseType.Design => "Funda diseno",
            StockManager.Domain.Enums.CaseType.Rugged => "Funda reforzada",
            _ => "Funda"
        },
        ProductCategory.ScreenProtector => ProtectorType switch
        {
            StockManager.Domain.Enums.ProtectorType.Common => "Templado comun",
            StockManager.Domain.Enums.ProtectorType.Reinforced => "Templado reforzado",
            StockManager.Domain.Enums.ProtectorType.Privacy => "Templado privacy",
            _ => "Templado"
        },
        ProductCategory.Accessory => "Accesorio",
        _ => Category
    };

    public string CategoryLabel => CategoryValue switch
    {
        ProductCategory.Case => "Funda",
        ProductCategory.ScreenProtector => "Templado",
        ProductCategory.Accessory => "Accesorio",
        _ => Category
    };
}

