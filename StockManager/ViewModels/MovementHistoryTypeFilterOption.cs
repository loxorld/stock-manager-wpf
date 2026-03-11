using StockManager.Domain.Enums;

namespace StockManager.ViewModels;

public sealed class MovementHistoryTypeFilterOption
{
    public StockMovementType? Value { get; }
    public string Display { get; }

    public MovementHistoryTypeFilterOption(StockMovementType? value, string display)
    {
        Value = value;
        Display = display;
    }

    public override string ToString() => Display;
}
