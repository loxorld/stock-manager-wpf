using System;
using System.Collections.Generic;
using System.Text;
using StockManager.Application.Dtos;
using StockManager.Domain.Enums;

namespace StockManager.Application.Services;

public interface ISkuQueryService
{
    Task<List<SkuListItemDto>> GetAllAsync(
        string? searchText = null,
        ProductCategory? category = null,
        bool? active = null,
        int? stockMax = null
    );

    Task<SkuDetailDto?> GetByIdAsync(int id);
}

