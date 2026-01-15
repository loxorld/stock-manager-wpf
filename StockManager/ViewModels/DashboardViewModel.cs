using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StockManager.Application.Dtos;
using StockManager.Application.Services;
using StockManager.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

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
    [ObservableProperty] private int unitsSold;
    [ObservableProperty] private int salesCount;
    [ObservableProperty] private DateTime? fromDate;
    [ObservableProperty] private DateTime? toDate;


    public ObservableCollection<DashboardTopItemDto> TopByUnits { get; } = new();
    public ObservableCollection<DashboardTopItemDto> TopByRevenue { get; } = new();

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


            var summary = await _dashboard.GetSummaryAsync(fromUtc, toUtc);
            Revenue = summary.Revenue;
            UnitsSold = summary.UnitsSold;
            SalesCount = summary.SalesCount;

            var topUnits = await _dashboard.GetTopByUnitsAsync(fromUtc, toUtc, 5);
            TopByUnits.Clear();
            foreach (var it in topUnits) TopByUnits.Add(it);

            var topRev = await _dashboard.GetTopByRevenueAsync(fromUtc, toUtc, 5);
            TopByRevenue.Clear();
            foreach (var it in topRev) TopByRevenue.Add(it);
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
}

