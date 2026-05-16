using Newtonsoft.Json;

namespace Dilettante.Services
{
    public class SteamSearchResponse {
        [JsonProperty("items")]
        public List<SteamSearchItem> items {  get; set; }
    }

    public class SteamSearchItem
    {
        [JsonProperty("id")]
        public int id {  get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("tiny_image")]
        public string TinyImage {  get; set; }
        
    }

}
