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
    public int? DuplicateFromId { get; }
    public int? SavedSkuId { get; private set; }

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
                if (category == ProductCategory.Accessory)
                {
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

                OnPropertyChanged(nameof(IsCaseTypeEnabled));
                OnPropertyChanged(nameof(IsProtectorTypeEnabled));
            }
        }
    }

    [ObservableProperty] private CaseType? caseType;
    [ObservableProperty] private ProtectorType? protectorType;

    [ObservableProperty] private string costText = "0";
    [ObservableProperty] private string priceText = "0";
    [ObservableProperty] private bool active = true;
    [ObservableProperty] private string? errorMessage;

    public bool IsCaseTypeEnabled => Category == ProductCategory.Case;
    public bool IsProtectorTypeEnabled => Category == ProductCategory.ScreenProtector;

    public SkuEditorViewModel(
        ISkuQueryService skuQuery,
        ISkuCommandService skuCommand,
        int? id = null,
        int? duplicateFromId = null)
    {
        _skuQuery = skuQuery;
        _skuCommand = skuCommand;

        Id = id;
        DuplicateFromId = duplicateFromId;
        Title = id is not null
            ? "Editar SKU"
            : duplicateFromId is not null
                ? "Duplicar SKU"
                : "Nuevo SKU";
    }

    public async Task InitializeAsync()
    {
        ErrorMessage = null;

        if (Id is null && DuplicateFromId is null)
            return;

        var skuId = Id ?? DuplicateFromId!.Value;
        var sku = await _skuQuery.GetByIdAsync(skuId);
        if (sku == null)
        {
            ErrorMessage = Id is null
                ? "No se encontro el SKU a duplicar."
                : "No se encontro el SKU para editar.";
            return;
        }

        Name = Id is null ? $"{sku.Name} copia" : sku.Name;
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
        SavedSkuId = null;

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
                SavedSkuId = await _skuCommand.CreateAsync(req);
            else
            {
                await _skuCommand.UpdateAsync(req);
                SavedSkuId = Id;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }
}
