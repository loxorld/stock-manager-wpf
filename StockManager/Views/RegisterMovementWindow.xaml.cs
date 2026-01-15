using System;
using System.Windows;
using StockManager.Domain.Enums;
using StockManager.ViewModels;

namespace StockManager.Views;

public partial class RegisterMovementWindow : Window
{
    private RegisterMovementViewModel Vm => (RegisterMovementViewModel)DataContext;

    public RegisterMovementWindow(RegisterMovementViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;

        TypeCombo.ItemsSource = new[]
        {
            StockMovementType.PurchaseEntry,
            StockMovementType.Sale,
            StockMovementType.Shrinkage,
            StockMovementType.Adjustment
        };
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
            await Vm.SaveAsync();

            // Si el VM detectó error de negocio/validación, lo mostramos con UiError
            if (!string.IsNullOrWhiteSpace(Vm.ErrorMessage))
            {
                UiError.Show(new InvalidOperationException(Vm.ErrorMessage), "No se pudo registrar el movimiento");
                return;
            }

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            // Red de seguridad: cualquier cosa inesperada
            UiError.Show(ex, "Error inesperado");
        }
    }
}
