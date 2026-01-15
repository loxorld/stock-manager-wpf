using System;
using System.Windows;
using StockManager.ViewModels;

namespace StockManager.Views;

public partial class PhoneModelEditorWindow : Window
{
    private PhoneModelEditorViewModel Vm => (PhoneModelEditorViewModel)DataContext;

    public PhoneModelEditorWindow(PhoneModelEditorViewModel vm)
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
                UiError.Show(new InvalidOperationException(Vm.ErrorMessage), "No se pudo guardar el modelo");
                return;
            }

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            // Red de seguridad: DB bloqueada, error inesperado, etc.
            UiError.Show(ex, "Error inesperado");
        }
    }
}
