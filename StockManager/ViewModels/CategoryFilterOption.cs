using System;
using System.Collections.Generic;
using System.Text;
using StockManager.Domain.Enums;

namespace StockManager.ViewModels;

public sealed class CategoryFilterOption
{
    public ProductCategory? Value { get; }
    public string Display { get; }

    public CategoryFilterOption(ProductCategory? value, string display)
    {
        Value = value;
        Display = display;
    }

    public override string ToString() => Display; // por si las moscas
}

