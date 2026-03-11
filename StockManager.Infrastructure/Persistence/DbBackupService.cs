using System;
using System.IO;
using System.Linq;

namespace StockManager.Infrastructure.Persistence;

public static class DbBackupService
{
    public static void CreateBackup()
    {
        try
        {
            var dbPath = DbPaths.GetDbPath();
            if (!File.Exists(dbPath))
                return;

            var dbDir = Path.GetDirectoryName(dbPath);
            if (string.IsNullOrWhiteSpace(dbDir))
                return;

            var backupDir = Path.Combine(dbDir, "Backups");
            Directory.CreateDirectory(backupDir);

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupPath = Path.Combine(backupDir, $"stockmanager_{timestamp}.db");

            if (File.Exists(backupPath))
                return;

            File.Copy(dbPath, backupPath, overwrite: false);

            var files = new DirectoryInfo(backupDir)
                .GetFiles("stockmanager_*.db")
                .OrderByDescending(f => f.Name, StringComparer.Ordinal)
                .Skip(30);

            foreach (var f in files)
                f.Delete();
        }
        catch
        {
            
        }
    }
}
