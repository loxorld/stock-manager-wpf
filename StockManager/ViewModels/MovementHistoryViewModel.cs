using System;
using System.Collections.Generic;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StockManager.Application.Dtos;
using StockManager.Application.Services;
using System.Collections.ObjectModel;

namespace StockManager.ViewModels;

public partial class MovementHistoryViewModel : ObservableObject
{
    private readonly IStockMovementQueryService _query;

    public int SkuId { get; }
    public string SkuName { get; }

    public ObservableCollection<StockMovementListItemDto> Items { get; } = new();

    [ObservableProperty]
    private bool isLoading;

    public MovementHistoryViewModel(IStockMovementQueryService query, int skuId, string skuName)
    {
        _query = query;
        SkuId = skuId;
        SkuName = skuName;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            var data = await _query.GetBySkuAsync(SkuId);
            Items.Clear();
            foreach (var x in data) Items.Add(x);
        }
        finally
        {
            IsLoading = false;
        }
    }
}

