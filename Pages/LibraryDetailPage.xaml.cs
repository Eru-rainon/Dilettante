using Dilettante.Data;
using Dilettante.Models;
using Dilettante.Services;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Input;
using System.Threading.Tasks;

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
            using var db = new AppDbContext();
            var freshGame = db.Games
                .Include(g => g.Achievements)
                .FirstOrDefault(g => g.Id == _game.Id);

            if (freshGame == null) return;

            GameTitle.Text = freshGame.Name;
            DeveloperText.Text = freshGame.Developers;
            StatusText.Text = freshGame.Status.ToString();
            OwnershipText.Text = freshGame.Ownership.ToString();
            ScoreText.Text = freshGame.Userscore.HasValue ? $"{freshGame.Userscore}/10" : "Not rated";
            DateAddedText.Text = freshGame.DateAdded.ToString();
            AchievementsButton.IsEnabled = freshGame.Status != GameStatus.Wishlisted;

            int total = freshGame.Achievements?.Count ?? 0;
            int unlocked = freshGame.Achievements?.Count(a => a.IsUnlocked) ?? 0;
            AchievementsText.Text = total > 0 ? $"{unlocked} / {total}" : "Not tracked";

            if (!string.IsNullOrEmpty(freshGame.HeaderImageUrl))
            {
                var headerBitmap = new BitmapImage(new Uri(freshGame.HeaderImageUrl));
                HeaderImage.Source = headerBitmap;

                headerBitmap.DownloadCompleted += (s, args) => ApplyColours(headerBitmap);
                if (!headerBitmap.IsDownloading)
                    ApplyColours(headerBitmap);
            }

            if (int.TryParse(freshGame.SteamAppId, out int appId))
            {
                _steamDetail = await _steamService.GetGameDetailsAsync(appId);
                GenresText.Text = string.Join(", ", _steamDetail?.Genres?.Select(g => g.Description) ?? []);
            }
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            var page = await GameDetailPage.CreateAsync(_game);
            Mouse.OverrideCursor = null;

            var window = (MainWindow)Window.GetWindow(this);
            window.MainFrame.Navigate(page);
            
        }

        private void AchievementsButton_Click(object sender, RoutedEventArgs e)
        {
            if (_steamDetail == null) return;
            var window = (MainWindow)Window.GetWindow(this);
            window.MainFrame.Navigate(
                new AchievementPage(_game.Id, _steamDetail.SteamAppId, _game.Name)
            );
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {

            var window = (MainWindow)Window.GetWindow(this);

            var dialog = new Views.ConfirmDialog(
               $"Are you sure you want to remove {_game.Name} from your library?",
               window);
            dialog.ShowDialog();

            if (!dialog.Confirmed) return;

           

            using var db = new AppDbContext();
            var game = db.Games.FirstOrDefault(g => g.SteamAppId == _game.SteamAppId);
            if (game == null) return;

            db.Remove(game);
            db.SaveChanges();

            window.MainFrame.Navigate(new LibraryPage());



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
            EditButton.Background = accentBrush;
            AchievementsButton.Background = accentBrush;
            DeleteButton.Background = accentBrush;
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