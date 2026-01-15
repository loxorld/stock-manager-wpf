using System;
using System.Collections.Generic;
using System.Text;
using StockManager.Application.Dtos;

namespace StockManager.Application.Services;

public interface IPhoneModelCommandService
{
    Task<int> CreateAsync(UpsertPhoneModelRequest request);
    Task UpdateAsync(UpsertPhoneModelRequest request);

    Task DeleteAsync(int id);

}

