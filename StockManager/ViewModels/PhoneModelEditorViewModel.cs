using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StockManager.Application.Dtos;
using StockManager.Application.Services;

namespace StockManager.ViewModels;

public partial class PhoneModelEditorViewModel : ObservableObject
{
    private readonly IPhoneModelCommandService _cmd;

    public int? Id { get; }

    [ObservableProperty] private string title = "";
    [ObservableProperty] private string brand = "";
    [ObservableProperty] private string modelName = "";
    [ObservableProperty] private string? errorMessage;

    public PhoneModelEditorViewModel(IPhoneModelCommandService cmd, int? id, string brand, string modelName)
    {
        _cmd = cmd;
        Id = id;

        Title = id is null ? "Nuevo modelo" : "Editar modelo";
        Brand = brand ?? "";
        ModelName = modelName ?? "";
    }

    [RelayCommand]
    public async Task SaveAsync()
    {
        ErrorMessage = null;

        // Validación local rápida 
        if (string.IsNullOrWhiteSpace(Brand))
        {
            ErrorMessage = "La marca es obligatoria.";
            return;
        }

        if (string.IsNullOrWhiteSpace(ModelName))
        {
            ErrorMessage = "El nombre del modelo es obligatorio.";
            return;
        }

        try
        {
            var req = new UpsertPhoneModelRequest
            {
                Id = Id,
                Brand = Brand.Trim(),
                ModelName = ModelName.Trim()
            };

            if (Id is null)
                await _cmd.CreateAsync(req);
            else
                await _cmd.UpdateAsync(req);
        }
        catch (Exception ex)
        {
            // Errores esperables (duplicado, inexistente, etc.)
            ErrorMessage = ex.Message;
        }
    }
}
