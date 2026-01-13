// File: Infrastructure/AppSettings.cs
using System;
using Microsoft.Extensions.Configuration;

namespace POSv01.Infrastructure
{
    public sealed class AppSettings
    {
        public bool AllowNegativeStock { get; init; } = true;

        public static AppSettings Load()
        {
            var baseDir = AppContext.BaseDirectory;

            var config = new ConfigurationBuilder()
                .SetBasePath(baseDir)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables(prefix: "POSV01_")
                .Build();

            // 支援：
            // 1) appsettings.json: "AllowNegativeStock": true/false
            // 2) 環境變數：POSV01__AllowNegativeStock=true/false
            // 3) 環境變數：POSV01_ALLOW_NEGATIVE_STOCK=true/false
            var allow =
                ParseBool(config["AllowNegativeStock"])
                ?? ParseBool(config["AllowNegativeStock".ToUpperInvariant()]) // 以防你用全大寫 key
                ?? ParseBool(Environment.GetEnvironmentVariable("POSV01_ALLOW_NEGATIVE_STOCK"))
                ?? true;

            return new AppSettings { AllowNegativeStock = allow };
        }

        private static bool? ParseBool(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            return bool.TryParse(s.Trim(), out var b) ? b : null;
        }
    }
}
