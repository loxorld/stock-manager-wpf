using System;
using System.Collections.Generic;
using System.Text;

namespace StockManager.Application.Dtos;

public class UpsertPhoneModelRequest
{
    public int? Id { get; set; } // null = create
    public string Brand { get; set; } = "";
    public string ModelName { get; set; } = "";
}

