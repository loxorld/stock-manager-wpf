# StockManager
Aplicación de escritorio WPF para gestionar inventario (SKU) para los locales Planeta Celular y Planeta Movil, movimientos de stock y métricas de ventas. Incluye un dashboard con ventas por período y ranking de productos, y persiste datos localmente con SQLite.

✨ Funcionalidades principales
Gestión de SKU: alta/edición/baja de ítems con categoría, costo, precio, stock y estado activo.

Filtros y búsqueda: por categoría, estado, stock bajo y búsqueda por nombre.

Movimientos de stock: compras, ventas, ajustes y mermas, con notas y métodos de pago.

Acciones rápidas: compra/venta rápida (+1/-1) desde el detalle.

Dashboard de ventas: ingresos, ventas por día, top por unidades/ingresos y comparativas con período anterior.

Historial de movimientos por SKU.

🧱 Stack tecnológico
WPF + MaterialDesignInXaml

MVVM con CommunityToolkit.Mvvm

EF Core + SQLite

.NET 10.0 (Windows)

🗂️ Estructura del proyecto
StockManager.slnx
├─ StockManager/                 # UI WPF (Views, ViewModels, Converters)
├─ StockManager.Application/     # DTOs e interfaces de servicios
├─ StockManager.Domain/          # Entidades y enums de dominio
└─ StockManager.Infrastructure/  # EF Core, SQLite, servicios y migraciones
🗄️ Base de datos
La aplicación guarda los datos localmente en:

%LOCALAPPDATA%\StockManager\stock.db
El acceso a la base está centralizado en StockManager.Infrastructure.Persistence.DbPaths.

▶️ Cómo compilar y ejecutar
Requiere Windows y .NET 10 instalado.

dotnet restore
dotnet build StockManager.slnx
dotnet run --project StockManager/StockManager.csproj
🧭 Notas de dominio
Categorías: Case, ScreenProtector, Accessory.

Tipos de movimiento: PurchaseEntry, Sale, Adjustment, Shrinkage.

El dashboard usa zona horaria Argentina Standard Time para rangos diarios.

