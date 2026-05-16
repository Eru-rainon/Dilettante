using Microsoft.EntityFrameworkCore.Metadata;
using Newtonsoft.Json;

namespace Dilettante.Services
{

    public class SteamAppDetailsResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("data")]
        public SteamGameDetail Data { get; set;}
    }
    public class SteamGameDetail
    {
        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("steam_appid")] public int SteamAppId { get; set; }

        [JsonProperty("short_description")] public string ShortDescription { get; set; }

        [JsonProperty("header_image")] public string HeaderImage { get; set; }

        [JsonProperty("background")] public string Background { get; set; }

        [JsonProperty("metacritic")] public SteamMetacritic Metacritic { get; set; }

        [JsonProperty("genres")] public List<SteamGenre> Genres { get; set; }

        [JsonProperty("screenshots")] public List<SteamScreenshot> Screenshots { get; set; }

        [JsonProperty("developers")] public List<String> Developers { get; set; }
        [JsonProperty("publishers")] public List<String> Publishers { get; set; }

    }

    public class SteamMetacritic
    {
        [JsonProperty("score")]
        public int Score { get; set; }
    }

    public class SteamGenre 
    {
        [JsonProperty("description")]
        public String Description { get; set; }
    }

    public class SteamScreenshot
    {
        [JsonProperty("path_thumbnail")]
        public string PathThumbnail { get; set; }

        [JsonProperty("path_full")]
        public string PathFull { get; set; }
    }

}
