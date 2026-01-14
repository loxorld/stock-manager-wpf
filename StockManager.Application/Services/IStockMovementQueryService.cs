using System;
using System.Collections.Generic;
using System.Text;
using StockManager.Application.Dtos;

namespace StockManager.Application.Services;

public interface IStockMovementQueryService
{
    Task<List<StockMovementListItemDto>> GetBySkuAsync(int skuId);
}

