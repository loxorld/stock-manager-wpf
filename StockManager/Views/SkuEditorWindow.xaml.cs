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
using StockManager.Domain.Enums;
using StockManager.ViewModels;


namespace StockManager.Views;

public partial class SkuEditorWindow : Window
{
    private SkuEditorViewModel Vm => (SkuEditorViewModel)DataContext;

    public SkuEditorWindow(SkuEditorViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;

        Loaded += async (_, __) => await Vm.InitializeAsync();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        await Vm.SaveAsync();
        if (string.IsNullOrWhiteSpace(Vm.ErrorMessage))
        {
            DialogResult = true;
            Close();
        }
    }
}

