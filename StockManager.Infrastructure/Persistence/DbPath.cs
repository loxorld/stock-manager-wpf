using System;
using System.Collections.Generic;
using System.Text;

namespace StockManager.Infrastructure.Persistence;

public static class DbPaths
{
    public static string GetDbPath()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "StockManager"
        );

        Directory.CreateDirectory(folder);
        return Path.Combine(folder, "stockmanager.db");
    }
}

