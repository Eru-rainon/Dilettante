using System.Windows;

namespace Dilettante.Views
{
    public partial class ConfirmDialog : Window
    {
        public bool Confirmed { get; private set; } = false;

        public ConfirmDialog(string message, Window owner, String yescontent, String Nocontent)
        {
            InitializeComponent();
            MessageText.Text = message;
            ConfirmButton.Content = yescontent;
            CancelButton.Content = Nocontent;
            Owner = owner;
        }

        public ConfirmDialog(string message, Window owner, String yescontent)
        {
            InitializeComponent();
            MessageText.Text = message;
            ConfirmButton.Content = yescontent;
            CancelButton.IsEnabled = false;
            Owner = owner;
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            Confirmed = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Confirmed = false;
            Close();
        }
    }
}