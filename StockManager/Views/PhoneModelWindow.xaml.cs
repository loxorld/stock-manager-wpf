using System;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using StockManager.Application.Services;
using StockManager.ViewModels;

namespace StockManager.Views;

public partial class PhoneModelsWindow : Window
{
    private readonly IServiceProvider _sp;
    private PhoneModelsViewModel Vm => (PhoneModelsViewModel)DataContext;

    public PhoneModelsWindow(IServiceProvider sp)
    {
        InitializeComponent();
        _sp = sp;

        var q = _sp.GetRequiredService<IPhoneModelQueryService>();
        DataContext = new PhoneModelsViewModel(q);

        Loaded += async (_, __) =>
        {
            try
            {
                await Vm.LoadAsync();
            }
            catch (Exception ex)
            {
                UiError.Show(ex, "No se pudieron cargar los modelos");
            }
        };
    }

    private void New_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var cmd = _sp.GetRequiredService<IPhoneModelCommandService>();
            var vm = new PhoneModelEditorViewModel(cmd, id: null, brand: "", modelName: "");

            var win = new PhoneModelEditorWindow(vm) { Owner = this };
            var ok = win.ShowDialog();
            if (ok == true)
                _ = SafeReloadAsync();
        }
        catch (Exception ex)
        {
            UiError.Show(ex, "No se pudo abrir el alta de modelo");
        }
    }

    private void Edit_Click(object sender, RoutedEventArgs e)
    {
        if (Vm.SelectedItem == null)
        {
            MessageBox.Show("Seleccioná un modelo primero.", "Atención",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        try
        {
            var cmd = _sp.GetRequiredService<IPhoneModelCommandService>();
            var vm = new PhoneModelEditorViewModel(
                cmd,
                id: Vm.SelectedItem.Id,
                brand: Vm.SelectedItem.Brand,
                modelName: Vm.SelectedItem.ModelName
            );

            var win = new PhoneModelEditorWindow(vm) { Owner = this };
            var ok = win.ShowDialog();
            if (ok == true)
                _ = SafeReloadAsync();
        }
        catch (Exception ex)
        {
            UiError.Show(ex, "No se pudo abrir la edición de modelo");
        }
    }

    private async void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (Vm.SelectedItem == null)
        {
            MessageBox.Show("Seleccioná un modelo primero.", "Atención",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var it = Vm.SelectedItem;

        var confirm = MessageBox.Show(
            $"¿Eliminar el modelo?\n\n{it.Brand} {it.ModelName}\n\n" +
            "Si hay SKUs asociados, no se podrá eliminar.",
            "Confirmar eliminación",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning
        );

        if (confirm != MessageBoxResult.Yes)
            return;

        try
        {
            var cmd = _sp.GetRequiredService<IPhoneModelCommandService>();
            await cmd.DeleteAsync(it.Id);
            await SafeReloadAsync();
        }
        catch (Exception ex)
        {
            UiError.Show(ex, "No se pudo eliminar el modelo");
        }
    }

    private async Task SafeReloadAsync()
    {
        try
        {
            await Vm.LoadAsync();
        }
        catch (Exception ex)
        {
            UiError.Show(ex, "No se pudieron recargar los modelos");
        }
    }
}
