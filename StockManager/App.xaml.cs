using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StockManager.Application.Services;
using StockManager.Infrastructure.Persistence;
using StockManager.Infrastructure.Services;
using StockManager.Views;
using StockManager.ViewModels;

namespace StockManager;

public partial class App : System.Windows.Application
{
    private readonly IHost _host;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                var dbPath = DbPaths.GetDbPath();

                services.AddDbContext<StockDbContext>(opt =>
                    opt.UseSqlite($"Data Source={dbPath}"));

                // Services
                services.AddTransient<ISkuQueryService, SkuQueryService>();
                services.AddTransient<ISkuCommandService, SkuCommandService>();
                
                services.AddTransient<IStockMovementService, StockMovementService>();
                services.AddTransient<IStockMovementQueryService, StockMovementQueryService>();
                services.AddTransient<StockManager.Application.Services.IDashboardQueryService, StockManager.Infrastructure.Services.DashboardQueryService>();
                services.AddTransient<StockManager.ViewModels.DashboardViewModel>();
                services.AddTransient<StockManager.Views.DashboardWindow>();


                // ViewModels
                services.AddSingleton<StockViewModel>();

                // Main window
                services.AddSingleton<MainWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        try
        {
            await _host.StartAsync();

            using (var scope = _host.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<StockDbContext>();
                await db.Database.MigrateAsync();
            }

            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }
        catch (Exception ex)
        {
            UiError.Show(ex, "No se pudo iniciar la aplicación");
            Shutdown(); // cierre controlado
        }
    }


    protected override async void OnExit(ExitEventArgs e)
    {
        await _host.StopAsync();
        base.OnExit(e);
    }
}
