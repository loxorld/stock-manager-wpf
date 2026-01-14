using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace StockManager.Infrastructure.Persistence;

public class StockDbContextFactory : IDesignTimeDbContextFactory<StockDbContext>
{
    public StockDbContext CreateDbContext(string[] args)
    {
        var dbPath = DbPaths.GetDbPath();

        var options = new DbContextOptionsBuilder<StockDbContext>()
            .UseSqlite($"Data Source={dbPath}")
            .Options;

        return new StockDbContext(options);
    }
}

