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

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await Vm.ApplyAsync();

            if (!string.IsNullOrWhiteSpace(Vm.ErrorMessage))
            {
                UiError.Show(new InvalidOperationException(Vm.ErrorMessage), "No se pudo actualizar el precio");
                return;
            }

            MessageBox.Show(
                $"Se actualizaron {Vm.UpdatedCount} ítems.",
                "Precios actualizados",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            UiError.Show(ex, "Error inesperado");
        }
    }
}