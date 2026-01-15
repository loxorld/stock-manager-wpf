using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StockManager.Application.Dtos;
using StockManager.Application.Services;
using StockManager.Domain.Enums;
using System.Collections.ObjectModel;

namespace StockManager.ViewModels;

public partial class StockViewModel : ObservableObject
{
    private readonly ISkuQueryService _skuQueryService;
    private readonly IStockMovementService _movementService;
    private readonly IStockMovementQueryService _movementQueryService;
    private readonly Debouncer _debouncer = new();

    public ObservableCollection<SkuListItemDto> Items { get; } = new();

    [ObservableProperty] private SkuListItemDto? selectedItem;
    [ObservableProperty] private string searchText = "";
    [ObservableProperty] private bool isLoading;

    // ====== Filtros ======
    public IReadOnlyList<CategoryFilterOption> CategoryOptions { get; } =
        new List<CategoryFilterOption>(
            new[] { new CategoryFilterOption(null, "Todas") }
            .Concat(Enum.GetValues(typeof(ProductCategory))
                .Cast<ProductCategory>()
                .Select(c => new CategoryFilterOption(c, c.ToString())))
        );

    private CategoryFilterOption selectedCategoryOption;
    public CategoryFilterOption SelectedCategoryOption
    {
        get => selectedCategoryOption;
        set
        {
            if (SetProperty(ref selectedCategoryOption, value))
                _debouncer.Debounce(250, LoadAsync);
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
                _debouncer.Debounce(250, LoadAsync);
        }
    }

    [ObservableProperty] private bool stockLowOnly;
    [ObservableProperty] private int stockLowThreshold = 5;

    // ====== Panel detalle ======
    [ObservableProperty] private bool isDetailLoading;

    [ObservableProperty] private string detailTitle = "Seleccioná un ítem";
    [ObservableProperty] private string detailCategory = "-";
    
    [ObservableProperty] private string detailActive = "-";

    [ObservableProperty] private int detailStock;
    [ObservableProperty] private decimal detailPrice;
    [ObservableProperty] private decimal detailCost;
    [ObservableProperty] private decimal detailMargin;

    [ObservableProperty] private string lastMovementText = "-";

    public StockViewModel(
        ISkuQueryService skuQueryService,
        IStockMovementService movementService,
        IStockMovementQueryService movementQueryService)
    {
        _skuQueryService = skuQueryService;
        _movementService = movementService;
        _movementQueryService = movementQueryService;

        selectedCategoryOption = CategoryOptions[0]; // "Todas"
    }

    // Auto-búsqueda
    partial void OnSearchTextChanged(string value)
        => _debouncer.Debounce(250, LoadAsync);

    partial void OnStockLowOnlyChanged(bool value)
        => _debouncer.Debounce(250, LoadAsync);

    partial void OnStockLowThresholdChanged(int value)
        => _debouncer.Debounce(250, LoadAsync);

    // Cuando cambia el seleccionado, cargo el panel derecho
    partial void OnSelectedItemChanged(SkuListItemDto? value)
        => _debouncer.Debounce(150, LoadSelectedDetailAsync);

    [RelayCommand]
    public async Task LoadAsync()
    {
        var selectedId = SelectedItem?.Id; //  guardar antes de refrescar

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

            Items.Clear();
            foreach (var x in data)
                Items.Add(x);

            //  restaurar selección
            RestoreSelection(selectedId);
        }
        finally
        {
            IsLoading = false;
        }
    }


    [RelayCommand]
    public Task SearchAsync() => LoadAsync();

    // ====== Cargar detalle ======
    [RelayCommand]
    public async Task LoadSelectedDetailAsync()
    {
        if (SelectedItem == null)
        {
            DetailTitle = "Seleccioná un ítem";
            DetailCategory = "-";
            
            DetailActive = "-";
            DetailStock = 0;
            DetailPrice = 0;
            DetailCost = 0;
            DetailMargin = 0;
            LastMovementText = "-";
            return;
        }

        IsDetailLoading = true;
        try
        {
            // Datos visibles del listado
            DetailTitle = SelectedItem.Name;
            DetailCategory = SelectedItem.Category;
            
            DetailStock = SelectedItem.Stock;
            DetailPrice = SelectedItem.Price;

            // Traer costo/activo desde detalle 
            var detail = await _skuQueryService.GetByIdAsync(SelectedItem.Id);
            if (detail != null)
            {
                DetailCost = detail.Cost;
                DetailActive = detail.Active ? "Activo" : "Inactivo";
                DetailMargin = DetailPrice - DetailCost;
            }
            else
            {
                DetailCost = 0;
                DetailActive = "-";
                DetailMargin = 0;
            }

            // Último movimiento 
            var history = await _movementQueryService.GetBySkuAsync(SelectedItem.Id);
            var last = history.FirstOrDefault();
            LastMovementText = last == null
                ? "-"
                : $"{last.CreatedAt:dd/MM HH:mm} · {last.Type} · {last.SignedQuantity:+0;-0;0}";
        }
        finally
        {
            IsDetailLoading = false;
        }
    }

    // ====== Acciones rápidas ======
    [RelayCommand]
    public async Task QuickPurchaseAsync()
    {
        if (SelectedItem == null) return;

        try
        {
            await _movementService.RegisterAsync(new RegisterMovementRequest
            {
                SkuId = SelectedItem.Id,
                Type = StockMovementType.PurchaseEntry,
                Quantity = 1,
                Note = "Compra rápida (+1)"
            });

            await LoadAsync();
            await LoadSelectedDetailAsync();
        }
        catch (Exception ex)
        {
            StockManager.Views.UiError.Show(ex, "No se pudo registrar la compra");
        }
    }

    [RelayCommand]
    public async Task QuickSaleAsync()
    {
        if (SelectedItem == null) return;

        try
        {
            await _movementService.RegisterAsync(new RegisterMovementRequest
            {
                SkuId = SelectedItem.Id,
                Type = StockMovementType.Sale,
                Quantity = 1,
                PaymentMethod = PaymentMethod.Cash,
                Note = "Venta rápida (-1)"
            });

            await LoadAsync();
            await LoadSelectedDetailAsync();
        }
        catch (Exception ex)
        {
            StockManager.Views.UiError.Show(ex, "No se pudo registrar la venta");
        }
    }

    private void RestoreSelection(int? selectedId)
    {
        if (selectedId == null) return;

        var match = Items.FirstOrDefault(x => x.Id == selectedId.Value);
        if (match != null)
            SelectedItem = match;
    }
}
