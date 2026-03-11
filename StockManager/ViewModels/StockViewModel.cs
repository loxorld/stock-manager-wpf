using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StockManager.Application.Dtos;
using StockManager.Application.Services;
using StockManager.Domain.Enums;
using StockManager.Infrastructure.Time;
using StockManager.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

namespace StockManager.ViewModels;

public partial class StockViewModel : ObservableObject
{
    private readonly ISkuQueryService _skuQueryService;
    private readonly IStockMovementService _movementService;
    private readonly IStockMovementQueryService _movementQueryService;
    private readonly Debouncer _debouncer = new();
    private string? _currentSortMemberPath;
    private ListSortDirection? _currentSortDirection;
    private int? _requestedSelectionId;

    public ObservableCollection<SkuListItemDto> Items { get; } = new();
    public ObservableCollection<SkuListItemDto> CriticalItems { get; } = new();
    public ICollectionView ItemsView { get; }

    [ObservableProperty] private SkuListItemDto? selectedItem;
    [ObservableProperty] private string searchText = "";
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private bool isEmpty = true;
    [ObservableProperty] private decimal totalCost;
    [ObservableProperty] private decimal totalPrice;
    [ObservableProperty] private decimal totalMargin;
    [ObservableProperty] private bool stockLowOnly;
    [ObservableProperty] private int stockLowThreshold = 5;
    [ObservableProperty] private bool stockZeroOnly;
    [ObservableProperty] private bool soldTodayOnly;
    [ObservableProperty] private bool hasCriticalItems;
    [ObservableProperty] private string criticalItemsSummary = "Sin items criticos.";
    [ObservableProperty] private bool isCriticalItemsExpanded;
    [ObservableProperty] private bool isFiltersExpanded;
    [ObservableProperty] private bool isCompactMode;

    [ObservableProperty] private bool isDetailLoading;
    [ObservableProperty] private string detailTitle = "Selecciona un item";
    [ObservableProperty] private string detailCategory = "-";
    [ObservableProperty] private string detailActive = "-";
    [ObservableProperty] private bool isCaseDetail;
    [ObservableProperty] private bool isGenderedDetail;
    [ObservableProperty] private int detailCaseStockWomen;
    [ObservableProperty] private int detailCaseStockMen;
    [ObservableProperty] private int detailStock;
    [ObservableProperty] private decimal detailPrice;
    [ObservableProperty] private decimal detailCost;
    [ObservableProperty] private decimal detailMargin;
    [ObservableProperty] private string lastMovementText = "-";

    public IReadOnlyList<CategoryFilterOption> CategoryOptions { get; } =
        new List<CategoryFilterOption>
        {
            new(null, "Todas")
        }
        .Concat(Enum.GetValues(typeof(ProductCategory))
            .Cast<ProductCategory>()
            .Select(c => new CategoryFilterOption(c, c.ToString())))
        .ToList();

    private CategoryFilterOption selectedCategoryOption;
    public CategoryFilterOption SelectedCategoryOption
    {
        get => selectedCategoryOption;
        set
        {
            if (SetProperty(ref selectedCategoryOption, value))
            {
                NotifyQuickFiltersChanged();
                _debouncer.Debounce(250, LoadAsync);
            }
        }
    }

    public IReadOnlyList<ActiveFilter> ActiveOptions { get; } =
        Enum.GetValues(typeof(ActiveFilter)).Cast<ActiveFilter>().ToList();

    private ActiveFilter selectedActive = ActiveFilter.All;
    public ActiveFilter SelectedActive
    {
        get => selectedActive;
        set
        {
            if (SetProperty(ref selectedActive, value))
            {
                NotifyQuickFiltersChanged();
                _debouncer.Debounce(250, LoadAsync);
            }
        }
    }

    public bool QuickActiveOnly => SelectedActive == ActiveFilter.Active;
    public bool QuickCasesOnly => SelectedCategoryOption.Value == ProductCategory.Case;
    public bool QuickProtectorsOnly => SelectedCategoryOption.Value == ProductCategory.ScreenProtector;
    public bool QuickOutOfStockOnly => StockZeroOnly;
    public bool QuickSoldTodayOnly => SoldTodayOnly;
    public double GridRowHeight => IsCompactMode ? 30d : 52d;
    public double GridFontSize => IsCompactMode ? 12d : 13d;
    public string GridDensityLabel => IsCompactMode ? "Modo normal" : "Modo compacto";
    public string CriticalItemsToggleLabel => IsCriticalItemsExpanded ? "Ocultar" : "Mostrar";
    public string FiltersToggleLabel => IsFiltersExpanded ? "Ocultar filtros" : "Mostrar filtros";
    public bool HasSelectedItem => SelectedItem != null;
    public string ItemsSummaryText => Items.Count == 1 ? "1 item visible" : $"{Items.Count} items visibles";
    public string FilterSummaryText => BuildFilterSummaryText();
    public string CriticalItemsBadgeText => HasCriticalItems
        ? (CriticalItems.Count == 1 ? "1 item critico" : $"{CriticalItems.Count} items criticos")
        : "Todo en rango";

    public StockViewModel(
        ISkuQueryService skuQueryService,
        IStockMovementService movementService,
        IStockMovementQueryService movementQueryService)
    {
        _skuQueryService = skuQueryService;
        _movementService = movementService;
        _movementQueryService = movementQueryService;
        ItemsView = CollectionViewSource.GetDefaultView(Items);

        selectedCategoryOption = CategoryOptions[0];

        Items.CollectionChanged += (_, _) =>
        {
            UpdateEmptyState();
            UpdateTotals();
            NotifyVisualStateChanged();
        };

        CriticalItems.CollectionChanged += (_, _) =>
        {
            UpdateCriticalItemsState();
            NotifyVisualStateChanged();
        };
    }

    partial void OnSearchTextChanged(string value)
    {
        NotifyVisualStateChanged();
        _debouncer.Debounce(250, LoadAsync);
    }

    partial void OnStockLowOnlyChanged(bool value)
    {
        NotifyVisualStateChanged();
        _debouncer.Debounce(250, LoadAsync);
    }

    partial void OnStockLowThresholdChanged(int value)
    {
        NotifyVisualStateChanged();
        _debouncer.Debounce(250, LoadAsync);
    }

    partial void OnStockZeroOnlyChanged(bool value)
    {
        NotifyQuickFiltersChanged();
        NotifyVisualStateChanged();
        _debouncer.Debounce(250, LoadAsync);
    }

    partial void OnSoldTodayOnlyChanged(bool value)
    {
        NotifyQuickFiltersChanged();
        NotifyVisualStateChanged();
        _debouncer.Debounce(250, LoadAsync);
    }

    partial void OnSelectedItemChanged(SkuListItemDto? value)
    {
        OnPropertyChanged(nameof(HasSelectedItem));
        _debouncer.Debounce(150, LoadSelectedDetailAsync);
    }

    partial void OnIsCriticalItemsExpandedChanged(bool value)
    {
        OnPropertyChanged(nameof(CriticalItemsToggleLabel));
        NotifyVisualStateChanged();
    }

    partial void OnIsFiltersExpandedChanged(bool value)
        => OnPropertyChanged(nameof(FiltersToggleLabel));

    partial void OnIsCompactModeChanged(bool value)
    {
        OnPropertyChanged(nameof(GridRowHeight));
        OnPropertyChanged(nameof(GridFontSize));
        OnPropertyChanged(nameof(GridDensityLabel));
        NotifyVisualStateChanged();
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        var selectedId = _requestedSelectionId ?? SelectedItem?.Id;
        _requestedSelectionId = null;

        IsLoading = true;
        try
        {
            bool? active = SelectedActive switch
            {
                ActiveFilter.Active => true,
                ActiveFilter.Inactive => false,
                _ => null
            };

            var threshold = StockLowThreshold < 0 ? 0 : StockLowThreshold;
            int? stockMax = StockLowOnly ? threshold : null;

            var data = await _skuQueryService.GetAllAsync(
                searchText: SearchText,
                category: SelectedCategoryOption.Value,
                active: active,
                stockMax: stockMax
            );

            if (StockZeroOnly)
                data = data.Where(x => x.Stock == 0).ToList();

            if (SoldTodayOnly)
            {
                var today = BusinessTime.GetBusinessToday();
                var (fromUtc, toUtc) = BusinessTime.GetUtcRangeForBusinessDates(today, today);
                var soldIds = await _movementQueryService.GetSkuIdsWithSalesBetweenAsync(fromUtc, toUtc);
                var soldTodaySet = soldIds.ToHashSet();
                data = data.Where(x => soldTodaySet.Contains(x.Id)).ToList();
            }

            Items.Clear();
            foreach (var x in data)
                Items.Add(x);

            ApplyStoredSort();
            RestoreSelection(selectedId);
            UpdateEmptyState();
            UpdateTotals();

            await LoadCriticalItemsAsync();
        }
        catch (Exception ex)
        {
            StockManager.Views.UiError.Show(ex, "No se pudo cargar el stock");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public Task SearchAsync() => LoadAsync();

    [RelayCommand]
    public void ToggleQuickFilter(string? filterKey)
    {
        switch (filterKey)
        {
            case "Active":
                SelectedActive = QuickActiveOnly ? ActiveFilter.All : ActiveFilter.Active;
                break;

            case "OutOfStock":
                StockZeroOnly = !StockZeroOnly;
                break;

            case "Cases":
                SelectedCategoryOption = QuickCasesOnly
                    ? CategoryOptions[0]
                    : CategoryOptions.First(x => x.Value == ProductCategory.Case);
                break;

            case "Protectors":
                SelectedCategoryOption = QuickProtectorsOnly
                    ? CategoryOptions[0]
                    : CategoryOptions.First(x => x.Value == ProductCategory.ScreenProtector);
                break;

            case "SoldToday":
                SoldTodayOnly = !SoldTodayOnly;
                break;

            case "Clear":
                SelectedActive = ActiveFilter.All;
                SelectedCategoryOption = CategoryOptions[0];
                StockZeroOnly = false;
                SoldTodayOnly = false;
                break;
        }
    }

    [RelayCommand]
    public void ToggleCriticalItemsExpanded()
        => IsCriticalItemsExpanded = !IsCriticalItemsExpanded;

    [RelayCommand]
    public void ToggleFiltersExpanded()
        => IsFiltersExpanded = !IsFiltersExpanded;

    [RelayCommand]
    public void ToggleGridDensity()
        => IsCompactMode = !IsCompactMode;

    [RelayCommand]
    public async Task LoadSelectedDetailAsync()
    {
        if (SelectedItem == null)
        {
            ClearDetail();
            return;
        }

        IsDetailLoading = true;
        try
        {
            DetailTitle = SelectedItem.Name;
            DetailCategory = SelectedItem.Category;
            DetailStock = SelectedItem.Stock;
            DetailPrice = SelectedItem.Price;

            var detail = await _skuQueryService.GetByIdAsync(SelectedItem.Id);
            if (detail != null)
            {
                DetailCost = detail.Cost;
                DetailActive = detail.Active ? "Activo" : "Inactivo";
                DetailMargin = DetailPrice - DetailCost;
                IsCaseDetail = detail.Category == ProductCategory.Case;
                IsGenderedDetail = detail.Category == ProductCategory.Case
                    && detail.CaseType != CaseType.Transparent;
                DetailStock = detail.Stock;
                DetailCaseStockWomen = detail.CaseStockWomen;
                DetailCaseStockMen = detail.CaseStockMen;
            }
            else
            {
                DetailCost = 0;
                DetailActive = "-";
                DetailMargin = 0;
                IsCaseDetail = false;
                IsGenderedDetail = false;
                DetailCaseStockWomen = 0;
                DetailCaseStockMen = 0;
            }

            var last = await _movementQueryService.GetLastBySkuAsync(SelectedItem.Id);
            LastMovementText = last == null
                ? "-"
                : $"{last.CreatedAt:dd/MM HH:mm} - {last.TypeLabel} - {last.SignedQuantity:+0;-0;0}";
        }
        catch (Exception ex)
        {
            ClearDetail();
            StockManager.Views.UiError.Show(ex, "No se pudo cargar el detalle");
        }
        finally
        {
            IsDetailLoading = false;
        }
    }

    [RelayCommand]
    public async Task QuickPurchaseAsync()
    {
        if (SelectedItem == null)
            return;

        await RegisterQuickPurchaseAsync(SelectedItem);
    }

    [RelayCommand]
    public async Task ReplenishCriticalItemAsync(SkuListItemDto? item)
    {
        if (item == null)
            return;

        await RegisterQuickPurchaseAsync(item);
    }

    [RelayCommand]
    public async Task QuickSaleAsync()
    {
        if (SelectedItem == null)
            return;

        try
        {
            var quickSaleSelection = GetQuickSaleSelectionOrNull(SelectedItem);
            var caseStockKind = quickSaleSelection?.CaseStockKind;
            var paymentMethod = quickSaleSelection?.PaymentMethod
                ?? GetQuickSalePaymentMethodOrNull(SelectedItem);

            if (paymentMethod is null)
                return;

            if (SelectedItem.CategoryValue == ProductCategory.Case
                && SelectedItem.CaseType != CaseType.Transparent
                && caseStockKind is null)
                return;

            await _movementService.RegisterAsync(new RegisterMovementRequest
            {
                SkuId = SelectedItem.Id,
                Type = StockMovementType.Sale,
                Quantity = 1,
                PaymentMethod = paymentMethod,
                CaseStockKind = caseStockKind,
                Note = "Venta rapida (-1)"
            });

            SelectItemOnNextLoad(SelectedItem.Id);
            await LoadAsync();
            await LoadSelectedDetailAsync();
            UiToast.ShowSuccess("Venta rapida registrada.");
        }
        catch (Exception ex)
        {
            UiError.Show(ex, "No se pudo registrar la venta");
        }
    }

    public void ToggleSort(string sortMemberPath)
    {
        if (string.IsNullOrWhiteSpace(sortMemberPath))
            return;

        if (string.Equals(_currentSortMemberPath, sortMemberPath, StringComparison.Ordinal))
        {
            _currentSortDirection = _currentSortDirection == ListSortDirection.Ascending
                ? ListSortDirection.Descending
                : ListSortDirection.Ascending;
        }
        else
        {
            _currentSortMemberPath = sortMemberPath;
            _currentSortDirection = ListSortDirection.Ascending;
        }

        ApplyStoredSort();
    }

    public ListSortDirection? GetSortDirection(string sortMemberPath)
    {
        if (!string.Equals(_currentSortMemberPath, sortMemberPath, StringComparison.Ordinal))
            return null;

        return _currentSortDirection;
    }

    public void SelectItemOnNextLoad(int id)
    {
        _requestedSelectionId = id;
    }

    private async Task RegisterQuickPurchaseAsync(SkuListItemDto item)
    {
        try
        {
            var caseStockKind = GetQuickCaseStockKindOrNull(item);
            if (item.CategoryValue == ProductCategory.Case
                && item.CaseType != CaseType.Transparent
                && caseStockKind is null)
                return;

            await _movementService.RegisterAsync(new RegisterMovementRequest
            {
                SkuId = item.Id,
                Type = StockMovementType.PurchaseEntry,
                Quantity = 1,
                CaseStockKind = caseStockKind,
                Note = "Compra rapida (+1)"
            });

            SelectItemOnNextLoad(item.Id);
            await LoadAsync();
            await LoadSelectedDetailAsync();
            UiToast.ShowSuccess("Compra rapida registrada.");
        }
        catch (Exception ex)
        {
            UiError.Show(ex, "No se pudo registrar la compra");
        }
    }

    private QuickSaleSelection? GetQuickSaleSelectionOrNull(SkuListItemDto item)
    {
        if (item.CategoryValue != ProductCategory.Case)
            return null;

        if (item.CaseType == CaseType.Transparent)
            return null;

        var dialog = new StockManager.Views.CaseStockKindWindow(showPaymentMethodSelection: true)
        {
            Owner = System.Windows.Application.Current?.MainWindow
        };

        var result = dialog.ShowDialog();
        if (result != true)
            return null;

        return dialog.SelectedCaseStockKind == null || dialog.SelectedPaymentMethod == null
            ? null
            : new QuickSaleSelection(dialog.SelectedCaseStockKind.Value, dialog.SelectedPaymentMethod.Value);
    }

    private PaymentMethod? GetQuickSalePaymentMethodOrNull(SkuListItemDto item)
    {
        if (item.CategoryValue == ProductCategory.Case
            && item.CaseType != CaseType.Transparent)
            return null;

        var dialog = new StockManager.Views.PaymentMethodWindow
        {
            Owner = System.Windows.Application.Current?.MainWindow
        };

        var result = dialog.ShowDialog();
        return result == true ? dialog.SelectedPaymentMethod : null;
    }

    private CaseStockKind? GetQuickCaseStockKindOrNull(SkuListItemDto item)
    {
        if (item.CategoryValue != ProductCategory.Case)
            return null;

        if (item.CaseType == CaseType.Transparent)
            return null;

        var dialog = new StockManager.Views.CaseStockKindWindow
        {
            Owner = System.Windows.Application.Current?.MainWindow
        };

        var result = dialog.ShowDialog();
        return result == true ? dialog.SelectedCaseStockKind : null;
    }

    private void RestoreSelection(int? selectedId)
    {
        if (selectedId == null)
        {
            SelectedItem = null;
            return;
        }

        SelectedItem = Items.FirstOrDefault(x => x.Id == selectedId.Value);
    }

    private void ApplyStoredSort()
    {
        using var _ = ItemsView.DeferRefresh();
        ItemsView.SortDescriptions.Clear();

        if (string.IsNullOrWhiteSpace(_currentSortMemberPath) || _currentSortDirection is null)
            return;

        ItemsView.SortDescriptions.Add(
            new SortDescription(_currentSortMemberPath, _currentSortDirection.Value));
    }

    private async Task LoadCriticalItemsAsync()
    {
        var threshold = StockLowThreshold < 0 ? 0 : StockLowThreshold;
        var data = await _skuQueryService.GetCriticalStockAsync(threshold);

        CriticalItems.Clear();
        foreach (var item in data)
            CriticalItems.Add(item);

        UpdateCriticalItemsState();
    }

    private void UpdateCriticalItemsState()
    {
        var threshold = StockLowThreshold < 0 ? 0 : StockLowThreshold;
        HasCriticalItems = CriticalItems.Count > 0;
        CriticalItemsSummary = HasCriticalItems
            ? $"{CriticalItems.Count} item(s) con stock en {threshold} o menos."
            : "Sin items criticos para reponer.";
    }

    private void UpdateEmptyState()
    {
        IsEmpty = Items.Count == 0;
    }

    private void ClearDetail()
    {
        DetailTitle = "Selecciona un item";
        DetailCategory = "-";
        DetailActive = "-";
        IsCaseDetail = false;
        IsGenderedDetail = false;
        DetailStock = 0;
        DetailCaseStockWomen = 0;
        DetailCaseStockMen = 0;
        DetailPrice = 0;
        DetailCost = 0;
        DetailMargin = 0;
        LastMovementText = "-";
    }

    private void UpdateTotals()
    {
        var totalCostValue = Items.Sum(item => item.Cost * item.Stock);
        var totalPriceValue = Items.Sum(item => item.Price * item.Stock);

        TotalCost = totalCostValue;
        TotalPrice = totalPriceValue;
        TotalMargin = totalPriceValue - totalCostValue;
    }

    private void NotifyQuickFiltersChanged()
    {
        OnPropertyChanged(nameof(QuickActiveOnly));
        OnPropertyChanged(nameof(QuickCasesOnly));
        OnPropertyChanged(nameof(QuickProtectorsOnly));
        OnPropertyChanged(nameof(QuickOutOfStockOnly));
        OnPropertyChanged(nameof(QuickSoldTodayOnly));
        OnPropertyChanged(nameof(FilterSummaryText));
    }

    private void NotifyVisualStateChanged()
    {
        OnPropertyChanged(nameof(ItemsSummaryText));
        OnPropertyChanged(nameof(FilterSummaryText));
        OnPropertyChanged(nameof(CriticalItemsBadgeText));
    }

    private string BuildFilterSummaryText()
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(SearchText))
            parts.Add($"Busqueda: {SearchText.Trim()}");

        if (SelectedCategoryOption.Value is not null)
            parts.Add($"Categoria: {SelectedCategoryOption.Display}");

        if (SelectedActive != ActiveFilter.All)
            parts.Add(SelectedActive == ActiveFilter.Active ? "Solo activos" : "Solo inactivos");

        if (StockLowOnly)
            parts.Add($"Stock bajo <= {Math.Max(0, StockLowThreshold)}");

        if (StockZeroOnly)
            parts.Add("Sin stock");

        if (SoldTodayOnly)
            parts.Add("Vendidos hoy");

        return parts.Count == 0
            ? "Sin filtros rapidos activos."
            : string.Join(" · ", parts);
    }

    private sealed record QuickSaleSelection(CaseStockKind CaseStockKind, PaymentMethod PaymentMethod);
}
