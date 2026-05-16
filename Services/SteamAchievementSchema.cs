using Newtonsoft.Json;
namespace Dilettante.Services
{
    public class SteamSchemaResponse
    {
        [JsonProperty("game")]  public SteamSchemaGame Game { get; set; }
    }

    public class SteamSchemaGame
    {
        [JsonProperty("availableGameStats")]  public SteamAvailableStats AvailableGameStats { get; set; }
    }
    public class SteamAvailableStats
    {
        [JsonProperty("achievements")]  public List<SteamAchievementSchema> Achievements { get; set; }
    }

    public class SteamAchievementSchema
    {
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("displayName")] public string DisplayName { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("icon")] public string Icon { get; set; }
        [JsonProperty("hidden")] public int Hidden { get; set; }

    }
}
