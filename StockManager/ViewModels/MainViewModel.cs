using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace StockManager.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string titulo = "Stock Manager";

    [ObservableProperty]
    private int contador = 0;

    [RelayCommand]
    private void Incrementar() => Contador++;
}


