using Dilettante.Pages;
using Dilettante.Data;
using Dilettante.Models;
using Dilettante.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Navigation;

namespace Dilettante
{
    public partial class MainWindow : Window
    {
        private readonly SteamService _steamService;
        private DispatcherTimer _debounceTimer;
        public MainWindow()
        {
            InitializeComponent();
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
            UpdateTag();


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
        private void SearchResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SearchResults.SelectedItem is SteamSearchItem selected)
            {
                SearchPopup.IsOpen = false;
                SearchBox.Text = string.Empty;
                MainFrame.Navigate(new GameDetailPage(selected));
            }
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Small delay so click on result registers before popup closes
            Task.Delay(150).ContinueWith(_ =>
                Dispatcher.Invoke(() => SearchPopup.IsOpen = false));
        }

        private void UpdateTag()
        {
            using var db = new AppDbContext();
            var games = db.Games;
            int completedCount = games.Count(g => g.Status == GameStatus.Completed);

            int total = games.Count();
            int CompletionPercentage = total == 0 ? 0 : completedCount * 100 / total;
            if (CompletionPercentage < 30)
            {
                ProfileTag.Text = "Incepta Sine Fine";
            }else if (CompletionPercentage > 70)
            {
                ProfileTag.Text = "The Completionist";
            }
            else
            {
                ProfileTag.Text = "The Half Blood Prince";
            }
        }

        private void Click_LibraryButton(object sender, RoutedEventArgs e)
        {
            var window = (MainWindow)Window.GetWindow(this);
            window.MainFrame.Navigate(
                 new LibraryPage()
             );
        }

        private void UpdateNavButtons(object sender, NavigationEventArgs e)
        {
            LibraryButton.IsEnabled = MainFrame.Content is not LibraryPage;
            LibraryButton.Content = MainFrame.Content is LibraryPage ? "In Library" : "Your Library";
            BackButton.IsEnabled = MainFrame.CanGoBack;
            ForwardButton.IsEnabled = MainFrame.CanGoForward;
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



    }
}