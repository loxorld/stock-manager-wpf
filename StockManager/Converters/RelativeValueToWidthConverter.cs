using System.Globalization;
using System.Windows.Data;

namespace StockManager.Converters;

public sealed class RelativeValueToWidthConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 3)
            return 0d;

        if (!TryGetDouble(values[0], out var value) ||
            !TryGetDouble(values[1], out var max) ||
            !TryGetDouble(values[2], out var availableWidth))
        {
            return 0d;
        }

        if (value <= 0 || max <= 0 || availableWidth <= 0)
            return 0d;

        var ratio = Math.Min(1d, value / max);
        return Math.Max(6d, availableWidth * ratio);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();

    private static bool TryGetDouble(object value, out double result)
    {
        result = value switch
        {
            double doubleValue => doubleValue,
            float floatValue => floatValue,
            decimal decimalValue => (double)decimalValue,
            int intValue => intValue,
            long longValue => longValue,
            _ => 0d
        };

        return value is double or float or decimal or int or long;
    }
}
