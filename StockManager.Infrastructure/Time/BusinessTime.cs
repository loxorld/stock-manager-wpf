using System;

namespace StockManager.Infrastructure.Time;

public static class BusinessTime
{
    private static readonly Lazy<TimeZoneInfo> BusinessTimeZone = new(ResolveTimeZone);

    public static TimeZoneInfo TimeZone => BusinessTimeZone.Value;

    public static DateTime GetBusinessNow()
        => ConvertUtcToBusinessLocal(DateTime.UtcNow);

    public static DateTime GetBusinessToday()
        => GetBusinessNow().Date;

    public static DateTime ConvertUtcToBusinessLocal(DateTime utcDateTime)
    {
        var normalizedUtc = utcDateTime.Kind == DateTimeKind.Utc
            ? utcDateTime
            : DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);

        return TimeZoneInfo.ConvertTimeFromUtc(normalizedUtc, TimeZone);
    }

    public static (DateTime fromUtc, DateTime toUtc) GetUtcRangeForBusinessDates(
        DateTime fromLocalDateInclusive,
        DateTime toLocalDateInclusive)
    {
        var fromLocal = DateTime.SpecifyKind(fromLocalDateInclusive.Date, DateTimeKind.Unspecified);
        var toLocalExclusive = DateTime.SpecifyKind(toLocalDateInclusive.Date.AddDays(1), DateTimeKind.Unspecified);

        return
        (
            ConvertBusinessLocalToUtc(fromLocal),
            ConvertBusinessLocalToUtc(toLocalExclusive)
        );
    }

    private static DateTime ConvertBusinessLocalToUtc(DateTime localDateTime)
    {
        var offset = TimeZone.GetUtcOffset(localDateTime);
        return new DateTimeOffset(localDateTime, offset).UtcDateTime;
    }

    private static TimeZoneInfo ResolveTimeZone()
    {
        var candidateIds = new[]
        {
            "Argentina Standard Time",
            "America/Argentina/Buenos_Aires"
        };

        foreach (var id in candidateIds)
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(id);
            }
            catch (TimeZoneNotFoundException)
            {
            }
            catch (InvalidTimeZoneException)
            {
            }
        }

        return TimeZoneInfo.Local;
    }
}
