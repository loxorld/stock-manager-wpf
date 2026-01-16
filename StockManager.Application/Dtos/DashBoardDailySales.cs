using System;
using System.Collections.Generic;
using System.Text;

namespace StockManager.Application.Dtos;

public class DashboardDailySalesDto
{
    public DateTime Date { get; set; }
    public decimal Revenue { get; set; }
    public int Units { get; set; }
}