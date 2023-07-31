using Microsoft.Xna.Framework.Input;
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
        public bool IsBackgroundVisible { get; set; }
        public bool IsWindowVisible { get; set; }
        public bool IsSpritesVisible { get; set; }
        public bool IsBackgroundHighlighted { get; set; }
        public bool IsWindowHighlighted { get; set; }
        public bool IsSpritesHighlighted { get; set; }
        public bool[] AudioChannels { get; set; }
        public Buttons Up_ButtonBinding { get; set; }
        public Buttons Down_ButtonBinding { get; set; }
        public Buttons Left_ButtonBinding { get; set; }
        public Buttons Right_ButtonBinding { get; set; }
        public Buttons A_ButtonBinding { get; set; }
        public Buttons B_ButtonBinding { get; set; }
        public Buttons Start_ButtonBinding { get; set; }
        public Buttons Select_ButtonBinding { get; set; }
        public Keys Up_KeyBinding { get; set; }
        public Keys Down_KeyBinding { get; set; }
        public Keys Left_KeyBinding { get; set; }
        public Keys Right_KeyBinding { get; set; }
        public Keys A_KeyBinding { get; set; }
        public Keys B_KeyBinding { get; set; }
        public Keys Start_KeyBinding { get; set; }
        public Keys Select_KeyBinding { get; set; }

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
            IsBackgroundVisible = true;
            IsWindowVisible = true;
            IsSpritesVisible = true;
            IsBackgroundHighlighted = false;
            IsWindowHighlighted = false;
            IsSpritesHighlighted = false;
            AudioChannels = new bool[] { true, true, true, true };
            Up_ButtonBinding = Buttons.DPadUp;
            Down_ButtonBinding = Buttons.DPadDown;
            Left_ButtonBinding = Buttons.DPadLeft;
            Right_ButtonBinding = Buttons.DPadRight;
            A_ButtonBinding = Buttons.B;
            B_ButtonBinding = Buttons.A;
            Start_ButtonBinding = Buttons.Start;
            Select_ButtonBinding = Buttons.Back;
            Up_KeyBinding = Keys.Up;
            Down_KeyBinding = Keys.Down;
            Left_KeyBinding = Keys.Left;
            Right_KeyBinding = Keys.Right;
            A_KeyBinding = Keys.X;
            B_KeyBinding = Keys.Y;
            Start_KeyBinding = Keys.Enter;
            Select_KeyBinding = Keys.Back;
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
