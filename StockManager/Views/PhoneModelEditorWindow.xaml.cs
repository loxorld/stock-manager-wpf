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
        await Vm.SaveAsync();
        if (string.IsNullOrWhiteSpace(Vm.ErrorMessage))
        {
            DialogResult = true;
            Close();
        }
    }
}

