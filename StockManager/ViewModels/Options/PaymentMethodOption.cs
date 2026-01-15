using System;
using System.Collections.Generic;
using System.Text;

using StockManager.Domain.Enums;

namespace StockManager.ViewModels;

public sealed class PaymentMethodOption
{
    public PaymentMethod Value { get; }
    public string Display { get; }

    public PaymentMethodOption(PaymentMethod value, string display)
    {
        Value = value;
        Display = display;
    }

    public override string ToString() => Display;
}
