using System;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace StockManager.ViewModels;

public sealed class Debouncer
{
    private readonly DispatcherTimer _timer;
    private Func<Task>? _action;

    public Debouncer()
    {
        _timer = new DispatcherTimer();
        _timer.Tick += async (_, __) =>
        {
            _timer.Stop();
            var act = _action;
            _action = null;

            if (act == null) return;

            try
            {
                await act();
            }
            catch
            {
                
                throw;
            }
        };
    }

    public void Debounce(int milliseconds, Func<Task> action)
    {
        _action = action;
        _timer.Interval = TimeSpan.FromMilliseconds(milliseconds);
        _timer.Stop();
        _timer.Start();
    }
}

