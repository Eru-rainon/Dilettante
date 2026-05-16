using Microsoft.Extensions.Configuration;

namespace Dilettante.Configuration
{
    public static class AppConfig
    {
        private static IConfiguration _config;

        static AppConfig()
        {
            _config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();
        }

        public static string SteamApiKey => _config["SteamApiKey"]!;
    }
}