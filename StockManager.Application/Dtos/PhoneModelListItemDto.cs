using System;
using System.Collections.Generic;
using System.Text;

namespace StockManager.Application.Dtos;

public class PhoneModelListItemDto
{
    public int Id { get; set; }
    public string Brand { get; set; } = "";
    public string ModelName { get; set; } = "";
    public string Display => $"{Brand} {ModelName}";
}

