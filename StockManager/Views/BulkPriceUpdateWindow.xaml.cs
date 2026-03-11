using System;
using System.Windows;
using StockManager.ViewModels;

namespace StockManager.Views;

public partial class BulkPriceUpdateWindow : Window
{
    private BulkPriceUpdateViewModel Vm => (BulkPriceUpdateViewModel)DataContext;

    public BulkPriceUpdateWindow(BulkPriceUpdateViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private async void ApplyByType_Click(object sender, RoutedEventArgs e)
        => await ExecuteAsync(Vm.ApplyByTypeAsync, "No se pudo actualizar los precios por tipo");

    private async void ApplyGlobalPercentage_Click(object sender, RoutedEventArgs e)
        => await ExecuteAsync(Vm.ApplyGlobalPercentageAsync, "No se pudo aplicar el porcentaje global");

    private async Task ExecuteAsync(Func<Task> action, string errorTitle)
    {
        try
        {
            await action();

            if (!string.IsNullOrWhiteSpace(Vm.ErrorMessage))
            {
                UiError.Show(new InvalidOperationException(Vm.ErrorMessage), errorTitle);
                return;
            }

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            UiError.Show(ex, "Error inesperado");
        }
    }
}
