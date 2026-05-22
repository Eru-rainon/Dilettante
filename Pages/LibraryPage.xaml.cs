using Dilettante.Data;
using Dilettante.Models;
using Dilettante.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.Windows.Controls;

namespace Dilettante.Pages
{
    /// <summary>
    /// Interaction logic for LibraryPage.xaml
    /// </summary>
    public partial class LibraryPage : Page
    {

        private List<GameCardViewModel> _allCards = new();
        public LibraryPage()
        {
            InitializeComponent();
            Loaded += OnPageLoaded;
        }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            using var db = new AppDbContext();
            var games = db.Games.Include(g => g.Achievements).ToList();
            _allCards = games.Select(g => new GameCardViewModel(g)).ToList();

           
            ApplyFilters();
            EmptyText.Visibility = _allCards.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

        }

        private void GameCard_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is GameCardViewModel card)
            {
                var window = (MainWindow)Window.GetWindow(this);
                window.MainFrame.Navigate(new LibraryDetailPage(card.game));
            }
        }

        private void ApplyFilters()
        {
            if (StatusFilter == null || OwnershipFilter == null || SortCombo == null || AscDescButton == null) return;

            var filtered = _allCards.AsEnumerable();

            if (StatusFilter.SelectedIndex > 0)
            {
                var status = (GameStatus)(StatusFilter.SelectedIndex-1);
                filtered = filtered.Where(c=>c.game.Status == status);
            }

            if (OwnershipFilter.SelectedIndex > 0)
            {
                var ownership = (GameOwnership)(OwnershipFilter.SelectedIndex - 1);
                filtered = filtered.Where(c => c.game.Ownership == ownership);
            }

            filtered = SortCombo.SelectedIndex switch
            {
                0 => filtered.OrderByDescending(c => c.game.DateAdded),
                1 => filtered.OrderByDescending(c => c.game.Userscore ?? -1),
                2 => filtered.OrderByDescending(c => c.CompletionPercentage),
                3 => filtered.OrderByDescending(c => c.game.Name),
                _ => filtered
            };

            if (AscDescButton.Tag?.ToString() == "asc")
                filtered = filtered.Reverse();

            GamesPanel.ItemsSource = filtered.ToList();
        }

        private void Filter_Changed(object sender, SelectionChangedEventArgs e) => ApplyFilters();

        private void AscDesc_Click(object sender, RoutedEventArgs e)
        {
            if(AscDescButton.Tag?.ToString() == "asc")
            {
                AscDescButton.Tag = "desc";
                AscDescButton.Content = "↓";
            }
            else
            {
                AscDescButton.Tag = "asc";
                AscDescButton.Content = "↑";
            }
            ApplyFilters();
        }
    }
}
