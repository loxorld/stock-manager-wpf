using System;
using System.Collections.Generic;
using System.Text;
using StockManager.Application.Dtos;

namespace StockManager.Application.Services;

public interface ISkuQueryService
{
    Task<List<SkuListItemDto>> GetAllAsync(string? search = null);
    Task<SkuDetailDto?> GetByIdAsync(int id);

}
