using Dilettante.Configuration;
using Newtonsoft.Json;
using System.Net.Http;

namespace Dilettante.Services
{
    class SteamService
    {
        private readonly HttpClient _httpClient;
        public SteamService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<List<SteamSearchItem>> SearchGameAsync(String query)
        {
            var url = $"https://store.steampowered.com/api/storesearch/?term={query}&cc=US&l=en";
            var response = await _httpClient.GetStringAsync(url);
            var result = JsonConvert.DeserializeObject<SteamSearchResponse>(response);
            return result.items ?? new List<SteamSearchItem>();

        }

        public async Task<SteamGameDetail?> GetGameDetailsAsync (int appid)
        {
            var url = $"https://store.steampowered.com/api/appdetails?appids={appid}";
            var response = await _httpClient.GetStringAsync(url);

            var wrapper = JsonConvert.DeserializeObject<Dictionary<string, SteamAppDetailsResponse>>(response);
            if (wrapper == null) return null;

            var entry = wrapper.Values.FirstOrDefault();
            return entry?.Success == true ? entry.Data : null;

        } 

        public async Task<List<SteamAchievementSchema>> GetSteamAchievementsAsync(int appid)
        {
            var key = AppConfig.SteamApiKey;
            var url = $"https://api.steampowered.com/ISteamUserStats/GetSchemaForGame/v2/?key={key}&appid={appid}&l=english";
            var response = await _httpClient.GetStringAsync(url);

            var result = JsonConvert.DeserializeObject<SteamSchemaResponse>(response);
            return result?.Game?.AvailableGameStats?.Achievements ?? new List<SteamAchievementSchema>();
        }
    }
}
