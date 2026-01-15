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

            // Backup en la MISMA carpeta que la DB
            // dbPath = ...\AppData\Local\StockManager\stockmanager.db
            // backupDir = ...\AppData\Local\StockManager\Backups
            var dbDir = Path.GetDirectoryName(dbPath);
            if (string.IsNullOrWhiteSpace(dbDir))
                return;

            var backupDir = Path.Combine(dbDir, "Backups");
            Directory.CreateDirectory(backupDir);

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupPath = Path.Combine(backupDir, $"stockmanager_{timestamp}.db");

            // Si por alguna razón se llama 2 veces en el mismo segundo, evitamos choque
            if (File.Exists(backupPath))
                return;

            File.Copy(dbPath, backupPath, overwrite: false);

            // Limpieza: dejar solo últimos 30 backups
            var files = new DirectoryInfo(backupDir)
                .GetFiles("stockmanager_*.db")
                .OrderByDescending(f => f.CreationTimeUtc)
                .Skip(30);

            foreach (var f in files)
                f.Delete();
        }
        catch
        {
            
        }
    }
}
