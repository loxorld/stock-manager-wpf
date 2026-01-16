using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace StockManager.Converters;

public class StockLevelToBrushConverter : IValueConverter
{
    private static readonly Brush CriticalBackground = new SolidColorBrush(Color.FromRgb(0xFF, 0xE0, 0xE0));
    private static readonly Brush WarningBackground = new SolidColorBrush(Color.FromRgb(0xFF, 0xF4, 0xC8));
    private static readonly Brush OkBackground = new SolidColorBrush(Color.FromRgb(0xE6, 0xF4, 0xEA));

    private static readonly Brush CriticalForeground = new SolidColorBrush(Color.FromRgb(0xB7, 0x1C, 0x1C));
    private static readonly Brush WarningForeground = new SolidColorBrush(Color.FromRgb(0x8D, 0x6E, 0x63));
    private static readonly Brush OkForeground = new SolidColorBrush(Color.FromRgb(0x1B, 0x5E, 0x20));

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not int stock)
            return Brushes.Transparent;

        var isForeground = string.Equals(parameter?.ToString(), "Foreground", StringComparison.OrdinalIgnoreCase);
        return isForeground ? ResolveForeground(stock) : ResolveBackground(stock);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();

    private static Brush ResolveBackground(int stock)
    {
        if (stock <= 2) return CriticalBackground;
        if (stock <= 5) return WarningBackground;
        return OkBackground;
    }

    private static Brush ResolveForeground(int stock)
    {
        if (stock <= 2) return CriticalForeground;
        if (stock <= 5) return WarningForeground;
        return OkForeground;
    }
}