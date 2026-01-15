using System;
using System.IO;

namespace StockManager.Infrastructure.Persistence;

public static class DbPaths
{
    public static string GetDbPath()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "StockManager"
        );

        Directory.CreateDirectory(folder);

        // nombre simple y consistente
        return Path.Combine(folder, "stock.db");
    }
}

