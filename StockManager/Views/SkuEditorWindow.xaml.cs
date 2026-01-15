using System;
using System.Windows;
using StockManager.ViewModels;

namespace StockManager.Views;

public partial class SkuEditorWindow : Window
{
    private SkuEditorViewModel Vm => (SkuEditorViewModel)DataContext;

    public SkuEditorWindow(SkuEditorViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;

        Loaded += async (_, __) =>
        {
            try
            {
                await Vm.InitializeAsync();

                if (!string.IsNullOrWhiteSpace(Vm.ErrorMessage))
                    UiError.Show(new InvalidOperationException(Vm.ErrorMessage), "Atención");
            }
            catch (Exception ex)
            {
                UiError.Show(ex, "Error inesperado");
            }
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

            if (!string.IsNullOrWhiteSpace(Vm.ErrorMessage))
            {
                UiError.Show(new InvalidOperationException(Vm.ErrorMessage), "No se pudo guardar el SKU");
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
