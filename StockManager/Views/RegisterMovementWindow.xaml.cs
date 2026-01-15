using System;
using System.Windows;
using StockManager.ViewModels;

namespace StockManager.Views;

public partial class RegisterMovementWindow : Window
{
    private RegisterMovementViewModel Vm => (RegisterMovementViewModel)DataContext;

    public RegisterMovementWindow(RegisterMovementViewModel vm)
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
            await Vm.SaveAsync();

            if (!string.IsNullOrWhiteSpace(Vm.ErrorMessage))
            {
                UiError.Show(
                    new InvalidOperationException(Vm.ErrorMessage),
                    "No se pudo registrar el movimiento"
                );
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
