using System;
using System.Collections.Generic;
using System.Text;
using StockManager.Domain.Enums;

namespace StockManager.ViewModels;

public sealed class CaseStockKindOption
{
    public CaseStockKind Value { get; }
    public string Display { get; }

    public CaseStockKindOption(CaseStockKind value, string display)
    {
        Value = value;
        Display = display;
    }

    public override string ToString() => Display;
}