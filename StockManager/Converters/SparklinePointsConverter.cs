using StockManager.Application.Dtos;
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace StockManager.Converters;

public sealed class SparklinePointsConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 3 ||
            values[0] is not IEnumerable items ||
            !TryGetDouble(values[1], out var width) ||
            !TryGetDouble(values[2], out var height) ||
            width <= 0 ||
            height <= 0)
        {
            return new PointCollection();
        }

        var points = items.OfType<DashboardDailySalesDto>().ToList();
        if (points.Count == 0)
            return new PointCollection();

        var maxRevenue = points.Max(x => x.Revenue);
        if (maxRevenue <= 0)
            maxRevenue = 1;

        var padding = 8d;
        var usableWidth = Math.Max(1d, width - (padding * 2));
        var usableHeight = Math.Max(1d, height - (padding * 2));
        var step = points.Count == 1 ? 0 : usableWidth / (points.Count - 1);

        var linePoints = new List<Point>(points.Count);
        for (var index = 0; index < points.Count; index++)
        {
            var x = padding + (step * index);
            var ratio = (double)(points[index].Revenue / maxRevenue);
            var y = padding + ((1d - ratio) * usableHeight);
            linePoints.Add(new Point(x, y));
        }

        if (!string.Equals(parameter?.ToString(), "Area", StringComparison.OrdinalIgnoreCase))
            return new PointCollection(linePoints);

        var areaPoints = new List<Point>(linePoints.Count + 2)
        {
            new(linePoints[0].X, height - padding)
        };
        areaPoints.AddRange(linePoints);
        areaPoints.Add(new Point(linePoints[^1].X, height - padding));
        return new PointCollection(areaPoints);
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
            _ => 0d
        };

        return value is double or float or decimal or int;
    }
}
