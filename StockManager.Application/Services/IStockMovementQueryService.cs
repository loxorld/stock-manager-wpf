using System;
using System.Collections.Generic;
using System.Text;
using StockManager.Application.Dtos;

namespace StockManager.Application.Services;

public interface IStockMovementQueryService
{
    Task<StockMovementListItemDto?> GetLastBySkuAsync(int skuId);
    Task<List<StockMovementListItemDto>> GetBySkuAsync(int skuId);
    Task<List<int>> GetSkuIdsWithSalesBetweenAsync(DateTime fromUtc, DateTime toUtc);
}

