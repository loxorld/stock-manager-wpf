using System.Windows;

namespace StockManager.Views;

public enum UiToastLevel
{
    Info,
    Success,
    Warning,
    Error
}

public sealed record UiToastMessage(UiToastLevel Level, string Message);

public static class UiToast
{
    public static event Action<UiToastMessage>? ToastRaised;

    public static bool ShowInfo(string message) => Raise(UiToastLevel.Info, message);
    public static bool ShowSuccess(string message) => Raise(UiToastLevel.Success, message);
    public static bool ShowWarning(string message) => Raise(UiToastLevel.Warning, message);
    public static bool ShowError(string message) => Raise(UiToastLevel.Error, message);

    private static bool Raise(UiToastLevel level, string message)
    {
        var handler = ToastRaised;
        if (handler == null || string.IsNullOrWhiteSpace(message))
            return false;

        var toast = new UiToastMessage(level, message.Trim());
        var dispatcher = System.Windows.Application.Current?.Dispatcher;

        if (dispatcher == null || dispatcher.CheckAccess())
        {
            handler(toast);
            return true;
        }

        dispatcher.Invoke(() => handler(toast));
        return true;
    }
}
