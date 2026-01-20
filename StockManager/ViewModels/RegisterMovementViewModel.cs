using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StockManager.Application.Dtos;
using StockManager.Application.Services;
using StockManager.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockManager.ViewModels;

public partial class RegisterMovementViewModel : ObservableObject
{
    private readonly IStockMovementService _service;

    public int SkuId { get; }
    public bool IsCaseSku { get; }

    [ObservableProperty]
    private string skuName;

    // Enum “real”
    [ObservableProperty]
    private StockMovementType type = StockMovementType.Sale;

    [ObservableProperty]
    private bool isSale = true;

    // Opciones para el ComboBox (texto lindo + valor real)
    public IReadOnlyList<MovementTypeOption> MovementTypeOptions { get; } =
        new List<MovementTypeOption>
        {
            new(StockMovementType.Sale, "Venta"),
            new(StockMovementType.PurchaseEntry, "Compra"),
            new(StockMovementType.Adjustment, "Ajuste de stock (+ / -)"),
            new(StockMovementType.Shrinkage, "Pérdida / merma")
        };

    public IReadOnlyList<PaymentMethodOption> PaymentMethodOptions { get; } =
        new List<PaymentMethodOption>
        {
            new(PaymentMethod.Cash, "Efectivo"),
            new(PaymentMethod.MercadoPago, "MercadoPago / Tarjeta")
        };

    public IReadOnlyList<CaseStockKindOption> CaseStockKindOptions { get; } =
        new List<CaseStockKindOption>
        {
            new(CaseStockKind.Women, "Funda de mujer"),
            new(CaseStockKind.Men, "Funda de hombre")
        };

    // Lo que selecciona el ComboBox
    private MovementTypeOption selectedTypeOption = null!;
    public MovementTypeOption SelectedTypeOption
    {
        get => selectedTypeOption;
        set
        {
            if (SetProperty(ref selectedTypeOption, value))
            {
                // Sincroniza el enum real
                Type = value.Value;
            }
        }
    }

    private PaymentMethodOption selectedPaymentMethodOption = null!;
    public PaymentMethodOption SelectedPaymentMethodOption
    {
        get => selectedPaymentMethodOption;
        set => SetProperty(ref selectedPaymentMethodOption, value);
    }

    private CaseStockKindOption? selectedCaseStockKindOption;
    public CaseStockKindOption? SelectedCaseStockKindOption
    {
        get => selectedCaseStockKindOption;
        set => SetProperty(ref selectedCaseStockKindOption, value);
    }

    // string para permitir "-" en ajuste
    [ObservableProperty]
    private string quantityText = "1";

    [ObservableProperty]
    private string? note;

    [ObservableProperty]
    private string? errorMessage;

    public RegisterMovementViewModel(
        IStockMovementService service,
        int skuId,
        string skuName,
        ProductCategory category)
    {
        _service = service;
        SkuId = skuId;
        SkuName = skuName;
        IsCaseSku = category == ProductCategory.Case;

        // Default: Venta
        SelectedTypeOption = MovementTypeOptions.First(x => x.Value == Type);
        SelectedPaymentMethodOption = PaymentMethodOptions.First(x => x.Value == PaymentMethod.Cash);
        SelectedCaseStockKindOption = CaseStockKindOptions.FirstOrDefault();
    }

    // Si Type cambia por algún motivo, mantenemos sincronizado el combo.
    partial void OnTypeChanged(StockMovementType value)
    {
        var opt = MovementTypeOptions.FirstOrDefault(x => x.Value == value);
        if (opt != null && !ReferenceEquals(opt, SelectedTypeOption))
            selectedTypeOption = opt; // set directo para evitar loop
        OnPropertyChanged(nameof(SelectedTypeOption));
        IsSale = value == StockMovementType.Sale;
    }

    [RelayCommand]
    public async Task SaveAsync()
    {
        ErrorMessage = null;

        if (!int.TryParse(QuantityText?.Trim(), out var qty))
        {
            ErrorMessage = "La cantidad debe ser un número entero.";
            return;
        }

        // Reglas de validación según tipo
        if (Type == StockMovementType.Adjustment)
        {
            if (qty == 0)
            {
                ErrorMessage = "El ajuste debe ser distinto de 0.";
                return;
            }

            if (string.IsNullOrWhiteSpace(Note))
            {
                ErrorMessage = "La nota es obligatoria para un ajuste.";
                return;
            }
        }
        else
        {
            // Para Entry/Sale/Shrinkage, la cantidad debe ser positiva
            if (qty <= 0)
            {
                ErrorMessage = "La cantidad debe ser mayor a 0.";
                return;
            }

            if (Type == StockMovementType.Shrinkage && string.IsNullOrWhiteSpace(Note))
            {
                ErrorMessage = "La nota es obligatoria para una merma.";
                return;
            }
        }

        try
        {
            var req = new RegisterMovementRequest
            {
                SkuId = SkuId,
                Type = Type,
                Note = string.IsNullOrWhiteSpace(Note) ? null : Note.Trim(),
                PaymentMethod = Type == StockMovementType.Sale
                    ? SelectedPaymentMethodOption?.Value
                    : null
            };

            if (IsCaseSku)
            {
                if (SelectedCaseStockKindOption == null)
                {
                    ErrorMessage = "Seleccioná si la funda es de mujer u hombre.";
                    return;
                }

                req.CaseStockKind = SelectedCaseStockKindOption.Value;
            }

            if (Type == StockMovementType.Adjustment)
            {
                // Adjustment acepta signo
                req.SignedQuantity = qty;

                // no se usa en Adjustment, pero lo dejamos válido
                req.Quantity = 1;
            }
            else
            {
                // Los demás tipos usan Quantity positiva
                req.Quantity = qty;
                req.SignedQuantity = null;
            }

            await _service.RegisterAsync(req);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    
}
