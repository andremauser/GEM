using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
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
        public bool IsMenuIconVisible { get; set; }
        public bool IsFpsVisible { get; set; }
        public bool IsAudioPanelVisible { get; set; }
        public bool IsOnScreenButtonsVisible { get; set; }
        public bool IsNotificationsVisible { get; set; }
        public bool IsGridVisible { get; set; }
        public bool IsBackgroundLayerVisible { get; set; }
        public bool IsWindowLayerVisible { get; set; }
        public bool IsSpritesLayerVisible { get; set; }
        public bool IsBackgroundLayerHighlighted { get; set; }
        public bool IsWindowLayerHighlighted { get; set; }
        public bool IsSpritesLayerHighlighted { get; set; }
        public bool[] AudioChannels { get; set; }
        public Dictionary<string, Buttons> ButtonBindings { get; set; }
        public Dictionary<string, Keys> KeyBindings { get; set; }

        public Settings()
        {
            // Default values
            ColorIndex = 0;
            IsFullScreen = false;
            ScreenWidth = 800;
            ScreenHeight = 720;
            VolumeIndex = 0;
            IsMenuIconVisible = true;
            IsFpsVisible = false;
            IsAudioPanelVisible = false;
            IsOnScreenButtonsVisible = false;
            IsNotificationsVisible = true;
            IsGridVisible = false;
            IsBackgroundLayerVisible = true;
            IsWindowLayerVisible = true;
            IsSpritesLayerVisible = true;
            IsBackgroundLayerHighlighted = false;
            IsWindowLayerHighlighted = false;
            IsSpritesLayerHighlighted = false;
            AudioChannels = new bool[] { true, true, true, true };
            ResetButtonBindings();
            ResetKeyBindings();
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

        public void ResetKeyBindings()
        {
            KeyBindings = new Dictionary<string, Keys>()
            {
                { "Up",     Keys.Up },
                { "Down",   Keys.Down },
                { "Left",   Keys.Left },
                { "Right",  Keys.Right },
                { "A",      Keys.X },
                { "B",      Keys.Y },
                { "Start",  Keys.Enter },
                { "Select", Keys.Back },
                { "Accept", Keys.X },
                { "Back",   Keys.Y },
                { "Menu",   Keys.LeftControl },
                { "FPS",    Keys.RightControl }
            };
        }
        public void ResetButtonBindings()
        {
            ButtonBindings = new Dictionary<string, Buttons>
            {
                { "Up",     Buttons.DPadUp },
                { "Down",   Buttons.DPadDown },
                { "Left",   Buttons.DPadLeft },
                { "Right",  Buttons.DPadRight },
                { "A",      Buttons.B },
                { "B",      Buttons.A },
                { "Start",  Buttons.Start },
                { "Select", Buttons.Back },
                { "Accept", Buttons.A },
                { "Back",   Buttons.B },
                { "Menu",   Buttons.LeftShoulder },
                { "FPS",    Buttons.RightShoulder }
            };
        }
    }
}
