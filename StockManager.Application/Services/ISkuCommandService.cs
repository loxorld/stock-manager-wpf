using System;
using System.Collections.Generic;
using System.Text;
using StockManager.Application.Dtos;

namespace StockManager.Application.Services;

public interface ISkuCommandService
{
    Task<int> CreateAsync(UpsertSkuRequest request);
    Task UpdateAsync(UpsertSkuRequest request);
    Task DeleteAsync(int id);
}

