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
using StockManager.ViewModels;

namespace StockManager.Views;

public partial class MovementHistoryWindow : Window
{
    private MovementHistoryViewModel Vm => (MovementHistoryViewModel)DataContext;

    public MovementHistoryWindow(MovementHistoryViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
        Loaded += async (_, __) => await Vm.LoadAsync();
    }
}

