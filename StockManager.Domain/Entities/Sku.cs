using System;
using System.Collections.Generic;
using System.Text;
using StockManager.Domain.Enums;

namespace StockManager.Domain.Entities;

public class Sku
{
    public int Id { get; set; }

    public ProductCategory Category { get; set; }

    // Nombre visible tipo: "Funda silicona A02" / "Templado Privacy A20" / "Cargador 20W"
    public string Name { get; set; } = "";

    // Para fundas y templados
    public int? PhoneModelId { get; set; }
    public PhoneModel? PhoneModel { get; set; }

    // Solo si Category = Case
    public CaseType? CaseType { get; set; }

    // Solo si Category = ScreenProtector
    public ProtectorType? ProtectorType { get; set; }

    // Stock y precios
    public int Stock { get; set; }
    public decimal Cost { get; set; }
    public decimal Price { get; set; }

    public bool Active { get; set; } = true;
}