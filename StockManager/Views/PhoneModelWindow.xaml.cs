using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
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

        Loaded += async (_, __) => await Vm.LoadAsync();
    }

    private void New_Click(object sender, RoutedEventArgs e)
    {
        var cmd = _sp.GetRequiredService<IPhoneModelCommandService>();
        var vm = new PhoneModelEditorViewModel(cmd, id: null, brand: "", modelName: "");

        var win = new PhoneModelEditorWindow(vm) { Owner = this };
        var ok = win.ShowDialog();
        if (ok == true)
            _ = Vm.LoadAsync();
    }

    private void Edit_Click(object sender, RoutedEventArgs e)
    {
        if (Vm.SelectedItem == null)
        {
            MessageBox.Show("Seleccioná un modelo primero.", "Atención",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var cmd = _sp.GetRequiredService<IPhoneModelCommandService>();
        var vm = new PhoneModelEditorViewModel(cmd,
            id: Vm.SelectedItem.Id,
            brand: Vm.SelectedItem.Brand,
            modelName: Vm.SelectedItem.ModelName);

        var win = new PhoneModelEditorWindow(vm) { Owner = this };
        var ok = win.ShowDialog();
        if (ok == true)
            _ = Vm.LoadAsync();
    }
}

