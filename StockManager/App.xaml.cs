using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StockManager.Application.Services;
using StockManager.Infrastructure.Persistence;
using StockManager.Infrastructure.Services;
using StockManager.ViewModels;
using StockManager.Views;

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
                services.AddTransient<IDashboardQueryService, DashboardQueryService>();

                // Dashboard
                services.AddTransient<DashboardViewModel>();
                services.AddTransient<DashboardWindow>();

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

                // Crea DB / aplica migrations
                await db.Database.MigrateAsync();

                // (Opcional beta) Seed solo si está vacío:
                // await SeedIfEmptyAsync(db);
            }

            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }
        catch (Exception ex)
        {
            UiError.Show(ex, "No se pudo iniciar la aplicación");
            Shutdown();
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await _host.StopAsync();
        base.OnExit(e);
    }

    // =========================================================
    // OPCIONAL: Seed manual SOLO si no hay datos.
    // Si lo activo, BORRÁ el HasData del DbContext (ver más abajo).
    // =========================================================
    /*
    private static async Task SeedIfEmptyAsync(StockDbContext db)
    {
        if (await db.Skus.AnyAsync()) return;

        db.Skus.AddRange(new[]
        {
            new Domain.Entities.Sku
            {
                Category = Domain.Enums.ProductCategory.Case,
                Name = "Funda silicona Samsung A02",
                CaseType = Domain.Enums.CaseType.Silicone,
                Stock = 10,
                Cost = 1500m,
                Price = 3000m,
                Active = true
            },
            new Domain.Entities.Sku
            {
                Category = Domain.Enums.ProductCategory.Case,
                Name = "Funda transparente Samsung A20",
                CaseType = Domain.Enums.CaseType.Transparent,
                Stock = 7,
                Cost = 1400m,
                Price = 2800m,
                Active = true
            },
            new Domain.Entities.Sku
            {
                Category = Domain.Enums.ProductCategory.ScreenProtector,
                Name = "Templado reforzado Samsung A02",
                ProtectorType = Domain.Enums.ProtectorType.Reinforced,
                Stock = 12,
                Cost = 1200m,
                Price = 2500m,
                Active = true
            },
            new Domain.Entities.Sku
            {
                Category = Domain.Enums.ProductCategory.ScreenProtector,
                Name = "Templado anti-espía Samsung A20",
                ProtectorType = Domain.Enums.ProtectorType.Privacy,
                Stock = 5,
                Cost = 1800m,
                Price = 3500m,
                Active = true
            },
            new Domain.Entities.Sku
            {
                Category = Domain.Enums.ProductCategory.Accessory,
                Name = "Cargador 20W USB-C",
                Stock = 6,
                Cost = 4000m,
                Price = 7500m,
                Active = true
            }
        });

        await db.SaveChangesAsync();
    }
    */
}
