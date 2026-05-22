using Dilettante.Services;
using Dilettante.Models;
using Dilettante.Data;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;

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

        private BitmapImage? _preloadedBitmap;
        private SteamGameDetail? _preloadedDetail;



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

        public static async Task<GameDetailPage> CreateAsync(SteamSearchItem searchItem)
        {
            var page = new GameDetailPage(searchItem);
            await page.LoadDataAsync();
            return page;
            
        }

        public static async Task<GameDetailPage> CreateAsync(Game existingGame)
        {
            var page = new GameDetailPage(existingGame);
            await page.LoadDataAsync();
            return page;
        }

        private async Task LoadDataAsync()
        {
            int appId = _isEditMode
                ? int.Parse(_existingGame!.SteamAppId)
                : _searchItem!.id;

            _gameDetail = await _steamService.GetGameDetailsAsync(appId);
            if (_gameDetail == null) return;

            // Pre-download the bitmap
            var headerBitmap = new BitmapImage();
            headerBitmap.BeginInit();
            headerBitmap.UriSource = new Uri(_gameDetail.HeaderImage);
            headerBitmap.CacheOption = BitmapCacheOption.OnLoad;
            headerBitmap.EndInit();
           

            _preloadedBitmap = headerBitmap;
            _preloadedDetail = _gameDetail;
        }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            if (_preloadedBitmap == null || _preloadedDetail == null) return;

            HeaderImage.Source = _preloadedBitmap;
            ApplyColours(_preloadedBitmap);

            GameTitle.Text = _preloadedDetail.Name;
            DescriptionText.Text = _preloadedDetail.ShortDescription;
            GenresText.Text = string.Join(", ", _preloadedDetail.Genres?.Select(g => g.Description) ?? []);
            DevelopersText.Text = string.Join(", ", _preloadedDetail.Developers ?? []);
            MetacriticText.Text = _preloadedDetail.Metacritic != null
                ? $"Metacritic: {_preloadedDetail.Metacritic.Score}"
                : "No Metacritic score";
            ScreenshotsList.ItemsSource = _preloadedDetail.Screenshots;

            using var db = new AppDbContext();
            var game = db.Games.FirstOrDefault(g => g.SteamAppId == _preloadedDetail.SteamAppId.ToString());
            if (game != null)
            {
                StatusCombo.SelectedIndex = (int)game.Status;
                OwnershipCombo.SelectedIndex = (int)game.Ownership;
                ScoreBox.Text = game.Userscore?.ToString() ?? "";
                SaveButton.Content = "Update";
                AchievementsButton.IsEnabled = game.Status != GameStatus.Wishlisted;
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
                game.DateAdded = DateTime.Now;
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

        private void ApplyColours(BitmapSource bitmapSource)
        {
            var (dominant, accent) = ColourExtractor.Extract(bitmapSource);

            BackgroundImage.Source = bitmapSource;
            BackgroundImage.Opacity = 0;
            var fade = new DoubleAnimation(0, 1,
                new Duration(TimeSpan.FromSeconds(2.0)))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            BackgroundImage.BeginAnimation(UIElement.OpacityProperty, fade);

            Color c1 = Darken(dominant, 0.25);
            Color c2 = Darken(dominant, 0.35);
            Color c3 = Darken(dominant, 0.20);
            Color c4 = Darken(dominant, 0.30);
            Color c2Blended = BlendColours(c2, accent, 0.15);

            AnimateGradientStop(GradStop1, c1, 0.0);
            AnimateGradientStop(GradStop2, c2Blended, 0.1);
            AnimateGradientStop(GradStop3, c3, 0.2);
            AnimateGradientStop(GradStop4, c4, 0.3);

          //window
            var windowBrush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1)
            };
            windowBrush.GradientStops.Add(new GradientStop(c1, 0));
            windowBrush.GradientStops.Add(new GradientStop(c2Blended, 0.3));
            windowBrush.GradientStops.Add(new GradientStop(c3, 0.6));
            windowBrush.GradientStops.Add(new GradientStop(c4, 1));

            var window = (MainWindow)Window.GetWindow(this);
            window.AnimateBackgroundTo(c1, c2Blended, c3, c4);

            // Page itself transparent so window gradient shows through
            Background = new SolidColorBrush(Colors.Transparent);

            var accentBrush = new SolidColorBrush(accent);
            SaveButton.Background = accentBrush;
            AchievementsButton.Background = accentBrush;
        }
        private static Color Darken(Color c, double factor)
        {
            return Color.FromRgb(
                (byte)(c.R * factor),
                (byte)(c.G * factor),
                (byte)(c.B * factor)
            );
        }

        private static Color BlendColours(Color a, Color b, double t)
        {
            return Color.FromRgb(
                (byte)(a.R + (b.R - a.R) * t),
                (byte)(a.G + (b.G - a.G) * t),
                (byte)(a.B + (b.B - a.B) * t)
            );
        }

        private void AnimateGradientStop(GradientStop stop, Color targetColor, double delaySeconds)
        {
            var anim = new ColorAnimation
            {
                To = targetColor,
                Duration = TimeSpan.FromSeconds(0.8),
                BeginTime = TimeSpan.FromSeconds(delaySeconds),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            stop.BeginAnimation(GradientStop.ColorProperty, anim);
        }

    

    }
}