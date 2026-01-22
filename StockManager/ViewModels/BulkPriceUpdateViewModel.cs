using CommunityToolkit.Mvvm.ComponentModel;
using StockManager.Application.Services;
using StockManager.Domain.Enums;
using System.Globalization;

namespace StockManager.ViewModels;

public partial class BulkPriceUpdateViewModel : ObservableObject
{
    private readonly ISkuCommandService _skuCommand;

    public IReadOnlyList<CategoryFilterOption> CategoryOptions { get; } =
        new List<CategoryFilterOption>
        {
            new(ProductCategory.Case, "Fundas"),
            new(ProductCategory.ScreenProtector, "Templados")
        };

    private CategoryFilterOption selectedCategoryOption;
    public CategoryFilterOption SelectedCategoryOption
    {
        get => selectedCategoryOption;
        set
        {
            if (SetProperty(ref selectedCategoryOption, value))
            {
                EnsureDefaultTypes();
                OnPropertyChanged(nameof(IsCaseCategory));
                OnPropertyChanged(nameof(IsProtectorCategory));
            }
        }
    }

    public IReadOnlyList<CaseType> CaseTypeOptions { get; } =
        Enum.GetValues(typeof(CaseType)).Cast<CaseType>().ToList();

    public IReadOnlyList<ProtectorType> ProtectorTypeOptions { get; } =
        Enum.GetValues(typeof(ProtectorType)).Cast<ProtectorType>().ToList();

    [ObservableProperty] private CaseType? selectedCaseType;
    [ObservableProperty] private ProtectorType? selectedProtectorType;
    [ObservableProperty] private string priceText = "";
    [ObservableProperty] private string costText = "";
    [ObservableProperty] private string? errorMessage;
    [ObservableProperty] private int updatedCount;

    public bool IsCaseCategory => SelectedCategoryOption.Value == ProductCategory.Case;
    public bool IsProtectorCategory => SelectedCategoryOption.Value == ProductCategory.ScreenProtector;

    public BulkPriceUpdateViewModel(ISkuCommandService skuCommand)
    {
        _skuCommand = skuCommand;
        selectedCategoryOption = CategoryOptions[0];
        EnsureDefaultTypes();
    }

    public async Task ApplyAsync()
    {
        ErrorMessage = null;
        UpdatedCount = 0;

        decimal? price = null;
        decimal? cost = null;

        if (!string.IsNullOrWhiteSpace(PriceText))
        {
            if (!decimal.TryParse(PriceText?.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out var parsedPrice)
                || parsedPrice < 0)
            {
                ErrorMessage = "Precio inválido.";
                return;
            }

            price = parsedPrice;
        }

        if (!string.IsNullOrWhiteSpace(CostText))
        {
            if (!decimal.TryParse(CostText?.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out var parsedCost)
                || parsedCost < 0)
            {
                ErrorMessage = "Costo inválido.";
                return;
            }

            cost = parsedCost;
        }

        if (price is null && cost is null)
        {
            ErrorMessage = "Indicá un precio y/o costo.";
            return;
        }

        if (IsCaseCategory && SelectedCaseType is null)
        {
            ErrorMessage = "Seleccioná un tipo de funda.";
            return;
        }

        if (IsProtectorCategory && SelectedProtectorType is null)
        {
            ErrorMessage = "Seleccioná un tipo de templado.";
            return;
        }

        var category = SelectedCategoryOption.Value
            ?? throw new InvalidOperationException("Categoría inválida.");

        UpdatedCount = await _skuCommand.UpdatePricingByTypeAsync(
            category,
            SelectedCaseType,
            SelectedProtectorType,
            price,
            cost);
    }

    private void EnsureDefaultTypes()
    {
        if (IsCaseCategory && SelectedCaseType is null)
            SelectedCaseType = CaseTypeOptions.FirstOrDefault();

        if (IsProtectorCategory && SelectedProtectorType is null)
            SelectedProtectorType = ProtectorTypeOptions.FirstOrDefault();
    }
}