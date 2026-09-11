using Dilettante.Pages;
using Dilettante.Data;
using Dilettante.Models;
using Dilettante.Services;
using Dilettante.Configuration;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Navigation;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.ComponentModel;

namespace Dilettante
{
    public partial class MainWindow : Window
    {
        private readonly SteamService _steamService;
        private DispatcherTimer _debounceTimer;

        private readonly LinearGradientBrush _defaultBackground = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(1, 1),
            GradientStops = new GradientStopCollection
    {
            new GradientStop((Color)ColorConverter.ConvertFromString("#0a1628"), 0),
            new GradientStop((Color)ColorConverter.ConvertFromString("#0d2137"), 0.3),
            new GradientStop((Color)ColorConverter.ConvertFromString("#0a1628"), 0.6),
            new GradientStop((Color)ColorConverter.ConvertFromString("#0d2137"), 1),
    }
        };
        public MainWindow()
        {
            InitializeComponent();


            var prefs = UserPreferencesService.Current;
            Width = prefs.WindowWidth;
            Height = prefs.WindowHeight;
            if(prefs.WindowLeft >=0 && prefs.WindowTop >= 0)
            {
                Left = prefs.WindowLeft;
                Top = prefs.WindowTop;
                WindowStartupLocation = WindowStartupLocation.Manual;
            }



            _steamService = new SteamService();
            _debounceTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };

            _debounceTimer.Tick += async (s, e) =>
            {
                _debounceTimer.Stop();
                await PerformSearch();
            };

            MainFrame.Navigate(new LibraryPage());



        }

        private async Task PerformSearch()
        {
           var query = SearchBox.Text.Trim();
           if (string.IsNullOrEmpty(query)) return;

           var results = await _steamService.SearchGameAsync(query);
           SearchResults.ItemsSource = results;
           SearchPopup.IsOpen = results.Count > 0;

        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Restart the timer on every keystroke
            _debounceTimer.Stop();

            if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                SearchPopup.IsOpen = false;
                return;
            }

            _debounceTimer.Start();
        }
        private async void SearchResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SearchResults.SelectedItem is SteamSearchItem selected)
            {
                SearchPopup.IsOpen = false;
                SearchBox.Text = string.Empty;

                Mouse.OverrideCursor = Cursors.Wait;
                var page = await GameDetailPage.CreateAsync(selected);
                Mouse.OverrideCursor = null;

                MainFrame.Navigate(page);

            }
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Small delay so click on result registers before popup closes
            Task.Delay(150).ContinueWith(_ =>
                Dispatcher.Invoke(() => SearchPopup.IsOpen = false));
        }

     

     

        private void ClickOnDilettante(Object sender, MouseButtonEventArgs e)
        {
            var window = (MainWindow)Window.GetWindow(this);
            window.MainFrame.Navigate(
                 new LibraryPage()
             );
        }

        private void UpdateNavButtons(object sender, NavigationEventArgs e)
        {
            
            BackButton.IsEnabled = MainFrame.CanGoBack && MainFrame.Content is not LibraryPage ;
            ForwardButton.IsEnabled = MainFrame.CanGoForward;

            if (MainFrame.Content is LibraryPage || MainFrame.Content is AchievementPage)
                AnimateBackgroundTo(
                    (Color)ColorConverter.ConvertFromString("#0a1628"),
                    (Color)ColorConverter.ConvertFromString("#0d2137"),
                    (Color)ColorConverter.ConvertFromString("#0a1628"),
                    (Color)ColorConverter.ConvertFromString("#0d2137")
                );
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.CanGoBack)
            {
                MainFrame.GoBack();
            }
        }

        private void ForwardButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.CanGoForward)
            {
                MainFrame.GoForward();
            }
        }

        public void AnimateBackgroundTo(Color c1, Color c2, Color c3, Color c4)
        {
            // If current background isn't a gradient we can animate, replace it first
            if (Background is not LinearGradientBrush existingBrush ||
                existingBrush.GradientStops.Count < 4)
            {
                Background = new LinearGradientBrush
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(1, 1),
                    GradientStops = new GradientStopCollection
            {
                new GradientStop(_defaultBackground.GradientStops[0].Color, 0),
                new GradientStop(_defaultBackground.GradientStops[1].Color, 0.3),
                new GradientStop(_defaultBackground.GradientStops[0].Color, 0.6),
                new GradientStop(_defaultBackground.GradientStops[1].Color, 1),
            }
                };
            }

            var brush = (LinearGradientBrush)Background;
            AnimateStop(brush.GradientStops[0], c1, 0.0);
            AnimateStop(brush.GradientStops[1], c2, 0.1);
            AnimateStop(brush.GradientStops[2], c3, 0.2);
            AnimateStop(brush.GradientStops[3], c4, 0.3);
        }

        private void AnimateStop(GradientStop stop, Color target, double delay)
        {
            var anim = new ColorAnimation
            {
                To = target,
                Duration = TimeSpan.FromSeconds(0.8),
                BeginTime = TimeSpan.FromSeconds(delay),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            stop.BeginAnimation(GradientStop.ColorProperty, anim);
        }

        private void Click_ProfileButton(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ProfilePage());
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            var prefs = UserPreferencesService.Current;
            prefs.WindowLeft = Left;
            prefs.WindowTop = Top;
            prefs.WindowWidth = Width;
            prefs.WindowHeight = Height;
            UserPreferencesService.Save();
            base.OnClosing(e);
        }





    }
}