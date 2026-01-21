using System.Windows;
using StockManager.Domain.Enums;

namespace StockManager.Views;

public partial class PaymentMethodWindow : Window
{
    public PaymentMethod? SelectedPaymentMethod { get; private set; }

    public PaymentMethodWindow()
    {
        InitializeComponent();
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
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

        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
