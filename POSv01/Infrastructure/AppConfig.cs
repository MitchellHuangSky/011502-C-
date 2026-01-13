using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;



namespace POSv01.Infrastructure
{
    public static class AppConfig
    {
        private static IConfigurationRoot? _config;

        public static IConfigurationRoot Current =>
            _config ??= new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables(prefix: "POSV01__")
                .Build();
    }
}
