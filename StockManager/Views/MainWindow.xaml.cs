using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using StockManager.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using StockManager.Views;
using StockManager.Application.Services;


namespace StockManager.Views;

public partial class MainWindow : Window
{
    private readonly IServiceProvider _sp;
    private readonly StockViewModel _vm;

    public MainWindow(IServiceProvider sp, StockViewModel vm)
    {
        InitializeComponent();
        _sp = sp;
        _vm = vm;

        DataContext = vm;
        Loaded += async (_, __) => await vm.LoadAsync();
    }

    private async void RegisterMovement_Click(object sender, RoutedEventArgs e)
    {
        if (_vm.SelectedItem == null)
        {
            MessageBox.Show("Seleccioná un ítem primero.", "Atención", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var mvm = ActivatorUtilities.CreateInstance<RegisterMovementViewModel>(
            _sp,
            _vm.SelectedItem.Id,
            _vm.SelectedItem.Name
        );

        var win = new RegisterMovementWindow(mvm)
        {
            Owner = this
        };

        var ok = win.ShowDialog();
        if (ok == true)
            await _vm.LoadAsync();
    }

    private void ViewHistory_Click(object sender, RoutedEventArgs e)
    {
        if (_vm.SelectedItem == null)
        {
            MessageBox.Show("Seleccioná un ítem primero.", "Atención", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var hvm = ActivatorUtilities.CreateInstance<MovementHistoryViewModel>(
            _sp,
            _vm.SelectedItem.Id,
            _vm.SelectedItem.Name
        );

        var win = new MovementHistoryWindow(hvm)
        {
            Owner = this
        };

        win.ShowDialog();
    }

    private async void NewSku_Click(object sender, RoutedEventArgs e)
    {
        var skuQuery = _sp.GetRequiredService<ISkuQueryService>();
        var skuCommand = _sp.GetRequiredService<ISkuCommandService>();
        var phoneModels = _sp.GetRequiredService<IPhoneModelQueryService>();

        var vm = new SkuEditorViewModel(skuQuery, skuCommand, phoneModels, id: null);
        var win = new SkuEditorWindow(vm) { Owner = this };

        var ok = win.ShowDialog();
        if (ok == true)
            await _vm.LoadAsync();
    }

    private async void EditSku_Click(object sender, RoutedEventArgs e)
    {
        if (_vm.SelectedItem == null)
        {
            MessageBox.Show("Seleccioná un ítem primero.", "Atención",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var skuQuery = _sp.GetRequiredService<ISkuQueryService>();
        var skuCommand = _sp.GetRequiredService<ISkuCommandService>();
        var phoneModels = _sp.GetRequiredService<IPhoneModelQueryService>();

        var vm = new SkuEditorViewModel(skuQuery, skuCommand, phoneModels, id: _vm.SelectedItem.Id);
        var win = new SkuEditorWindow(vm) { Owner = this };

        var ok = win.ShowDialog();
        if (ok == true)
            await _vm.LoadAsync();
    }

    private void PhoneModels_Click(object sender, RoutedEventArgs e)
    {
        var win = new PhoneModelsWindow(_sp) { Owner = this };
        win.ShowDialog();
    }

    public static readonly RoutedUICommand NewSkuCmd = new("Nuevo SKU", "NewSkuCmd", typeof(MainWindow));
    public static readonly RoutedUICommand MovementCmd = new("Registrar movimiento", "MovementCmd", typeof(MainWindow));

    private void NewSku_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        NewSku_Click(sender, e);
    }

    private void Movement_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        RegisterMovement_Click(sender, e);
    }

    private void Dashboard_Click(object sender, RoutedEventArgs e)
    {
        var win = _sp.GetRequiredService<DashboardWindow>();
        win.Owner = this;
        win.ShowDialog();
    }

}
