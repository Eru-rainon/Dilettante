
namespace Dilettante.Configuration
{
    class UserPreferences
    {
        public int StatusFilterIndex { get; set; } = 0;
        public int OwnershipFilterIndex { get; set; } = 0;
        public int SortComboIndex { get; set; } = 0;
        public string SortDirection { get; set; } = "desc";
        public double WindowWidth { get; set; } = 800;
        public double WindowHeight { get; set; } = 550;
        public double WindowLeft { get; set; } = -1; 
        public double WindowTop { get; set; } = -1;
    }
}
