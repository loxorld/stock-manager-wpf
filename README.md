# StockManager

Aplicacion de escritorio WPF para gestionar inventario, movimientos de stock y metricas de ventas para los locales Planeta Celular y Planeta Movil. La aplicacion persiste los datos localmente con SQLite.

## Funcionalidades principales

- Gestion de SKU: alta, edicion, baja, estado activo, costo, precio y stock.
- Filtros y busqueda: por categoria, estado, stock bajo y texto libre.
- Movimientos de stock: compras, ventas, ajustes y mermas, con nota y medio de pago.
- Acciones rapidas: compra y venta rapida desde el detalle del item.
- Dashboard de ventas: ingresos, ventas por dia, top por unidades e ingresos y comparativas con el periodo anterior.
- Historial de movimientos por SKU.

## Stack tecnologico

- WPF + MaterialDesignInXaml
- MVVM con CommunityToolkit.Mvvm
- EF Core + SQLite
- .NET 10.0 para Windows

## Estructura del proyecto

- `StockManager/`: UI WPF (Views, ViewModels y Converters)
- `StockManager.Application/`: DTOs e interfaces de servicios
- `StockManager.Domain/`: entidades y enums de dominio
- `StockManager.Infrastructure/`: EF Core, SQLite, servicios y migraciones

## Base de datos

La base de datos se guarda localmente en:

`%LOCALAPPDATA%\StockManager\stock.db`

La ruta esta centralizada en `StockManager.Infrastructure.Persistence.DbPaths`.

## Compilar y ejecutar

Requiere Windows y .NET 10 instalado.

```powershell
dotnet restore
dotnet build StockManager.slnx
dotnet run --project StockManager/StockManager.csproj
```

## Notas de dominio

- Categorias: `Case`, `ScreenProtector`, `Accessory`
- Tipos de movimiento: `PurchaseEntry`, `Sale`, `Adjustment`, `Shrinkage`
- El dashboard usa la zona horaria `Argentina Standard Time` para los rangos diarios
