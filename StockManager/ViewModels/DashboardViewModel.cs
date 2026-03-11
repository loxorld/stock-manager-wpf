using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StockManager.Application.Dtos;
using StockManager.Application.Services;
using StockManager.Infrastructure.Time;
using StockManager.Views;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;

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
    private readonly IStockMovementService _stockMovementService;

    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private DashboardPeriod selectedPeriod = DashboardPeriod.Today;
    [ObservableProperty] private decimal revenue;
    [ObservableProperty] private decimal cashRevenue;
    [ObservableProperty] private decimal cardRevenue;
    [ObservableProperty] private decimal estimatedMargin;
    [ObservableProperty] private decimal inventoryCost;
    [ObservableProperty] private int unitsSold;
    [ObservableProperty] private int salesCount;
    [ObservableProperty] private int productsWithoutMovement;
    [ObservableProperty] private DateTime? fromDate;
    [ObservableProperty] private DateTime? toDate;
    [ObservableProperty] private decimal maxDailyRevenue = 1;
    [ObservableProperty] private decimal maxTopRevenue = 1;
    [ObservableProperty] private decimal maxTopMargin = 1;
    [ObservableProperty] private int maxTopUnits = 1;
    [ObservableProperty] private bool hasDailySales;
    [ObservableProperty] private bool hasTopByUnits;
    [ObservableProperty] private bool hasTopByRevenue;
    [ObservableProperty] private bool hasTopByMargin;
    [ObservableProperty] private string revenueDeltaText = "Sin cambios";
    [ObservableProperty] private string cashRevenueDeltaText = "Sin cambios";
    [ObservableProperty] private string cardRevenueDeltaText = "Sin cambios";
    [ObservableProperty] private string estimatedMarginDeltaText = "Sin cambios";
    [ObservableProperty] private string unitsSoldDeltaText = "Sin cambios";
    [ObservableProperty] private string salesCountDeltaText = "Sin cambios";
    [ObservableProperty] private string currentRangeLabel = "Hoy";
    [ObservableProperty] private string averageDailyRevenueText = "Promedio diario: $0";
    [ObservableProperty] private string bestDayText = "Sin mejor dia todavia";
    [ObservableProperty] private bool isFiltersExpanded;

    public ObservableCollection<DashboardTopItemDto> TopByUnits { get; } = new();
    public ObservableCollection<DashboardTopItemDto> TopByRevenue { get; } = new();
    public ObservableCollection<DashboardTopItemDto> TopByMargin { get; } = new();
    public ObservableCollection<DashboardDailySalesDto> DailySales { get; } = new();
    public ObservableCollection<DashboardSaleHistoryItemDto> SalesHistory { get; } = new();

    public DashboardViewModel(IDashboardQueryService dashboard, IStockMovementService stockMovementService)
    {
        _dashboard = dashboard;
        _stockMovementService = stockMovementService;

        var today = BusinessTime.GetBusinessToday();
        FromDate = today;
        ToDate = today;
        HasDailySales = true;
    }

    public string FiltersToggleLabel => IsFiltersExpanded ? "Ocultar filtros" : "Mostrar filtros";

    [RelayCommand]
    public async Task DeleteSaleAsync(DashboardSaleHistoryItemDto? sale)
    {
        if (sale == null)
            return;

        var confirm = MessageBox.Show(
            $"Queres eliminar la venta de \"{sale.SkuName}\" por {sale.Quantity} unidad(es)?\n" +
            "Esto devolvera el stock y eliminara el movimiento.",
            "Confirmar eliminacion de venta",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes)
            return;

        try
        {
            await _stockMovementService.DeleteSaleAsync(sale.Id);
            await LoadAsync();
        }
        catch (Exception ex)
        {
            UiError.Show(ex, "No se pudo eliminar la venta");
        }
    }

    [RelayCommand]
    public async Task ApplyRangeAsync()
    {
        SelectedPeriod = DashboardPeriod.Range;
        await LoadAsync();
    }

    [RelayCommand]
    public void ToggleFiltersExpanded()
        => IsFiltersExpanded = !IsFiltersExpanded;

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsLoading = true;
        HasDailySales = true;

        try
        {
            var (fromUtc, toUtc) = SelectedPeriod == DashboardPeriod.Range
                ? GetCustomRangeUtc()
                : GetRangeUtc(SelectedPeriod);

            CurrentRangeLabel = BuildRangeLabel(fromUtc, toUtc);
            var (prevFromUtc, prevToUtc) = GetPreviousRangeUtc(fromUtc, toUtc);

            var summary = await _dashboard.GetSummaryAsync(fromUtc, toUtc);
            Revenue = summary.Revenue;
            CashRevenue = summary.CashRevenue;
            CardRevenue = summary.CardRevenue;
            EstimatedMargin = summary.EstimatedMargin;
            InventoryCost = summary.InventoryCost;
            UnitsSold = summary.UnitsSold;
            SalesCount = summary.SalesCount;
            ProductsWithoutMovement = summary.ProductsWithoutMovement;

            var previousSummary = await _dashboard.GetSummaryAsync(prevFromUtc, prevToUtc);
            RevenueDeltaText = BuildDeltaText(Revenue, previousSummary.Revenue);
            CashRevenueDeltaText = BuildDeltaText(CashRevenue, previousSummary.CashRevenue);
            CardRevenueDeltaText = BuildDeltaText(CardRevenue, previousSummary.CardRevenue);
            EstimatedMarginDeltaText = BuildDeltaText(EstimatedMargin, previousSummary.EstimatedMargin);
            UnitsSoldDeltaText = BuildDeltaText(UnitsSold, previousSummary.UnitsSold);
            SalesCountDeltaText = BuildDeltaText(SalesCount, previousSummary.SalesCount);

            var history = await _dashboard.GetSalesHistoryAsync(fromUtc, toUtc);
            SalesHistory.Clear();
            foreach (var item in history)
                SalesHistory.Add(item);

            var dailySales = await _dashboard.GetDailySalesAsync(fromUtc, toUtc);
            DailySales.Clear();
            foreach (var item in dailySales)
                DailySales.Add(item);
            OnPropertyChanged(nameof(DailySales));

            MaxDailyRevenue = DailySales.Count == 0 ? 1 : DailySales.Max(x => x.Revenue);
            HasDailySales = DailySales.Count > 0;
            AverageDailyRevenueText = DailySales.Count == 0
                ? "Promedio diario: $0"
                : $"Promedio diario: {DailySales.Average(x => x.Revenue):C}";
            BestDayText = DailySales.Count == 0
                ? "Sin mejor dia todavia"
                : BuildBestDayText(DailySales
                    .OrderByDescending(x => x.Revenue)
                    .ThenByDescending(x => x.Units)
                    .First());

            if (SelectedPeriod == DashboardPeriod.Range)
            {
                Debug.WriteLine(
                    $"[Dashboard] ApplyRange FromDate={FromDate:yyyy-MM-dd} ToDate={ToDate:yyyy-MM-dd} " +
                    $"DailySalesCount={DailySales.Count} MaxDailyRevenue={MaxDailyRevenue}"
                );
            }

            TopByUnits.Clear();
            TopByRevenue.Clear();
            TopByMargin.Clear();

            var topByUnits = await _dashboard.GetTopByUnitsAsync(fromUtc, toUtc);
            foreach (var item in topByUnits)
                TopByUnits.Add(item);
            HasTopByUnits = TopByUnits.Count > 0;
            MaxTopUnits = HasTopByUnits ? TopByUnits.Max(x => x.Units) : 1;

            var topByRevenue = await _dashboard.GetTopByRevenueAsync(fromUtc, toUtc);
            foreach (var item in topByRevenue)
                TopByRevenue.Add(item);
            HasTopByRevenue = TopByRevenue.Count > 0;
            MaxTopRevenue = HasTopByRevenue ? TopByRevenue.Max(x => x.Revenue) : 1;

            var topByMargin = await _dashboard.GetTopByMarginAsync(fromUtc, toUtc);
            foreach (var item in topByMargin)
                TopByMargin.Add(item);
            HasTopByMargin = TopByMargin.Count > 0;
            MaxTopMargin = HasTopByMargin ? TopByMargin.Max(x => x.Margin) : 1;
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
            throw new ArgumentException("Selecciona Desde y Hasta.");

        var fromLocalDate = FromDate.Value.Date;
        var toLocalDate = ToDate.Value.Date;

        if (toLocalDate < fromLocalDate)
            throw new ArgumentException("La fecha Hasta no puede ser menor que Desde.");

        return BusinessTime.GetUtcRangeForBusinessDates(fromLocalDate, toLocalDate);
    }

    partial void OnSelectedPeriodChanged(DashboardPeriod value)
    {
        if (value != DashboardPeriod.Range)
            _ = LoadAsync();
    }

    partial void OnIsFiltersExpandedChanged(bool value)
        => OnPropertyChanged(nameof(FiltersToggleLabel));

    private static (DateTime fromUtc, DateTime toUtc) GetRangeUtc(DashboardPeriod period)
    {
        var nowLocal = BusinessTime.GetBusinessNow();

        DateTime startLocal = period switch
        {
            DashboardPeriod.Today => nowLocal.Date,
            DashboardPeriod.Week => nowLocal.Date.AddDays(-(int)nowLocal.DayOfWeek),
            DashboardPeriod.Month => new DateTime(nowLocal.Year, nowLocal.Month, 1),
            _ => nowLocal.Date
        };

        DateTime endLocal = period switch
        {
            DashboardPeriod.Today => startLocal.AddDays(1),
            DashboardPeriod.Week => startLocal.AddDays(7),
            DashboardPeriod.Month => startLocal.AddMonths(1),
            _ => startLocal.AddDays(1)
        };

        return BusinessTime.GetUtcRangeForBusinessDates(startLocal, endLocal.AddDays(-1));
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
            return current == 0 ? "Sin cambios" : "Nuevo periodo";

        var delta = current - previous;
        var percent = delta / previous;
        var trend = percent >= 0 ? "Sube" : "Baja";
        return $"{trend} {Math.Abs(percent):P0} vs periodo anterior";
    }

    private static string BuildRangeLabel(DateTime fromUtc, DateTime toUtc)
    {
        var fromLocal = BusinessTime.ConvertUtcToBusinessLocal(fromUtc);
        var toLocal = BusinessTime.ConvertUtcToBusinessLocal(toUtc.AddTicks(-1));

        if (fromLocal.Date == toLocal.Date)
            return $"{fromLocal:dddd dd MMM}";

        return $"{fromLocal:dd MMM} - {toLocal:dd MMM}";
    }

    private static string BuildBestDayText(DashboardDailySalesDto day)
        => $"Mejor dia: {day.Date:dd/MM} con {day.Revenue:C}";
}
