using Dilettante.Data;
using Dilettante.Models;
using Dilettante.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Dilettante.Pages
{
    public partial class AchievementPage : Page
    {
        private readonly int _gameId;
        private readonly int _steamAppId;
        private readonly SteamService _steamService;
        private List<Achievement> _achievements = new();

        public AchievementPage(int gameId, int steamAppId, string gameName)
        {
            InitializeComponent();
            _gameId = gameId;
            _steamAppId = steamAppId;
            _steamService = new SteamService();
            PageTitle.Text = gameName;
            Loaded += OnPageLoaded;
        }

        private async void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            var schemas = await _steamService.GetSteamAchievementsAsync(_steamAppId);

            using var db = new AppDbContext();
            var savedAchievements = db.Achievements
                .Where(a => a.GameId == _gameId)
                .ToList();

            _achievements = schemas.Select(s => new Achievement
            {
                GameId = _gameId,
                ApiName = s.Name,
                DisplayName = s.DisplayName,
                Description = s.Hidden == 1 ? "Hidden" : (s.Description ?? ""),
                IconUrl = s.Icon,
                IsUnlocked = savedAchievements.FirstOrDefault(a=>a.ApiName == s.Name)?.IsUnlocked??false

            }).ToList();

            AchievementList.ItemsSource = _achievements;
            UpdateProgress();
        }

        private void UpdateProgress()
        {
            int unlocked = _achievements.Count(a => a.IsUnlocked);
            ProgressText.Text = $"{unlocked} / {_achievements.Count} achievements unlocked";
            UpdateToggleAllButton();
        }

        private void UpdateToggleAllButton()
        {
            bool allUnlocked = _achievements.All(a => a.IsUnlocked);
            ToggleAllButton.Content = allUnlocked ? "Unselect All" : "Select All";
        }

        private void SaveAchievementsButton_Click(object sender, RoutedEventArgs e)
        {
            using var db = new AppDbContext();
            

            var existing = db.Achievements
               .Where(a => a.GameId == _gameId)
               .ToList();

            if (existing.Any())
            {
                foreach(Achievement ach in existing)
                {
                    var updated = _achievements.FirstOrDefault(a => a.ApiName == ach.ApiName);
                    if (updated != null)
                        ach.IsUnlocked = updated.IsUnlocked;
                }
            }
            else
            {
                db.Achievements.AddRange(_achievements);
            }

            db.SaveChanges();
            SaveAchievementsButton.Content = "saved!";
            SaveAchievementsButton.IsEnabled = false;    
        }

        private void ToggleAchievement(Object sender, MouseButtonEventArgs e)
        {
            if(sender is FrameworkElement element && element.DataContext is Achievement achievement)
            {
                achievement.IsUnlocked = !achievement.IsUnlocked;
                UpdateProgress();
            }
        }

        private void CheckBoxToggle(object sender, RoutedEventArgs e)
        {
            UpdateProgress();
        }

        private void ToggleAll(object sender, RoutedEventArgs e)
        {
            bool allUnlocked = _achievements.All(a=>a.IsUnlocked);
            bool newValue = !allUnlocked;
            foreach(var achievement in _achievements)
            {
                achievement.IsUnlocked = newValue;
            }
            UpdateProgress();
        }
    }
}