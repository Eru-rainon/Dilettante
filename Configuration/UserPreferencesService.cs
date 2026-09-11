using Newtonsoft.Json;
using System.IO;

namespace Dilettante.Configuration
{
    static class UserPreferencesService
    {
        private static readonly string _path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "userprefs.json");

        private static UserPreferences _current = Load();
        public static UserPreferences Current => _current;


        private static UserPreferences Load()
        {
            try
            {
                if (File.Exists(_path))
                {
                    var json = File.ReadAllText(_path);
                    return JsonConvert.DeserializeObject<UserPreferences>(json)
                           ?? new UserPreferences();
                }
            }
            catch
            {
                // If file is corrupt or unreadable, start fresh
            }
            return new UserPreferences();
        }

        public static void Save()
        {
            try
            {
                var json = JsonConvert.SerializeObject(_current, Formatting.Indented);
                File.WriteAllText(_path, json);
            }
            catch
            {
                // Silently fail — preferences not saving shouldn't crash the app
            }
        }

    }

}

    

    
