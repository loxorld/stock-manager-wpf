using System.Windows;
using StockManager.Domain.Enums;

namespace StockManager.Views;

public partial class CaseStockKindWindow : Window
{
    public CaseStockKind? SelectedCaseStockKind { get; private set; }
    public PaymentMethod? SelectedPaymentMethod { get; private set; }
    public bool ShowPaymentMethodSelection { get; }

    public CaseStockKindWindow(bool showPaymentMethodSelection = false)
    {
        ShowPaymentMethodSelection = showPaymentMethodSelection;
        InitializeComponent();
        PaymentMethodPanel.Visibility = ShowPaymentMethodSelection
            ? Visibility.Visible
            : Visibility.Collapsed;
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

        if (ShowPaymentMethodSelection)
        {
            if (CashRadio.IsChecked == true)
            {
                SelectedPaymentMethod = PaymentMethod.Cash;
            }
            else if (MercadoPagoRadio.IsChecked == true)
            {
                SelectedPaymentMethod = PaymentMethod.MercadoPago;
            }
            else
            {
                MessageBox.Show(
                    "Seleccioná el medio de pago para continuar.",
                    "Medio de pago",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }
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