using System.Windows;
using StockManager.Domain.Enums;

namespace StockManager.Views;

public partial class CaseStockKindWindow : Window
{
    public CaseStockKind? SelectedCaseStockKind { get; private set; }

    public CaseStockKindWindow()
    {
        InitializeComponent();
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        if (WomenRadio.IsChecked == true)
        {
            SelectedCaseStockKind = CaseStockKind.Women;
        }
        else if (MenRadio.IsChecked == true)
        {
            SelectedCaseStockKind = CaseStockKind.Men;
        }
        else
        {
            MessageBox.Show(
                "Seleccioná una opción para continuar.",
                "Tipo de funda",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
