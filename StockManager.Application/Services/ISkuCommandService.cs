using StockManager.Application.Dtos;
using StockManager.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;


namespace StockManager.Application.Services;

public interface ISkuCommandService
{
    Task<int> CreateAsync(UpsertSkuRequest request);
    Task UpdateAsync(UpsertSkuRequest request);
    Task DeleteAsync(int id);
    Task<int> UpdatePricingByTypeAsync(
         ProductCategory category,
         CaseType? caseType,
         ProtectorType? protectorType,
         decimal? price,
         decimal? cost);
}

