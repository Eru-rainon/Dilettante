
using System.ComponentModel;
namespace Dilettante.Models
{
    public class Achievement : INotifyPropertyChanged
    {
        private bool _isUnlocked;
        public int Id { get; set; }
        public string ApiName { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string IconUrl { get; set; }
        public bool IsUnlocked {
            get => _isUnlocked;
            set
            {
                if (_isUnlocked != value)
                {
                    _isUnlocked = value;
                    OnPropertyChanged(nameof(IsUnlocked));
                }
            }
        }
        public int GameId { get; set; }
        public Game Game { get; set; }


        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName)
            );
        }
    }
}
