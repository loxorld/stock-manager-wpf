using System;
using System.Collections.Generic;
using System.Text;
using StockManager.Domain.Enums;

namespace StockManager.Application.Dtos;

public class SkuDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public ProductCategory Category { get; set; }
    public int? PhoneModelId { get; set; }
    public CaseType? CaseType { get; set; }
    public ProtectorType? ProtectorType { get; set; }
    public decimal Cost { get; set; }
    public decimal Price { get; set; }
    public bool Active { get; set; }
}
