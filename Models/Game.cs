

namespace Dilettante.Models
{
    public class Game
    {
        public int Id{  get; set; }
        public string SteamAppId { get; set; }
        public string Name {  get; set; }
        public string? Developers { get; set; }
        public string? Publishers { get; set; }
        public string HeaderImageUrl { get; set; }
        public string IconURL {  get; set; }
        public DateTime DateAdded { get; set; }

        public GameStatus Status { get; set; }
        public GameOwnership Ownership { get; set; }
        public int? Userscore { get; set; }
        public List<Achievement> Achievements { get; set; } = new();
        
    }
}
