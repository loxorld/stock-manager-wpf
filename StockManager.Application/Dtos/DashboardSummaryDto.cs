using System;
using System.Collections.Generic;
using System.Text;

namespace StockManager.Application.Dtos;

public class DashboardSummaryDto
{
    public decimal Revenue { get; set; }          // Ingresos
    public int UnitsSold { get; set; }            // Unidades vendidas
    public int SalesCount { get; set; }           // Cantidad de ventas (movimientos Sale)
}
