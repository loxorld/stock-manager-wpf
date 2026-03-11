using System;
using System.Windows;

namespace StockManager.Views;

public static class UiError
{
    public static void Show(Exception ex, string title = "Error")
    {
        var msg = ex switch
        {
            ArgumentException => ex.Message,
            InvalidOperationException => ex.Message,
            _ => "Ocurrio un error inesperado."
        };

        if (UiToast.ShowError(msg))
            return;

        MessageBox.Show(msg, title, MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}
