using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StockManager.Application.Dtos;
using StockManager.Application.Services;
using System.Collections.ObjectModel;

namespace StockManager.ViewModels;

public partial class PhoneModelsViewModel : ObservableObject
{
    private readonly IPhoneModelQueryService _query;
    private readonly Debouncer _debouncer = new();

    public ObservableCollection<PhoneModelListItemDto> Items { get; } = new();

    [ObservableProperty]
    private PhoneModelListItemDto? selectedItem;

    partial void OnSelectedItemChanged(PhoneModelListItemDto? value)
        => OnPropertyChanged(nameof(HasSelection));

    public bool HasSelection => SelectedItem != null;

    private string searchText = "";
    public string SearchText
    {
        get => searchText;
        set
        {
            if (SetProperty(ref searchText, value))
            {
                _debouncer.Debounce(300, LoadAsync);
            }
        }
    }

    [ObservableProperty]
    private bool isLoading;

    public PhoneModelsViewModel(IPhoneModelQueryService query)
    {
        _query = query;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsLoading) return;

        IsLoading = true;
        try
        {
            var data = await _query.GetAllForAdminAsync(SearchText?.Trim());

            Items.Clear();
            foreach (var x in data)
                Items.Add(x);
        }
        finally
        {
            IsLoading = false;
        }
    }
}
