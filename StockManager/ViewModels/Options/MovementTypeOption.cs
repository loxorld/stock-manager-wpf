using System;
using System.Collections.Generic;
using System.Text;
using StockManager.Domain.Enums;

namespace StockManager.ViewModels;

public sealed class MovementTypeOption
{
    public StockMovementType Value { get; }
    public string Display { get; }

    public MovementTypeOption(StockMovementType value, string display)
    {
        Value = value;
        Display = display;
    }

    public override string ToString() => Display;
}
