using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StockManager.Application.Dtos;
using StockManager.Application.Services;
using StockManager.Views;
using System;
using System.Linq;
using System.Collections.ObjectModel;


namespace StockManager.ViewModels;

public enum DashboardPeriod
{
    Today = 0,
    Week = 1,
    Month = 2,
    Range = 3
}

public partial class DashboardViewModel : ObservableObject
{
    private readonly IDashboardQueryService _dashboard;

    [ObservableProperty] private bool isLoading;

    [ObservableProperty] private DashboardPeriod selectedPeriod = DashboardPeriod.Today;

    [ObservableProperty] private decimal revenue;
    [ObservableProperty] private decimal cashRevenue;
    [ObservableProperty] private decimal cardRevenue;
    [ObservableProperty] private int unitsSold;
    [ObservableProperty] private int salesCount;
    [ObservableProperty] private DateTime? fromDate;
    [ObservableProperty] private DateTime? toDate;
    [ObservableProperty] private decimal maxDailyRevenue = 1;
    [ObservableProperty] private bool hasDailySales;
    [ObservableProperty] private bool hasTopByUnits;
    [ObservableProperty] private bool hasTopByRevenue;

    [ObservableProperty] private string revenueDeltaText = "Sin cambios";
    [ObservableProperty] private string cashRevenueDeltaText = "Sin cambios";
    [ObservableProperty] private string cardRevenueDeltaText = "Sin cambios";
    [ObservableProperty] private string unitsSoldDeltaText = "Sin cambios";
    [ObservableProperty] private string salesCountDeltaText = "Sin cambios";


    public ObservableCollection<DashboardTopItemDto> TopByUnits { get; } = new();
    public ObservableCollection<DashboardTopItemDto> TopByRevenue { get; } = new();
    public ObservableCollection<DashboardDailySalesDto> DailySales { get; } = new();

    public ObservableCollection<DashboardSaleHistoryItemDto> SalesHistory { get; } = new();


    public DashboardViewModel(IDashboardQueryService dashboard)
    {
        _dashboard = dashboard;

        var today = DateTime.Today;
        FromDate = today;
        ToDate = today;
    }

    [RelayCommand]
    public async Task ApplyRangeAsync()
    {
        SelectedPeriod = DashboardPeriod.Range;
        await LoadAsync();
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            var (fromUtc, toUtc) = SelectedPeriod == DashboardPeriod.Range
                ? GetCustomRangeUtc()
                : GetRangeUtc(SelectedPeriod);

            var (prevFromUtc, prevToUtc) = GetPreviousRangeUtc(fromUtc, toUtc);

            var summary = await _dashboard.GetSummaryAsync(fromUtc, toUtc);
            Revenue = summary.Revenue;
            CashRevenue = summary.CashRevenue;
            CardRevenue = summary.CardRevenue;
            UnitsSold = summary.UnitsSold;
            SalesCount = summary.SalesCount;

            var previousSummary = await _dashboard.GetSummaryAsync(prevFromUtc, prevToUtc);
            RevenueDeltaText = BuildDeltaText(Revenue, previousSummary.Revenue);
            CashRevenueDeltaText = BuildDeltaText(CashRevenue, previousSummary.CashRevenue);
            CardRevenueDeltaText = BuildDeltaText(CardRevenue, previousSummary.CardRevenue);
            UnitsSoldDeltaText = BuildDeltaText(UnitsSold, previousSummary.UnitsSold);
            SalesCountDeltaText = BuildDeltaText(SalesCount, previousSummary.SalesCount);

            var history = await _dashboard.GetSalesHistoryAsync(fromUtc, toUtc);
            SalesHistory.Clear();
            foreach (var it in history) SalesHistory.Add(it);

            var dailySales = await _dashboard.GetDailySalesAsync(fromUtc, toUtc);
            DailySales.Clear();
            foreach (var it in dailySales) DailySales.Add(it);
            MaxDailyRevenue = DailySales.Count == 0 ? 1 : DailySales.Max(x => x.Revenue);



            TopByUnits.Clear();
            TopByRevenue.Clear();
            var topByUnits = await _dashboard.GetTopByUnitsAsync(fromUtc, toUtc);
            foreach (var it in topByUnits) TopByUnits.Add(it);
            HasTopByUnits = TopByUnits.Count > 0;

            var topByRevenue = await _dashboard.GetTopByRevenueAsync(fromUtc, toUtc);
            foreach (var it in topByRevenue) TopByRevenue.Add(it);
            HasTopByRevenue = TopByRevenue.Count > 0;
        }
        catch (Exception ex)
        {
            UiError.Show(ex, "Error al actualizar el dashboard");
        }
        finally
        {
            IsLoading = false;
        }
    }


    private (DateTime fromUtc, DateTime toUtc) GetCustomRangeUtc()
    {
        if (FromDate is null || ToDate is null)
            throw new ArgumentException("Seleccioná Desde y Hasta.");

        var fromLocalDate = FromDate.Value.Date;
        var toLocalDate = ToDate.Value.Date;

        if (toLocalDate < fromLocalDate)
            throw new ArgumentException("La fecha 'Hasta' no puede ser menor que 'Desde'.");

        // día completo de 'Hasta' => [Desde 00:00, Hasta+1 00:00)
        var toLocalExclusiveDate = toLocalDate.AddDays(1);

        // Argentina timezone (Windows)
        var tz = TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time");

        //  a UTC usando DateTimeOffset (evita el problema Kind=Unspecified)
        var fromOffset = new DateTimeOffset(
            fromLocalDate,
            tz.GetUtcOffset(fromLocalDate)
        );

        var toOffset = new DateTimeOffset(
            toLocalExclusiveDate,
            tz.GetUtcOffset(toLocalExclusiveDate)
        );

        return (fromOffset.UtcDateTime, toOffset.UtcDateTime);
    }



    partial void OnSelectedPeriodChanged(DashboardPeriod value)
    {
        if (value != DashboardPeriod.Range)
            _ = LoadAsync();
    }

    private static (DateTime fromUtc, DateTime toUtc) GetRangeUtc(DashboardPeriod period)
    {
        // Argentina timezone
        var tz = TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time");

        var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);

        DateTime startLocal = period switch
        {
            DashboardPeriod.Today => nowLocal.Date,
            DashboardPeriod.Week => nowLocal.Date.AddDays(-(int)nowLocal.DayOfWeek), // Semana desde domingo
            DashboardPeriod.Month => new DateTime(nowLocal.Year, nowLocal.Month, 1),
            _ => nowLocal.Date
        };

        var endLocal = period switch
        {
            DashboardPeriod.Today => startLocal.AddDays(1),
            DashboardPeriod.Week => startLocal.AddDays(7),
            DashboardPeriod.Month => startLocal.AddMonths(1),
            _ => startLocal.AddDays(1)
        };

        var fromUtc = TimeZoneInfo.ConvertTimeToUtc(startLocal, tz);
        var toUtc = TimeZoneInfo.ConvertTimeToUtc(endLocal, tz);
        return (fromUtc, toUtc);
    }

    private static (DateTime fromUtc, DateTime toUtc) GetPreviousRangeUtc(DateTime fromUtc, DateTime toUtc)
    {
        var span = toUtc - fromUtc;
        return (fromUtc - span, fromUtc);
    }

    private static string BuildDeltaText(decimal current, decimal previous)
        => BuildDeltaText((double)current, (double)previous);

    private static string BuildDeltaText(int current, int previous)
        => BuildDeltaText(current, (double)previous);

    private static string BuildDeltaText(double current, double previous)
    {
        if (Math.Abs(previous) < 0.0001)
            return current == 0 ? "Sin cambios" : "Nuevo período";

        var delta = current - previous;
        var percent = delta / previous;
        var arrow = percent >= 0 ? "▲" : "▼";
        return $"{arrow} {Math.Abs(percent):P0} vs período anterior";
    }
}

