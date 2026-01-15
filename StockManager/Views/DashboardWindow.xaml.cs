using System;
using System.Windows;
using StockManager.ViewModels;

namespace StockManager.Views;

public partial class DashboardWindow : Window
{
    private readonly DashboardViewModel _vm;

    public DashboardWindow(DashboardViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = vm;

        Loaded += async (_, __) =>
        {
            try
            {
                await _vm.LoadAsync();
            }
            catch (Exception ex)
            {
                UiError.Show(ex, "No se pudo cargar el dashboard");
            }
        };
    }

    private void Today_Click(object sender, RoutedEventArgs e) => _vm.SelectedPeriod = DashboardPeriod.Today;
    private void Week_Click(object sender, RoutedEventArgs e) => _vm.SelectedPeriod = DashboardPeriod.Week;
    private void Month_Click(object sender, RoutedEventArgs e) => _vm.SelectedPeriod = DashboardPeriod.Month;

}
