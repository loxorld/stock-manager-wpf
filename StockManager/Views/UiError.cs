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
            _ => "Ocurrió un error inesperado."
        };

        MessageBox.Show(msg, title, MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}
