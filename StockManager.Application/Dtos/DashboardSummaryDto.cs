using System;
using System.Collections.Generic;
using System.Text;

namespace StockManager.Application.Dtos;

public class DashboardSummaryDto
{
    public decimal Revenue { get; set; }          // Ingresos
    public decimal CashRevenue { get; set; }      // Ingresos en efectivo
    public decimal CardRevenue { get; set; }      // Ingresos por MercadoPago / tarjeta
    public int UnitsSold { get; set; }            // Unidades vendidas
    public int SalesCount { get; set; }           // Cantidad de ventas (movimientos Sale)
}
