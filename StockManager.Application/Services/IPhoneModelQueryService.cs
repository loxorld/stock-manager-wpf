using System;
using System.Collections.Generic;
using System.Text;
using StockManager.Application.Dtos;

namespace StockManager.Application.Services;

public interface IPhoneModelQueryService
{
    Task<List<PhoneModelDto>> GetAllAsync();
    Task<List<PhoneModelListItemDto>> GetAllForAdminAsync(string? search = null);
}

