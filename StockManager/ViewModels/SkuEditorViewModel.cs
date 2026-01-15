using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StockManager.Application.Dtos;
using StockManager.Application.Services;
using StockManager.Domain.Enums;
using System.Collections.ObjectModel;
using System.Globalization;

namespace StockManager.ViewModels;

public partial class SkuEditorViewModel : ObservableObject
{
    private readonly ISkuQueryService _skuQuery;
    private readonly ISkuCommandService _skuCommand;
    

    public int? Id { get; }

    

    // ✅ Listas para combos (enums)
    public IReadOnlyList<ProductCategory> CategoryOptions { get; } =
        Enum.GetValues(typeof(ProductCategory)).Cast<ProductCategory>().ToList();

    public IReadOnlyList<CaseType> CaseTypeOptions { get; } =
        Enum.GetValues(typeof(CaseType)).Cast<CaseType>().ToList();

    public IReadOnlyList<ProtectorType> ProtectorTypeOptions { get; } =
        Enum.GetValues(typeof(ProtectorType)).Cast<ProtectorType>().ToList();

    [ObservableProperty] private string title = "Nuevo SKU";
    [ObservableProperty] private string name = "";

    private ProductCategory category = ProductCategory.Accessory;
    public ProductCategory Category
    {
        get => category;
        set
        {
            if (SetProperty(ref category, value))
            {
                // Cuando cambia categoría, limpiamos lo que no aplica.
                if (category == ProductCategory.Accessory)
                {
                    PhoneModelId = null;
                    CaseType = null;
                    ProtectorType = null;
                }
                else if (category == ProductCategory.Case)
                {
                    ProtectorType = null;
                }
                else if (category == ProductCategory.ScreenProtector)
                {
                    CaseType = null;
                }

                OnPropertyChanged(nameof(IsPhoneModelEnabled));
                OnPropertyChanged(nameof(IsCaseTypeEnabled));
                OnPropertyChanged(nameof(IsProtectorTypeEnabled));
            }
        }
    }

    [ObservableProperty] private int? phoneModelId;
    [ObservableProperty] private CaseType? caseType;
    [ObservableProperty] private ProtectorType? protectorType;

    [ObservableProperty] private string costText = "0";
    [ObservableProperty] private string priceText = "0";
    [ObservableProperty] private bool active = true;
    [ObservableProperty] private string? errorMessage;

    // ✅ Para habilitar/deshabilitar campos según categoría (no ocultamos, pero queda claro y consistente)
    public bool IsPhoneModelEnabled => Category != ProductCategory.Accessory;
    public bool IsCaseTypeEnabled => Category == ProductCategory.Case;
    public bool IsProtectorTypeEnabled => Category == ProductCategory.ScreenProtector;

    public SkuEditorViewModel(
        ISkuQueryService skuQuery,
        ISkuCommandService skuCommand,
        int? id = null)
    {
        _skuQuery = skuQuery;
        _skuCommand = skuCommand;
        
        Id = id;
        Title = id is null ? "Nuevo SKU" : "Editar SKU";
    }

    public async Task InitializeAsync()
    {
        ErrorMessage = null;

        

        if (Id is null) return;

        var sku = await _skuQuery.GetByIdAsync(Id.Value);
        if (sku == null)
        {
            ErrorMessage = "No se encontró el SKU para editar.";
            return;
        }

        Name = sku.Name;
        Category = sku.Category;
        
        CaseType = sku.CaseType;
        ProtectorType = sku.ProtectorType;
        CostText = sku.Cost.ToString(CultureInfo.CurrentCulture);
        PriceText = sku.Price.ToString(CultureInfo.CurrentCulture);
        Active = sku.Active;
    }

    [RelayCommand]
    public async Task SaveAsync()
    {
        ErrorMessage = null;

        if (string.IsNullOrWhiteSpace(Name))
        {
            ErrorMessage = "El nombre es obligatorio.";
            return;
        }

        if (!decimal.TryParse(CostText?.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out var cost) || cost < 0)
        {
            ErrorMessage = "Costo inválido.";
            return;
        }

        if (!decimal.TryParse(PriceText?.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out var price) || price < 0)
        {
            ErrorMessage = "Precio inválido.";
            return;
        }

        var req = new UpsertSkuRequest
        {
            Id = Id,
            Name = Name.Trim(),
            Category = Category,
            
            CaseType = CaseType,
            ProtectorType = ProtectorType,
            Cost = cost,
            Price = price,
            Active = Active
        };

        try
        {
            if (Id is null)
                await _skuCommand.CreateAsync(req);
            else
                await _skuCommand.UpdateAsync(req);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }
}
