using System;
using System.Collections.Generic;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StockManager.Application.Dtos;
using StockManager.Application.Services;
using System.Collections.ObjectModel;

namespace StockManager.ViewModels;

public partial class StockViewModel : ObservableObject
{
    private readonly ISkuQueryService _skuQueryService;
    private readonly IStockMovementService _movementService;

    public ObservableCollection<SkuListItemDto> Items { get; } = new();

    [ObservableProperty]
    private SkuListItemDto? selectedItem;

    [ObservableProperty]
    private string searchText = "";

    [ObservableProperty]
    private bool isLoading;

    public StockViewModel(ISkuQueryService skuQueryService, IStockMovementService movementService)
    {
        _skuQueryService = skuQueryService;
        _movementService = movementService;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            var data = await _skuQueryService.GetAllAsync(SearchText);
            Items.Clear();
            foreach (var x in data) Items.Add(x);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task SearchAsync()
    {
        await LoadAsync();
    }
}

