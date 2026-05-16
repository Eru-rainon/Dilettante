using Dilettante.Models;

namespace Dilettante.ViewModels
{
    class GameCardViewModel
    {
        public Game game { get; }
        public String Name => game.Name;
        public string HeaderImageUrl => game.HeaderImageUrl;
        public string AchievementProgress { get
            {
                if (game.Achievements == null || game.Achievements.Count == 0) return "_";

                int unlocked = game.Achievements.Count(a => a.IsUnlocked);
                return $"{unlocked}/{game.Achievements.Count}";

            } 
        }
        public double CompletionPercentage
        {
            get
            {
                if (game.Achievements == null || game.Achievements.Count == 0)
                    return 0;
                return (double)game.Achievements.Count(a => a.IsUnlocked) / game.Achievements.Count * 100;
            }
        }
        public string CompletionText => game.Achievements?.Count > 0
           ? $"{(int)CompletionPercentage}%"
           : "—";

        public string StatusIcon => game.Status switch
        {
            GameStatus.Completed => "✓",
            GameStatus.Abandoned => "X",
            GameStatus.Playing => "▶",
            GameStatus.Paused => "⏸",
            GameStatus.Wishlisted => "♥",
            _ => "?"
        };

        public string StatusColor => game.Status switch
        {
            GameStatus.Completed => "#4caf50",
            GameStatus.Abandoned => "#ef5350",
            GameStatus.Playing => "#2a7abf",
            GameStatus.Paused => "#ff9800",
            GameStatus.Wishlisted => "#ab47bc",
            _ => "#8899aa"
        };

        public GameCardViewModel(Game game)
        {
            this.game = game;
        }

    }
}
