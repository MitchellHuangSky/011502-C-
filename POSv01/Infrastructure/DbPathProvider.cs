using System;
using System.Collections.Generic;
using System.Text;

namespace POSv01.Infrastructure
{
    public static class DbPathProvider
    {
        public static string GetDbPath()
        {
            var cfg = AppConfig.Current.GetSection("Database");
            var mode = (cfg["Mode"] ?? "LocalAppData").Trim();

            return mode switch
            {
                "Bin" => Path.Combine(AppContext.BaseDirectory, "pos.db"),

                "Custom" => ResolveCustomPath(cfg["CustomPath"]),

                _ => ResolveLocalAppDataPath()
            };
        }

        public static string GetImagesDir()
        {
            // 圖片跟 DB 同根：LocalAppData/POSv01/images 或 exe/images 或 CustomPath 的資料夾/images
            var dbPath = GetDbPath();
            var baseDir = Path.GetDirectoryName(dbPath) ?? AppContext.BaseDirectory;
            var imagesDir = Path.Combine(baseDir, "images");
            Directory.CreateDirectory(imagesDir);
            return imagesDir;
        }

        private static string ResolveLocalAppDataPath()
        {
            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "POSv01");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, "pos.db");
        }

        private static string ResolveCustomPath(string? customPath)
        {
            customPath = (customPath ?? "pos.db").Trim();

            if (Path.IsPathRooted(customPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(customPath)!);
                return customPath;
            }

            // 相對路徑：以 exe 旁邊為基準（發布後容易找）
            var full = Path.Combine(AppContext.BaseDirectory, customPath);
            Directory.CreateDirectory(Path.GetDirectoryName(full)!);
            return full;
        }
    }
}