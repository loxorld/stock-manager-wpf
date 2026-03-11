using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StockManager.Application.Dtos;
using StockManager.Application.Services;
using StockManager.Application.Text;
using StockManager.Domain.Enums;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;

namespace StockManager.ViewModels;

public partial class MovementHistoryViewModel : ObservableObject
{
    private readonly IStockMovementQueryService _query;

    public int SkuId { get; }
    public string SkuName { get; }

    public ObservableCollection<StockMovementListItemDto> Items { get; } = new();
    public ICollectionView ItemsView { get; }

    public IReadOnlyList<MovementHistoryTypeFilterOption> TypeOptions { get; } = new List<MovementHistoryTypeFilterOption>
    {
        new(null, "Todos"),
        new(StockMovementType.Sale, "Ventas"),
        new(StockMovementType.PurchaseEntry, "Compras"),
        new(StockMovementType.Adjustment, "Ajustes"),
        new(StockMovementType.Shrinkage, "Mermas")
    };

    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private string searchText = "";
    [ObservableProperty] private DateTime? fromDate;
    [ObservableProperty] private DateTime? toDate;
    [ObservableProperty] private bool hasItems;

    private MovementHistoryTypeFilterOption selectedTypeOption;
    public MovementHistoryTypeFilterOption SelectedTypeOption
    {
        get => selectedTypeOption;
        set
        {
            if (SetProperty(ref selectedTypeOption, value))
                ApplyFilters();
        }
    }

    public MovementHistoryViewModel(IStockMovementQueryService query, int skuId, string skuName)
    {
        _query = query;
        SkuId = skuId;
        SkuName = skuName;
        selectedTypeOption = TypeOptions[0];
        ItemsView = CollectionViewSource.GetDefaultView(Items);
        ItemsView.Filter = FilterItem;
        Items.CollectionChanged += (_, _) => UpdateHasItems();
    }

    partial void OnSearchTextChanged(string value) => ApplyFilters();
    partial void OnFromDateChanged(DateTime? value) => ApplyFilters();
    partial void OnToDateChanged(DateTime? value) => ApplyFilters();

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            var data = await _query.GetBySkuAsync(SkuId);
            Items.Clear();
            foreach (var x in data)
                Items.Add(x);

            ApplyFilters();
            UpdateHasItems();
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public void ClearFilters()
    {
        SelectedTypeOption = TypeOptions[0];
        FromDate = null;
        ToDate = null;
        SearchText = string.Empty;
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        ItemsView.Refresh();
        UpdateHasItems();
    }

    private bool FilterItem(object obj)
    {
        if (obj is not StockMovementListItemDto item)
            return false;

        if (SelectedTypeOption.Value.HasValue && item.TypeValue != SelectedTypeOption.Value.Value)
            return false;

        if (FromDate.HasValue && item.CreatedAt.Date < FromDate.Value.Date)
            return false;

        if (ToDate.HasValue && item.CreatedAt.Date > ToDate.Value.Date)
            return false;

        if (string.IsNullOrWhiteSpace(SearchText))
            return true;

        var search = SearchTextNormalizer.Normalize(SearchText);
        var searchable = SearchTextNormalizer.Normalize(
            $"{item.Note} {item.TypeLabel} {item.CaseStockKindLabel}");

        return searchable.Contains(search, StringComparison.Ordinal);
    }

    private void UpdateHasItems()
    {
        HasItems = ItemsView.Cast<object>().Any();
    }
}
