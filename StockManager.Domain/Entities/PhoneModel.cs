using System;
using System.Collections.Generic;
using System.Text;

namespace StockManager.Domain.Entities;

public class PhoneModel
{
    public int Id { get; set; }
    public string Brand { get; set; } = "";
    public string ModelName { get; set; } = "";

    public bool Active { get; set; } = true;
}

