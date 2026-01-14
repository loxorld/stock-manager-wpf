using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StockManager.Application.Dtos;
using StockManager.Application.Services;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Text;

namespace StockManager.ViewModels;

public partial class PhoneModelEditorViewModel : ObservableObject
{
    private readonly IPhoneModelCommandService _cmd;

    public int? Id { get; }

    [ObservableProperty] private string title;
    [ObservableProperty] private string brand;
    [ObservableProperty] private string modelName;
    [ObservableProperty] private string? errorMessage;

    public PhoneModelEditorViewModel(IPhoneModelCommandService cmd, int? id, string brand, string modelName)
    {
        _cmd = cmd;
        Id = id;
        Title = id is null ? "Nuevo modelo" : "Editar modelo";
        Brand = brand;
        ModelName = modelName;
    }

    [RelayCommand]
    public async Task SaveAsync()
    {
        ErrorMessage = null;

        try
        {
            if (Id is null)
            {
                await _cmd.CreateAsync(new UpsertPhoneModelRequest
                {
                    Brand = Brand,
                    ModelName = ModelName
                });
            }
            else
            {
                await _cmd.UpdateAsync(new UpsertPhoneModelRequest
                {
                    Id = Id,
                    Brand = Brand,
                    ModelName = ModelName
                });
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }
}

