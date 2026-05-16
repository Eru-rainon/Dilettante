using Dilettante.Data;
using Dilettante.Models;
using Dilettante.Services;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Dilettante.Pages
{
    public partial class LibraryDetailPage : Page
    {
        private readonly Game _game;
        private readonly SteamService _steamService;
        private SteamGameDetail? _steamDetail;

        public LibraryDetailPage(Game game)
        {
            InitializeComponent();
            _game = game;
            _steamService = new SteamService();
            Loaded += OnPageLoaded;
        }

        private async void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            // Load fresh from DB with achievements
            using var db = new AppDbContext();
            var freshGame = db.Games
                .Include(g => g.Achievements)
                .FirstOrDefault(g => g.Id == _game.Id);

            if (freshGame == null) return;

            // Populate static info
            GameTitle.Text = freshGame.Name;
            DeveloperText.Text = freshGame.Developers;
            StatusText.Text = freshGame.Status.ToString();
            OwnershipText.Text = freshGame.Ownership.ToString();
            ScoreText.Text = freshGame.Userscore.HasValue ? $"{freshGame.Userscore}/10" : "Not rated";

            int total = freshGame.Achievements?.Count ?? 0;
            int unlocked = freshGame.Achievements?.Count(a => a.IsUnlocked) ?? 0;
            AchievementsText.Text = total > 0 ? $"{unlocked} / {total}" : "Not tracked";

            // Set header image from saved URL
            if (!string.IsNullOrEmpty(freshGame.HeaderImageUrl))
                HeaderImage.Source = new BitmapImage(new Uri(freshGame.HeaderImageUrl));

            // Fetch background from Steam for the blurred bg
            if (int.TryParse(freshGame.SteamAppId, out int appId))
            {
                _steamDetail = await _steamService.GetGameDetailsAsync(appId);
                if (_steamDetail?.Background != null)
                    BackgroundImage.Source = new BitmapImage(new Uri(_steamDetail.Background));

                GenresText.Text = string.Join(", ", _steamDetail?.Genres?.Select(g => g.Description) ?? []);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var window = (MainWindow)Window.GetWindow(this);
            window.MainFrame.Navigate(
                new GameDetailPage(_game)
            );
        }

        private void AchievementsButton_Click(object sender, RoutedEventArgs e)
        {
            if (_steamDetail == null) return;
            var window = (MainWindow)Window.GetWindow(this);
            window.MainFrame.Navigate(
                new AchievementPage(_game.Id, _steamDetail.SteamAppId, _game.Name)
            );
        }
    }
}