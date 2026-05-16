using Dilettante.Data;
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
        public LibraryPage()
        {
            InitializeComponent();
            Loaded += OnPageLoaded;
        }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            using var db = new AppDbContext();
            var games = db.Games.Include(g => g.Achievements).ToList();
            var cards = games.Select(g => new GameCardViewModel(g)).ToList();

            GamesPanel.ItemsSource = cards;

            EmptyText.Visibility = cards.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

        }

        private void GameCard_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is GameCardViewModel card)
            {
                var window = (MainWindow)Window.GetWindow(this);
                window.MainFrame.Navigate(new LibraryDetailPage(card.game));
            }
        }
    }
}
