using System;
using System.Collections.Generic;
using StockManager.Application.Dtos;

namespace StockManager.Application.Services;

public interface IDashboardQueryService
{
    Task<DashboardSummaryDto> GetSummaryAsync(DateTime fromUtc, DateTime toUtc);
    Task<List<DashboardTopItemDto>> GetTopByUnitsAsync(DateTime fromUtc, DateTime toUtc, int take = 5);
    Task<List<DashboardTopItemDto>> GetTopByRevenueAsync(DateTime fromUtc, DateTime toUtc, int take = 5);

    Task<List<DashboardSaleHistoryItemDto>> GetSalesHistoryAsync(DateTime fromUtc, DateTime toUtc);
    Task<List<DashboardDailySalesDto>> GetDailySalesAsync(DateTime fromUtc, DateTime toUtc);
}
