using System;
using System.Collections.Generic;
using System.Text;
using System;
using System.Globalization;
using System.Windows.Data;

namespace StockManager.Converters;

public class BooleanToOpacityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var b = value is bool bb && bb;
        return b ? 1.0 : 0.35;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

