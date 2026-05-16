using Dilettante.Services;
using Dilettante.Models;
using Dilettante.Data;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.Windows.Controls;

namespace Dilettante.Pages
{
    public partial class GameDetailPage : Page
    {
        private readonly SteamService _steamService;
        private readonly SteamSearchItem? _searchItem;
        private readonly Game? _existingGame;
        private SteamGameDetail? _gameDetail;
        private int? _savedGameId = null;
        private bool _isEditMode => _existingGame != null;

        // Add mode constructor
        public GameDetailPage(SteamSearchItem steamSearchItem)
        {
            InitializeComponent();
            _steamService = new SteamService();
            _searchItem = steamSearchItem;
            Loaded += OnPageLoaded;
        }

        // Edit mode constructor
        public GameDetailPage(Game existingGame)
        {
            InitializeComponent();
            _steamService = new SteamService();
            _existingGame = existingGame;
            _savedGameId = existingGame.Id;
            Loaded += OnPageLoaded;
        }

        private async void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            int appId = _isEditMode
                ? int.Parse(_existingGame!.SteamAppId)
                : _searchItem!.id;

            _gameDetail = await _steamService.GetGameDetailsAsync(appId);
            if (_gameDetail == null) return;

            HeaderImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(_gameDetail.HeaderImage));
            BackgroundImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(_gameDetail.Background));

            GameTitle.Text = _gameDetail.Name;
            DescriptionText.Text = _gameDetail.ShortDescription;
            GenresText.Text = string.Join(", ", _gameDetail.Genres?.Select(g => g.Description) ?? []);
            DevelopersText.Text = string.Join(", ", _gameDetail.Developers ?? []);
            MetacriticText.Text = _gameDetail.Metacritic != null
                ? $"Metacritic: {_gameDetail.Metacritic.Score}"
                : "No Metacritic score";
            ScreenshotsList.ItemsSource = _gameDetail.Screenshots;

            // Pre-fill fields if in edit mode
            if (_isEditMode)
            {
                StatusCombo.SelectedIndex = (int)_existingGame!.Status;
                OwnershipCombo.SelectedIndex = (int)_existingGame.Ownership;
                ScoreBox.Text = _existingGame.Userscore?.ToString() ?? "";
                SaveButton.Content = "Update";
                AchievementsButton.IsEnabled = _existingGame.Status != GameStatus.Wishlisted;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_gameDetail == null) return;

            var selectedStatus = (GameStatus)StatusCombo.SelectedIndex;

            using var db = new AppDbContext();

            if (_isEditMode)
            {
             
                var game = db.Games.FirstOrDefault(g => g.SteamAppId == _existingGame!.SteamAppId);
                if (game == null) return;

                game.Status = selectedStatus;
                game.Ownership = (GameOwnership)OwnershipCombo.SelectedIndex;
                game.Userscore = int.TryParse(ScoreBox.Text, out int score) ? score : null;
                db.SaveChanges();

                _savedGameId = game.Id;
                SaveButton.Content = "Updated!";
            }
            else
            {
                
                var game = new Game
                {
                    SteamAppId = _gameDetail.SteamAppId.ToString(),
                    Name = _gameDetail.Name,
                    HeaderImageUrl = _gameDetail.HeaderImage,
                    IconURL = _searchItem!.TinyImage,
                    DateAdded = DateTime.Now,
                    Developers = string.Join(", ", _gameDetail.Developers ?? []),
                    Publishers = string.Join(", ", _gameDetail.Publishers ?? []),
                    Status = selectedStatus,
                    Ownership = (GameOwnership)OwnershipCombo.SelectedIndex,
                    Userscore = int.TryParse(ScoreBox.Text, out int score) ? score : null
                };

                db.Games.Add(game);
                db.SaveChanges();

                _savedGameId = game.Id;
                SaveButton.Content = "Saved!";
            }

            SaveButton.IsEnabled = false;
            AchievementsButton.IsEnabled = selectedStatus != GameStatus.Wishlisted;
        }

        private void AchievementsButton_Click(object sender, RoutedEventArgs e)
        {
            if (_savedGameId == null || _gameDetail == null) return;

            var window = (MainWindow)Window.GetWindow(this);
            window.MainFrame.Navigate(
                new AchievementPage(_savedGameId.Value, _gameDetail.SteamAppId, _gameDetail.Name)
            );
        }
    }
}