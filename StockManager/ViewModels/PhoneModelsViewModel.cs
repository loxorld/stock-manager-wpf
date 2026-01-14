using System;
using System.Collections.Generic;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StockManager.Application.Dtos;
using StockManager.Application.Services;
using System.Collections.ObjectModel;

namespace StockManager.ViewModels;

public partial class PhoneModelsViewModel : ObservableObject
{
    private readonly IPhoneModelQueryService _query;

    public ObservableCollection<PhoneModelListItemDto> Items { get; } = new();

    [ObservableProperty] private PhoneModelListItemDto? selectedItem;
    [ObservableProperty] private string searchText = "";
    [ObservableProperty] private bool isLoading;

    public PhoneModelsViewModel(IPhoneModelQueryService query)
    {
        _query = query;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            var data = await _query.GetAllForAdminAsync(SearchText);
            Items.Clear();
            foreach (var x in data) Items.Add(x);
        }
        finally
        {
            IsLoading = false;
        }
    }
}
