using System;
using System.Collections.Generic;
using System.Text;

namespace StockManager.Application.Services;

using StockManager.Application.Dtos;

public interface IStockMovementService
{
    Task RegisterAsync(RegisterMovementRequest request);

    Task DeleteSaleAsync(long movementId);
}

