using Microsoft.Extensions.DependencyInjection;
using MaterialDesignThemes.Wpf;
using StockManager.Application.Services;
using StockManager.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StockManager.Views;

public partial class MainWindow : Window
{
    private readonly IServiceProvider _sp;
    private readonly StockViewModel _vm;
    private bool _hasAnimated;
    public SnackbarMessageQueue SnackbarMessageQueue { get; } = new(TimeSpan.FromSeconds(4));

    public static readonly RoutedUICommand NewSkuCmd = new("Nuevo SKU", "NewSkuCmd", typeof(MainWindow));
    public static readonly RoutedUICommand MovementCmd = new("Registrar movimiento", "MovementCmd", typeof(MainWindow));
    public static readonly RoutedUICommand RefreshCmd = new("Refrescar", "RefreshCmd", typeof(MainWindow));
    public static readonly RoutedUICommand DeleteSkuCmd = new("Eliminar SKU", "DeleteSkuCmd", typeof(MainWindow));
    public static readonly RoutedUICommand EditSkuCmd = new("Editar SKU", "EditSkuCmd", typeof(MainWindow));
    public static readonly RoutedUICommand FocusSearchCmd = new("Buscar", "FocusSearchCmd", typeof(MainWindow));
    public static readonly RoutedUICommand DuplicateSkuCmd = new("Duplicar SKU", "DuplicateSkuCmd", typeof(MainWindow));

    public MainWindow(IServiceProvider sp, StockViewModel vm)
    {
        InitializeComponent();
        _sp = sp;
        _vm = vm;

        DataContext = vm;
        UiToast.ToastRaised += OnToastRaised;
        Closed += (_, __) => UiToast.ToastRaised -= OnToastRaised;
        Loaded += async (_, __) =>
        {
            await vm.LoadAsync();
            ApplySortIndicators();
            RunEntranceAnimations();
        };
    }

    private async void RegisterMovement_Click(object sender, RoutedEventArgs e)
    {
        if (_vm.SelectedItem == null)
        {
            UiToast.ShowInfo("Selecciona un item primero.");
            return;
        }

        RegisterMovementViewModel movementVm;
        if (_vm.SelectedItem.CategoryValue == StockManager.Domain.Enums.ProductCategory.Case)
        {
            var caseType = _vm.SelectedItem.CaseType;
            if (caseType == null)
            {
                UiToast.ShowWarning("El SKU de funda no tiene tipo asignado.");
                return;
            }

            movementVm = ActivatorUtilities.CreateInstance<RegisterMovementViewModel>(
                _sp,
                _vm.SelectedItem.Id,
                _vm.SelectedItem.Name,
                _vm.SelectedItem.CategoryValue,
                caseType.Value
            );
        }
        else
        {
            movementVm = ActivatorUtilities.CreateInstance<RegisterMovementViewModel>(
                _sp,
                _vm.SelectedItem.Id,
                _vm.SelectedItem.Name,
                _vm.SelectedItem.CategoryValue
            );
        }

        var win = new RegisterMovementWindow(movementVm)
        {
            Owner = this
        };

        var ok = win.ShowDialog();
        if (ok == true)
        {
            await ReloadStockAsync(_vm.SelectedItem.Id);
            UiToast.ShowSuccess("Movimiento registrado.");
        }
    }

    private void ViewHistory_Click(object sender, RoutedEventArgs e)
    {
        if (_vm.SelectedItem == null)
        {
            UiToast.ShowInfo("Selecciona un item primero.");
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
        await OpenSkuEditorAsync(id: null, duplicateFromId: null);
    }

    private async void EditSku_Click(object sender, RoutedEventArgs e)
    {
        if (_vm.SelectedItem == null)
        {
            UiToast.ShowInfo("Selecciona un item primero.");
            return;
        }

        await OpenSkuEditorAsync(id: _vm.SelectedItem.Id, duplicateFromId: null);
    }

    private async void DuplicateSku_Click(object sender, RoutedEventArgs e)
    {
        if (_vm.SelectedItem == null)
        {
            UiToast.ShowInfo("Selecciona un item para duplicar.");
            return;
        }

        await OpenSkuEditorAsync(id: null, duplicateFromId: _vm.SelectedItem.Id);
    }

    private async void BulkPriceUpdate_Click(object sender, RoutedEventArgs e)
    {
        var skuCommand = _sp.GetRequiredService<ISkuCommandService>();
        var vm = new BulkPriceUpdateViewModel(skuCommand);
        var win = new BulkPriceUpdateWindow(vm) { Owner = this };

        var ok = win.ShowDialog();
        if (ok == true)
        {
            await ReloadStockAsync(_vm.SelectedItem?.Id);
            UiToast.ShowSuccess(
                string.IsNullOrWhiteSpace(vm.SuccessMessage)
                    ? "Precios actualizados."
                    : vm.SuccessMessage);
        }
    }

    private void NewSku_Executed(object sender, ExecutedRoutedEventArgs e) => NewSku_Click(sender, e);
    private void Movement_Executed(object sender, ExecutedRoutedEventArgs e) => RegisterMovement_Click(sender, e);
    private async void Refresh_Executed(object sender, ExecutedRoutedEventArgs e) => await ReloadStockAsync(_vm.SelectedItem?.Id);
    private void DeleteSku_Executed(object sender, ExecutedRoutedEventArgs e) => DeleteSku_Click(sender, e);
    private void EditSku_Executed(object sender, ExecutedRoutedEventArgs e) => EditSku_Click(sender, e);
    private void DuplicateSku_Executed(object sender, ExecutedRoutedEventArgs e) => DuplicateSku_Click(sender, e);

    private void FocusSearch_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        SearchTextBox.Focus();
        SearchTextBox.SelectAll();
    }

    private void Dashboard_Click(object sender, RoutedEventArgs e)
    {
        var win = _sp.GetRequiredService<DashboardWindow>();
        win.Owner = this;
        win.ShowDialog();
    }

    private async void DeleteSku_Click(object sender, RoutedEventArgs e)
    {
        if (_vm.SelectedItem == null)
        {
            UiToast.ShowInfo("Selecciona un SKU primero.");
            return;
        }

        var item = _vm.SelectedItem;

        var confirm = MessageBox.Show(
            $"Eliminar el SKU?\n\n{item.Name}\n\n" +
            "Solo se puede eliminar si el stock es 0 y no tiene historial.",
            "Confirmar eliminacion",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning
        );

        if (confirm != MessageBoxResult.Yes)
            return;

        try
        {
            var cmd = _sp.GetRequiredService<ISkuCommandService>();
            await cmd.DeleteAsync(item.Id);
            await ReloadStockAsync(null);
            UiToast.ShowSuccess("SKU eliminado.");
        }
        catch (Exception ex)
        {
            UiError.Show(ex, "No se pudo eliminar el SKU");
        }
    }

    private void ItemsGrid_Sorting(object sender, DataGridSortingEventArgs e)
    {
        e.Handled = true;

        var sortMemberPath = e.Column.SortMemberPath;
        if (string.IsNullOrWhiteSpace(sortMemberPath))
            return;

        _vm.ToggleSort(sortMemberPath);
        ApplySortIndicators();
    }

    private void ApplySortIndicators()
    {
        foreach (var column in ItemsGrid.Columns)
        {
            if (string.IsNullOrWhiteSpace(column.SortMemberPath))
            {
                column.SortDirection = null;
                continue;
            }

            column.SortDirection = _vm.GetSortDirection(column.SortMemberPath);
        }
    }

    private async Task OpenSkuEditorAsync(int? id, int? duplicateFromId)
    {
        var skuQuery = _sp.GetRequiredService<ISkuQueryService>();
        var skuCommand = _sp.GetRequiredService<ISkuCommandService>();

        var vm = new SkuEditorViewModel(skuQuery, skuCommand, id, duplicateFromId);
        var win = new SkuEditorWindow(vm) { Owner = this };

        var ok = win.ShowDialog();
        if (ok != true)
            return;

        await ReloadStockAsync(vm.SavedSkuId ?? _vm.SelectedItem?.Id);
        UiToast.ShowSuccess(
            id is not null
                ? "SKU actualizado."
                : duplicateFromId is not null
                    ? "SKU duplicado."
                    : "SKU creado.");
    }

    private async Task ReloadStockAsync(int? selectId)
    {
        if (selectId.HasValue)
            _vm.SelectItemOnNextLoad(selectId.Value);

        await _vm.LoadAsync();
        ApplySortIndicators();
    }

    private void OnToastRaised(UiToastMessage toast)
    {
        var prefix = toast.Level switch
        {
            UiToastLevel.Success => "Listo",
            UiToastLevel.Warning => "Aviso",
            UiToastLevel.Error => "Error",
            _ => "Info"
        };

        SnackbarMessageQueue.Enqueue($"{prefix}: {toast.Message}");
    }

    private void RunEntranceAnimations()
    {
        if (_hasAnimated)
            return;

        _hasAnimated = true;
        EntranceAnimator.AnimateSequence(
            TopBarPanel,
            FiltersCard,
            CriticalCard,
            TotalsCard,
            StockGridCard,
            DetailPanelCard);
    }
}
