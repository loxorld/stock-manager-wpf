using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StockManager.Application.Dtos;
using StockManager.Application.Services;
using StockManager.Domain.Enums;

namespace StockManager.ViewModels;

public partial class RegisterMovementViewModel : ObservableObject
{
    private readonly IStockMovementService _service;

    public int SkuId { get; }

    [ObservableProperty]
    private string skuName;

    [ObservableProperty]
    private StockMovementType type = StockMovementType.Sale;

    // 🔥 Importante: string para permitir escribir "-" y validar después
    [ObservableProperty]
    private string quantityText = "1";

    [ObservableProperty]
    private string? note;

    [ObservableProperty]
    private string? errorMessage;

    public RegisterMovementViewModel(IStockMovementService service, int skuId, string skuName)
    {
        _service = service;
        SkuId = skuId;
        SkuName = skuName;
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
                Note = string.IsNullOrWhiteSpace(Note) ? null : Note.Trim()
            };

            if (Type == StockMovementType.Adjustment)
            {
                // Adjustment acepta signo
                req.SignedQuantity = qty;
                req.Quantity = 1; // no se usa en Adjustment, pero lo dejamos con un valor válido
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
            return;
        }
    }
}


