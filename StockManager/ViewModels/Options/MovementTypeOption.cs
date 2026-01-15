using System;
using System.Collections.Generic;
using System.Text;
using StockManager.Domain.Enums;

namespace StockManager.ViewModels;

public sealed class MovementTypeOption
{
    public StockMovementType Value { get; }
    public string Text { get; }

    public MovementTypeOption(StockMovementType value, string text)
    {
        Value = value;
        Text = text;
    }

    public override string ToString() => Text; 
}
