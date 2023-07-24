using System.IO;
using System.Text.Json;

namespace GEM.Menu
{
    internal class Settings
    {
        public int ColorIndex { get; set; }
        public bool IsFullScreen { get; set; }  
        public int ScreenWidth { get; set; }
        public int ScreenHeight { get; set; }
        public int VolumeIndex { get; set; }
        public bool IsFpsVisible { get; set; }
        public bool IsAudioPanelVisible { get; set; }
        public bool IsOnScreenButtonsVisible { get; set; }
        public bool IsNotificationsVisible { get; set; }
        public bool IsGridVisible { get; set; }
        public bool IsBackgroundVisible { get; set; }
        public bool IsWindowVisible { get; set; }
        public bool IsSpritesVisible { get; set; }
        public bool IsBackgroundHighlighted { get; set; }
        public bool IsWindowHighlighted { get; set; }
        public bool IsSpritesHighlighted { get; set; }
        public bool[] AudioChannels { get; set; }

        public Settings()
        {
            // Default values
            ColorIndex = 0;
            IsFullScreen = false;
            ScreenWidth = 800;
            ScreenHeight = 720;
            VolumeIndex = 0;
            IsFpsVisible = false;
            IsAudioPanelVisible = false;
            IsOnScreenButtonsVisible = false;
            IsNotificationsVisible = true;
            IsGridVisible = false;
            IsBackgroundVisible = true;
            IsWindowVisible = true;
            IsSpritesVisible = true;
            IsBackgroundHighlighted = false;
            IsWindowHighlighted = false;
            IsSpritesHighlighted = false;
            AudioChannels = new bool[] { true, true, true, true };
        }

        public void SaveSettings()
        {
            string fileName = "settings.json";
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(this, options);
            File.WriteAllText(fileName, jsonString);
        }

        public Settings LoadSettings()
        {
            Settings settings;
            string fileName = "settings.json";
            if (File.Exists(fileName))
            {
                string jsonString = File.ReadAllText(fileName);
                settings = JsonSerializer.Deserialize<Settings>(jsonString)!;
            }
            else
            {
                settings = new Settings();
            }
            return settings;
        }
    }
}
