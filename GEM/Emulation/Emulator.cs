using GEM.Menu;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GEM.Emulation
{
    /// <summary>
    /// Connection between user and gameboy instance: 
    /// Handles user input and draws emulated Gameboy screen to window including debug informations
    /// </summary>
    internal class Emulator
    {
        #region Fields
        static public Texture2D _Pixel;
        static public SpriteFont _Font;

        const int OPEN_ENTRIES = 12;
        const int SAVE_DELAY_MS = 500;

        Gameboy _gameboy;
        GraphicsDevice _graphicsDevice;
        SpriteBatch _spriteBatch;

        // colors
        Color[][] _emuPalette;
        string[] _emuPaletteNames;
        Color _gridColorDark;
        Color _gridColorLight;
        Color _pixelMarkerTextColor;
        Color _pixelMarkerColor;

        // styles
        Style _menuStyle;
        Style _menuButtonStyle;
        Style _onScreenStyle;
        Style _toolTipStyle;
        Style _notificationStyle;

        // events
        public delegate void PaletteChange(Color[] colors);
        public event PaletteChange OnPaletteChange;

        // states
        int _openStartIndex;

        // timespans
        double _timespanUpdate;
        double _timespanDraw;
        double _timespanEmulation;

        // buttonset base
        BaseControl _onScreenButtonsBase;

        // fields
        List<string> _romList;
        List<BaseControl> _controls;
        float[] _volumeList;
        MenuButton _menu;
        MenuButton _audioBar;
        MenuButton _toolTip;
        NotificationPanel _notifications;
        MenuButton _fps;
        int _screenWidth;
        int _screenHeight;
        int _screenLeft;
        int _screenTop;
        bool _saveRamToFile;
        DateTime _saveTime;
        Settings _settings;
        #endregion

        #region Constructors
        public Emulator(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _gameboy = new Gameboy(_graphicsDevice);
            _spriteBatch = new SpriteBatch(_graphicsDevice);
            _Pixel = new Texture2D(_graphicsDevice, 1, 1);
            _Pixel.SetData(new Color[] { Color.White });
            _controls = new List<BaseControl>();
            _romList = new List<string>();
            _settings = new Settings();
        }
        #endregion

        #region Properties
        public string CartridgeTitle
        {
            get
            {
                string title = _gameboy.CartridgeTitle;
                return title != "" ? title : "N/A";
            }
        }
        public int VolumeIndex 
        { 
            get
            {
                return _settings.VolumeIndex;
            }
            set
            {
                _settings.VolumeIndex = Math.Clamp(value, 0, _volumeList.Length - 1);
                _gameboy.SetVolume(_volumeList[_settings.VolumeIndex]);
            }
        }
        public int EmuColorIndex
        {
            get
            {
                return _settings.ColorIndex;
            }
            set
            {
                _settings.ColorIndex = Math.Clamp(value, 0, _emuPalette.Length - 1); ;
                OnPaletteChange?.Invoke(_emuPalette[_settings.ColorIndex]);
            }
        }
        #endregion

        #region Methods
        public void LoadContent(ContentManager content)
        {
            _Font = content.Load<SpriteFont>("Console");
            // volumes
            _volumeList = new float[] { 0f, 0.01f, 0.05f, 0.1f, 0.25f, 0.5f, 0.75f, 1f };
            // colors
            _emuPalette = new Color[][]
            {
                new Color[]
                {
                    // Green
                    new Color(245, 250, 239),
                    new Color(134, 194, 112),
                    new Color(47, 105, 87),
                    new Color(11, 25, 32)
                },
                new Color[]
                {
                    // Yellow
                    new Color(255, 255, 255),
                    new Color(253, 204, 102),
                    new Color(207, 106, 40),
                    new Color(7, 9, 9)
                },
                new Color[]
                {
                    // Game and Watch
                    new Color(0xef, 0xee, 0xe8),
                    new Color(0xb0, 0xb3, 0xa6),
                    new Color(0x53, 0x5b, 0x4e),
                    new Color(0x06, 0x16, 0x0f)
                },
                new Color[]
                {
                    // Zelda SGB
                    new Color(0xff, 0xff, 0xb5),
                    new Color(0x7b, 0xc6, 0x7b),
                    new Color(0x6b, 0x8c, 0x42),
                    new Color(0x5a, 0x39, 0x21)
                },
                new Color[]
                {
                    // Pokemon SGB
                    new Color(0xff, 0xef, 0xff),
                    new Color(0xf7, 0xb5, 0x8c),
                    new Color(0x84, 0x73, 0x9c),
                    new Color(0x18, 0x10, 0x10)
                },
                new Color[]
                {
                    // Megaman SGB
                    new Color(0xce, 0xce, 0xce),
                    new Color(0x6f, 0x9e, 0xdf),
                    new Color(0x42, 0x67, 0x8e),
                    new Color(0x10, 0x25, 0x33)
                },
                new Color[]
                {
                    // Standard SGB
                    new Color(0xf7, 0xe7, 0xc6),
                    new Color(0xd6, 0x8e, 0x49),
                    new Color(0xa6, 0x37, 0x25),
                    new Color(0x33, 0x1e, 0x50)
                },
                new Color[]
                {
                    // Original DMG
                    new Color(127, 134, 15),
                    new Color(87, 124, 68),
                    new Color(54, 93, 72),
                    new Color(42, 69, 59)
                },
            };
            _emuPaletteNames = new string[]
            {
                "Green",
                "Yellow",
                "Game and Watch",
                "Zelda SGB",
                "Pokemon SGB",
                "Megaman SGB",
                "Default SGB",
                "Original DMG"
            };
            _gridColorDark = new Color(0, 0, 0, 128);
            _gridColorLight = new Color(0, 0, 0, 32);
            _pixelMarkerTextColor = new Color(255, 0, 255, 255);
            _pixelMarkerColor = new Color(255, 0, 255, 255);

            // initial call to rom search - updated by click on "Open ROM"
            updateRomList();
            _openStartIndex = 0;

            // disable and re-enable background controls
            MenuButton.OnFocusChange += (o, e) => {
                // no focus
                if (!MenuButton.IsFocusSet)
                {
                    _onScreenButtonsBase.Enabled = true;
                    _audioBar.Enabled = true;
                }
                // focus set not on onscreenbuttons
                if (MenuButton.IsFocusSet && MenuButton.Focus.RootControl != _onScreenButtonsBase)
                {
                    _onScreenButtonsBase.Enabled = false;
                }
                // focus set not on audiobar
                if (MenuButton.IsFocusSet && MenuButton.Focus.RootControl != _audioBar)
                {
                    _audioBar.Enabled = false;
                }
            };

            // styles
            createStyles();

            // on screen buttons
            _onScreenButtonsBase = onScreenButtons();
            _controls.Add(_onScreenButtonsBase);

            // audio bar
            _audioBar = audioMenu();
            _audioBar.OnClose += setFocusToNull;
            _audioBar.OnDraw += (o, e) => {
                _audioBar.Top = _screenTop + _screenHeight / 2 - (7 * 60 / 2);
                _audioBar.Left = -_audioBar.Width;
            };
            _controls.Add(_audioBar);

            // fps menu
            _fps = fpsMenu();
            _controls.Add(_fps);

            // main menu
            _menu = mainMenu();
            _controls.Add(_menu);

            // tooltip
            _toolTip = toolTip();
            _controls.Add(_toolTip);

            // notifications
            _notifications = new NotificationPanel(null);
            _notifications.OnDraw += (o, e) => {
                _notifications.Left = Game1._Graphics.GraphicsDevice.Viewport.Width;
                _notifications.Top = Game1._Graphics.GraphicsDevice.Viewport.Height;
            };
            _controls.Add(_notifications);

            // set notifications
            _notifications.Push("Welcome to GEM", _notificationStyle, NotificationType.Information, 10);
            _gameboy.OnPowerOn += (o, e) => { _notifications.Push(CartridgeTitle, _notificationStyle, NotificationType.Information); };

            // save RAM event
            _saveRamToFile = false;
            Cartridge.OnRamDisable += (o, e) => { _saveRamToFile = true; _saveTime = DateTime.Now + TimeSpan.FromMilliseconds(SAVE_DELAY_MS); };
            Game1._Instance.Exiting += (o, e) => { SaveRamToFile(); _settings.SaveSettings(); };
            // resize event
            Game1._Instance.Window.ClientSizeChanged += (o, e) => { 
                _settings.ScreenWidth = Game1._Instance.Window.ClientBounds.Width;
                _settings.ScreenHeight = Game1._Instance.Window.ClientBounds.Height;
            };

            // load settings
            _settings = _settings.LoadSettings();
            EmuColorIndex = _settings.ColorIndex;
            Game1._Graphics.IsFullScreen = _settings.IsFullScreen;
            Game1._Graphics.PreferredBackBufferWidth = _settings.ScreenWidth;
            Game1._Graphics.PreferredBackBufferHeight = _settings.ScreenHeight;
            Game1._Graphics.ApplyChanges();
            VolumeIndex = _settings.VolumeIndex;
            _menu.Image.Visible = _settings.IsMenuIconVisible;
            _fps.Visible = _settings.IsFpsVisible;
            _audioBar.Panel.Visible = _settings.IsAudioPanelVisible;
            _onScreenButtonsBase.Visible = _settings.IsOnScreenButtonsVisible;
            _notifications.Visible = _settings.IsNotificationsVisible;
            _gameboy.MasterSwitch = _settings.AudioChannels;
        }

        public void Update(GameTime gameTime)
        {
            DateTime start = DateTime.Now;

            // update input
            Input.Update();

            // update controls' states
            foreach (BaseControl control in _controls)
            {
                control.Update(gameTime);
            }

            DateTime end = DateTime.Now;
            _timespanUpdate = (end - start).TotalMilliseconds;
        }

        public void Draw(Viewport viewport)
        {
            DateTime start = DateTime.Now;

            // update emulator
            _gameboy.UpdateFrame();
            DateTime afterEmulation = DateTime.Now;
            Texture2D[] screens = _gameboy.GetScreens(_emuPalette[EmuColorIndex]);

            // draw emulator
            _spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);
            drawEmulator(viewport, screens);
            _spriteBatch.End();

            // draw controls
            _spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.LinearClamp);
            foreach (BaseControl control in _controls)
            {
                control.Draw(_spriteBatch);
            }
            _spriteBatch.End();

            // save RAM to file
            if (_saveRamToFile && DateTime.Now >= _saveTime)
            {
                SaveRamToFile();
            }

            DateTime end = DateTime.Now;
            _timespanEmulation = (afterEmulation - start).TotalMilliseconds;
            _timespanDraw = (end - afterEmulation).TotalMilliseconds;
        }

        // Public Helper Methods
        public void SaveRamToFile()
        {
            _gameboy.SaveRAM();
            _notifications.Push("RAM saved", _notificationStyle, NotificationType.Success);
            _saveRamToFile = false;
        }

        // Private Helper Methods
        private void fillOpenDialog(MenuButton parent)
        {
            parent["Scroll up"].Enabled = (_openStartIndex - OPEN_ENTRIES) >= 0;
            for (int i = 0; i < OPEN_ENTRIES; i++)
            {
                int index = _openStartIndex + i;
                if (index < _romList.Count)
                {
                    parent[i.ToString()].Enabled = true;
                    parent[i.ToString()].ButtonData = index;
                    string fullName = _romList[index];
                    string fileName = fullName.Substring(fullName.LastIndexOf("/") + 1);
                    parent[i.ToString()].ToolTip = fileName;
                    parent[i.ToString()].Label.Caption = fileName.Substring(0, Math.Min(fileName.Length, 26));
                    parent[i.ToString()].Label.HorizontalAlign = Align.Left;
                }
                else
                {
                    parent[i.ToString()].Enabled = false;
                    parent[i.ToString()].Label.Caption = "";
                }
            }
            if (_romList.Count == 0)
            {
                parent["0"].Enabled = true;
                parent["0"].Label.Caption = "No ROMs found";
                parent["0"].ToolTip = "No ROM files found in /roms folder";
            }
            parent["Scroll down"].Enabled = (_openStartIndex + OPEN_ENTRIES) < _romList.Count;
        }
        private void updateRomList()
        {
#if __ANDROID__
            return;
#endif
            if (!Directory.Exists("roms/"))
                Directory.CreateDirectory("roms/");
            int i = 0;
            _romList.Clear();
            foreach (var file in Directory.EnumerateFiles("roms/"))
            {
                int dotPos = file.LastIndexOf('.');
                string fileExt = file.Substring(dotPos, file.Length - dotPos);
                if (fileExt == ".gb" || fileExt == ".gbc")
                {
                    i++;
                    _romList.Add(file);
                }
            }
            _openStartIndex = 0;
        }
        private void createStyles()
        {
            _menuStyle = new Style(this);
            _menuStyle.SetColor(Element.Background, State.Idle, 3, 0.8f);
            _menuStyle.SetColor(Element.Background, State.Hover, 2);
            _menuStyle.SetColor(Element.Background, State.Press, 1);
            _menuStyle.SetColor(Element.Background, State.Disabled, 3, 0.8f);
            _menuStyle.SetColor(Element.Foreground, State.Idle, Color.White);
            _menuStyle.SetColor(Element.Foreground, State.Hover, Color.White);
            _menuStyle.SetColor(Element.Foreground, State.Press, Color.White);
            _menuStyle.SetColor(Element.Foreground, State.Disabled, Color.White);
            _menuStyle.SetColor(Element.Border, State.Idle, Color.Transparent, 0f);
            _menuStyle.SetColor(Element.Border, State.Hover, Color.Transparent, 0f);
            _menuStyle.SetColor(Element.Border, State.Press, Color.Transparent, 0f);
            _menuStyle.SetColor(Element.Border, State.Disabled, Color.Transparent, 0f);
            _menuStyle.BorderWidth = 1;

            _menuButtonStyle = new Style(_menuStyle);
            _menuButtonStyle.SetColor(Element.Background, State.Idle, Color.Transparent, 0f);
            _menuButtonStyle.SetColor(Element.Foreground, State.Idle, 2);

            _onScreenStyle = new Style(this);
            _onScreenStyle.SetColor(Element.Background, State.Idle, Color.Transparent, 0f);
            _onScreenStyle.SetColor(Element.Background, State.Hover, Color.Transparent, 0f);
            _onScreenStyle.SetColor(Element.Background, State.Press, Color.Transparent, 0f);
            _onScreenStyle.SetColor(Element.Background, State.Disabled, Color.Transparent, 0f);
            _onScreenStyle.SetColor(Element.Foreground, State.Idle, 2);
            _onScreenStyle.SetColor(Element.Foreground, State.Hover, 2);
            _onScreenStyle.SetColor(Element.Foreground, State.Press, 2);
            _onScreenStyle.SetColor(Element.Foreground, State.Disabled, 2);

            _toolTipStyle = new Style(_menuStyle);
            _notificationStyle = new Style(_menuStyle);
        }
        private MenuButton mainMenu()
        {
            MenuButton mainMenu;
            MenuButton current;
            MenuButton temp;

            // menu
            mainMenu = new MenuButton(image: "menu", width: 60, height: 60, style: _menuStyle);
            mainMenu.PanelAnchorPoint.HorizontalAlign = Align.Left;
            mainMenu.PanelAnchorPoint.VerticalAlign = Align.Bottom;
            mainMenu.KeyBinding = Keys.LeftControl;
            mainMenu.BtnBinding = Buttons.LeftShoulder;
            mainMenu.OnOpen += setFocusToFirstEntry;
            mainMenu.OnClose += setFocusToNull;
            // add menu entries
            mainMenu.AddSubMenu("Game");
            mainMenu.AddSubMenu("Settings");
            mainMenu.AddSubMenu("Emulator");
            mainMenu.AddSubMenu("Quit");

            mainMenu.Style = _menuButtonStyle;

            // ROM
            current = mainMenu["Game"];

            MenuButton romBrowser = current.AddSubMenu("Open ROM");
            romBrowser.OnOpen += (o, e) => { updateRomList(); fillOpenDialog(romBrowser); };

            temp = romBrowser.AddSubMenu("Scroll up", "arrow", 300, 40);
            temp.Image.Rotation = 90;
            temp.OnClick += (o, e) => {
                _openStartIndex -= OPEN_ENTRIES;
                fillOpenDialog(romBrowser);
                MenuButton.Focus = romBrowser.SubMenu.Values.ToArray()[OPEN_ENTRIES];
            };
            for (int i = 0; i < OPEN_ENTRIES; i++)
            {
                romBrowser.AddSubMenu(i.ToString(), null, 300, 40).OnClick += openRomHandler; // add empty entry dummies
            }
            temp = romBrowser.AddSubMenu("Scroll down", "arrow", 300, 40);
            temp.Image.Rotation = -90;
            temp.OnClick += (o, e) => {
                _openStartIndex += OPEN_ENTRIES;
                fillOpenDialog(romBrowser);
                MenuButton.Focus = romBrowser.SubMenu.Values.ToArray()[1];
            };

            current.AddSubMenu("Reset ROM").OnClick += _gameboy.Reset;
            current.AddSubMenu("Exit ROM").OnClick += _gameboy.EjectCartridge;

            // settings
            current = mainMenu["Settings"];

            MenuButton colorList = current.AddSubMenu("Color Palette");
            for (int colorIndex = 0; colorIndex < _emuPalette.Count(); colorIndex++)
            {
                temp = colorList.AddSubMenu("color" + colorIndex.ToString());
                temp.Label.Caption = "";
                if (_emuPaletteNames.Count() > colorIndex) temp.ToolTip = _emuPaletteNames[colorIndex];
                Panel colorPanel = temp.AddPanel();
                colorPanel.Direction = Direction.Horizontal;
                colorPanel.HorizontalAlign = Align.Center;
                colorPanel.VerticalAlign = Align.Center;
                for (int i = 0; i < 4; i++)
                {
                    Label tile = colorPanel.AddLabel("");
                    tile.Width = 30;
                    tile.Height = 30;
                    tile.MarkColor = _emuPalette[colorIndex][i];
                    tile.VerticalAlign = Align.Top;
                    tile.HorizontalAlign = Align.Left;
                }
                temp.ButtonData = colorIndex;
                temp.OnClick += setPaletteButtonHandler;
            }

            temp = current.AddSubMenu("Screen Size");

            MenuButton fs = temp.AddSubMenu("Fullscreen");
            fs.AddSwitch().OnDraw += (o, e) => { ((SwitchControl)o).UpdateSwitch(Game1._Graphics.IsFullScreen); };
            fs.OnClick += fullscreenHandler;
            fs.ToolTip = "Toggle fullscreen mode";

            temp.AddSubMenu("800x720").OnClick += (o, e) => {
                _settings.ScreenWidth = 800;
                _settings.ScreenHeight = 720;
                changeScreenSize();
            };
            temp.AddSubMenu("1280x720").OnClick += (o, e) => {
                _settings.ScreenWidth = 1280;
                _settings.ScreenHeight = 720;
                changeScreenSize();
            };
            temp.AddSubMenu("1920x1080").OnClick += (o, e) => {
                _settings.ScreenWidth = 1920;
                _settings.ScreenHeight = 1080;
                changeScreenSize();
            };

            MenuButton volume = current.AddSubMenu("Volume");
            volume.OnDraw += (o, e) => { ((MenuButton)o).Label.Caption = "Volume: " + _volumeList[_settings.VolumeIndex].ToString("0%"); };

            for (int i = 0; i < _volumeList.Count(); i++)
            {
                temp = volume.AddSubMenu(_volumeList[i].ToString("0%"));
                temp.Height = 40;
                temp.Label.HorizontalAlign = Align.Center;
                temp.ButtonData = i;
                temp.OnClick += (o, e) => { VolumeIndex = ((MenuButton)o).ButtonData; };
            }

            // emulator
            current = mainMenu["Emulator"];

            MenuButton overlay = current.AddSubMenu("Overlay");

            temp = overlay.AddSubMenu("Menu Icon");
            temp.AddSwitch().OnDraw += (o, e) => { ((SwitchControl)o).UpdateSwitch(_menu.Image.Visible); };
            temp.OnClick += (o, e) => { _menu.Image.Visible = !_menu.Image.Visible; _settings.IsMenuIconVisible = _menu.Image.Visible; };

            temp = overlay.AddSubMenu("FPS");
            temp.AddSwitch().OnDraw += (o, e) => { ((SwitchControl)o).UpdateSwitch(_fps.Visible); };
            temp.OnClick += (o, e) => { _fps.Visible = !_fps.Visible; _settings.IsFpsVisible = _fps.Visible; };
            temp.ToolTip = "Show FPS";

            MenuButton audio = overlay.AddSubMenu("Audio Panel");
            audio.AddSwitch().OnDraw += (o, e) => { ((SwitchControl)o).UpdateSwitch(_audioBar.Panel.Visible); };
            audio.OnClick += (o, e) => { _audioBar.Panel.Visible = !_audioBar.Panel.Visible; _settings.IsAudioPanelVisible = _audioBar.Panel.Visible; };
            audio.ToolTip = "Show audio sidebar";

            temp = overlay.AddSubMenu("Buttons");
            temp.AddSwitch().OnDraw += (o, e) => { ((SwitchControl)o).UpdateSwitch(_onScreenButtonsBase.Visible); };
            temp.OnClick += (o, e) => { _onScreenButtonsBase.Visible = !_onScreenButtonsBase.Visible; _settings.IsOnScreenButtonsVisible = _onScreenButtonsBase.Visible; };
            temp.ToolTip = "Show onscreen buttons";

            temp = overlay.AddSubMenu("Notifications");
            temp.AddSwitch().OnDraw += (o, e) => { ((SwitchControl)o).UpdateSwitch(_notifications.Visible); };
            temp.OnClick += (o, e) => { _notifications.Visible = !_notifications.Visible; _settings.IsNotificationsVisible = _notifications.Visible; };

            temp = overlay.AddSubMenu("Grid");
            temp.AddSwitch().OnDraw += (o, e) => { ((SwitchControl)o).UpdateSwitch(_settings.IsGridVisible); };
            temp.OnClick += (o, e) => { _settings.IsGridVisible = !_settings.IsGridVisible; };
            temp.ToolTip = "Show pixel grid";

            MenuButton layers = current.AddSubMenu("Layers");

            MenuButton draw = layers.AddSubMenu("Visible");
            temp = draw.AddSubMenu("Background");
            temp.AddSwitch().OnDraw += (o, e) => { ((SwitchControl)o).UpdateSwitch(_settings.IsBackgroundVisible); };
            temp.OnClick += (o, e) => { _settings.IsBackgroundVisible = !_settings.IsBackgroundVisible; };
            temp = draw.AddSubMenu("Window");
            temp.AddSwitch().OnDraw += (o, e) => { ((SwitchControl)o).UpdateSwitch(_settings.IsWindowVisible); };
            temp.OnClick += (o, e) => { _settings.IsWindowVisible = !_settings.IsWindowVisible; };
            temp = draw.AddSubMenu("Sprites");
            temp.AddSwitch().OnDraw += (o, e) => { ((SwitchControl)o).UpdateSwitch(_settings.IsSpritesVisible); };
            temp.OnClick += (o, e) => { _settings.IsSpritesVisible = !_settings.IsSpritesVisible; };

            MenuButton mark = layers.AddSubMenu("Highlight");
            temp = mark.AddSubMenu("Background");
            temp.AddSwitch().OnDraw += (o, e) => { ((SwitchControl)o).UpdateSwitch(_settings.IsBackgroundHighlighted); };
            temp.OnClick += (o, e) => { _settings.IsBackgroundHighlighted = !_settings.IsBackgroundHighlighted; };
            temp = mark.AddSubMenu("Window");
            temp.AddSwitch().OnDraw += (o, e) => { ((SwitchControl)o).UpdateSwitch(_settings.IsWindowHighlighted); };
            temp.OnClick += (o, e) => { _settings.IsWindowHighlighted = !_settings.IsWindowHighlighted; };
            temp = mark.AddSubMenu("Sprites");
            temp.AddSwitch().OnDraw += (o, e) => { ((SwitchControl)o).UpdateSwitch(_settings.IsSpritesHighlighted); };
            temp.OnClick += (o, e) => { _settings.IsSpritesHighlighted = !_settings.IsSpritesHighlighted; };

            audio = current.AddSubMenu("Channels");
            for (int i = 0; i < 4; i++)
            {
                temp = audio.AddSubMenu("CH" + (i + 1).ToString());
                temp.ButtonData = i;
                temp.AddSwitch().OnDraw += (o, e) => { ((SwitchControl)o).UpdateSwitch(_gameboy.MasterSwitch[((MenuButton)((SwitchControl)o).ParentControl).ButtonData]); };
                temp.OnClick += audioSwitchHandler;
            }
            audio["CH1"].ToolTip = "Channel 1: Square wave 1";
            audio["CH2"].ToolTip = "Channel 2: Square wave 2";
            audio["CH3"].ToolTip = "Channel 3: Custom wave";
            audio["CH4"].ToolTip = "Channel 4: Noise";

            current.AddSubMenu("About").OnClick += (o, e) => { _notifications.Push("GEM by André Mauser\ngithub.com/andremauser", _notificationStyle, NotificationType.Information); };

            // quit
            current = mainMenu["Quit"];
            current.AddSubMenu("Quit GEM").OnClick += exitHandler;

            return mainMenu;
        }
        private MenuButton fpsMenu()
        {
            MenuButton fpsMenu;
            MenuButton temp;
            Label label;
            int subMenuWidth = 200;

            fpsMenu = new MenuButton(null, null, "fps", _menuStyle, MenuType.Default) { Width = 60, Height = 60 };
            fpsMenu.KeyBinding = Keys.RightControl;
            fpsMenu.BtnBinding = Buttons.RightShoulder;
            fpsMenu.ToolTip = "Current Frame Rate";
            fpsMenu.Visible = false;
            fpsMenu.Label.HorizontalAlign = Align.Center;
            fpsMenu.OnDraw += (o, e) => {
                ((MenuButton)o).Label.Caption = Game1._Instance.FPS.ToString();
                ((BaseControl)o).Left = Game1._Graphics.GraphicsDevice.Viewport.Width - ((BaseControl)o).Width;
                ((BaseControl)o).Top = 0;
            };
            fpsMenu.PanelAnchorPoint.HorizontalAlign = Align.Right;
            fpsMenu.PanelAnchorPoint.VerticalAlign = Align.Bottom;
            fpsMenu.Panel.HorizontalAlign = Align.Right;
            fpsMenu.OnOpen += setFocusToFirstEntry;
            fpsMenu.OnClose += setFocusToNull;

            // submenu
            temp = fpsMenu.AddSubMenu("update");
            temp.Width = subMenuWidth;
            temp.ToolTip = "Input / UI";
            temp.OnDraw += (o, e) => { ((MenuButton)o).Label.Caption = String.Format("{0:0 %}", _timespanUpdate * Game1.FRAME_RATE / 1000); };
            temp.Label.HorizontalAlign = Align.Right;
            label = temp.AddLabel(temp.ToolTip + ":");
            label.Margin = 15;
            label.HorizontalAlign = Align.Left;

            temp = fpsMenu.AddSubMenu("emulation");
            temp.Width = subMenuWidth;
            temp.ToolTip = "Emulation";
            temp.OnDraw += (o, e) => { ((MenuButton)o).Label.Caption = String.Format("{0:0 %}", _timespanEmulation * Game1.FRAME_RATE / 1000); };
            temp.Label.HorizontalAlign = Align.Right;
            label = temp.AddLabel(temp.ToolTip + ":");
            label.Margin = 15;
            label.HorizontalAlign = Align.Left;

            temp = fpsMenu.AddSubMenu("draw");
            temp.Width = subMenuWidth;
            temp.ToolTip = "Rendering";
            temp.OnDraw += (o, e) => { ((MenuButton)o).Label.Caption = String.Format("{0:0 %}", _timespanDraw * Game1.FRAME_RATE / 1000); };
            temp.Label.HorizontalAlign = Align.Right;
            label = temp.AddLabel(temp.ToolTip + ":");
            label.Margin = 15;
            label.HorizontalAlign = Align.Left;

            return fpsMenu;
        }
        private MenuButton audioMenu()
        {
            MenuButton audioMenu;
            MenuButton current;
            MenuButton temp;

            audioMenu = new MenuButton(image: "menu", menuType: MenuType.Default, style: _menuStyle) { Width = 60, Height = 60 };
            audioMenu.IsClosedOnClickOutside = false;
            audioMenu.Image.ResizeToParent();
            audioMenu.PanelAnchorPoint.HorizontalAlign = Align.Right;
            audioMenu.PanelAnchorPoint.VerticalAlign = Align.Top;


            // vol %
            current = audioMenu.AddSubMenu("Volume", null, 60);
            current.Label.HorizontalAlign = Align.Center;
            current.OnDraw += (o, e) => { ((MenuButton)o).Label.Caption = _volumeList[_settings.VolumeIndex].ToString("0%"); };
            current.ToolTip = "Volume";
            for (int i = 0; i < _volumeList.Count(); i++)
            {
                temp = current.AddSubMenu(_volumeList[i].ToString("0%"));
                temp.Label.HorizontalAlign = Align.Center;
                temp.Height = 40;
                temp.ButtonData = i;
                temp.OnClick += (o, e) => { VolumeIndex = ((MenuButton)o).ButtonData; };
            }
            // vol plus
            current = audioMenu.AddSubMenu("volplus", "volplus", 60);
            current.OnClick += (o, e) => { VolumeIndex++; };
            current.ToolTip = "Volume up";
            // vol minus
            current = audioMenu.AddSubMenu("volminus", "volminus", 60);
            current.OnClick += (o, e) => { VolumeIndex--; };
            current.ToolTip = "Volume down";
            // channels
            for (int i = 0; i < 4; i++)
            {
                temp = audioMenu.AddSubMenu("CH" + (i + 1).ToString(), "sound", 60, 60, 3);
                temp.ButtonData = i;
                temp.Style = new Style(_menuStyle);
                temp.OnClick += audioSwitchHandler;
                temp.OnDraw += audioIconsHandler;
            }
            audioMenu["CH1"].ToolTip = "Channel 1: Square wave 1";
            audioMenu["CH2"].ToolTip = "Channel 2: Square wave 2";
            audioMenu["CH3"].ToolTip = "Channel 3: Custom wave";
            audioMenu["CH4"].ToolTip = "Channel 4: Noise";

            return audioMenu;
        }
        private MenuButton toolTip()
        {
            MenuButton toolTip;

            toolTip = new MenuButton(null, null, "", _toolTipStyle, MenuType.StandAlone) { Enabled = false, Height = 40 };
            toolTip.Label.Padding = 15;
            toolTip.Label.HorizontalAlign = Align.Left;
            toolTip.OnDraw += (o, e) =>
            {
                // update tooltip line
                toolTip.Label.Caption = MenuButton.Focus != null ? MenuButton.Focus.ToolTip : "";
                toolTip.Width = toolTip.Label.Caption == "" ? 0 : toolTip.Label.Width + 2 * toolTip.Label.Padding.Left;
                toolTip.Top = Game1._Graphics.GraphicsDevice.Viewport.Height - _toolTip.Height;
            };

            return toolTip;
        }
        private BaseControl onScreenButtons()
        {
            BaseControl onScreenButtons;
            onScreenButtons = new BaseControl(null);

            // dpad
            BaseControl dpad = new BaseControl(null);
            onScreenButtons.Add(dpad);
            dpad.OnDraw += (o, e) =>
            {
                ((BaseControl)o).Top = _screenTop + _screenHeight / 2;
                ((BaseControl)o).Left = Math.Max(_screenLeft / 2, 120);
            };

            // up
            MenuButton dpadUp;
            dpadUp = new MenuButton(dpad, null, "up", _onScreenStyle, MenuType.StandAlone, "dpad", 4, 100, 100);
            dpadUp.Left = -50;
            dpadUp.Top = -120;
            dpadUp.Image.Rotation = 90;
            dpadUp.KeyBinding = Keys.Up;
            dpadUp.BtnBinding = Buttons.DPadUp;
            dpadUp.OnPress += (o, e) => _gameboy.IsButton_Up = true;
            dpadUp.OnRelease += (o, e) => _gameboy.IsButton_Up = false;
            dpad.Add(dpadUp);

            // down
            MenuButton dpadDown;
            dpadDown = new MenuButton(dpad, null, "down", _onScreenStyle, MenuType.StandAlone, "dpad", 4, 100, 100);
            dpadDown.Left = -50;
            dpadDown.Top = 20;
            dpadDown.Image.Rotation = 270;
            dpadDown.KeyBinding = Keys.Down;
            dpadDown.BtnBinding = Buttons.DPadDown;
            dpadDown.OnPress += (o, e) => _gameboy.IsButton_Down = true;
            dpadDown.OnRelease += (o, e) => _gameboy.IsButton_Down = false;
            dpad.Add(dpadDown);

            // right
            MenuButton dpadRight;
            dpadRight = new MenuButton(dpad, null, "->", _onScreenStyle, MenuType.StandAlone, "dpad", 4, 100, 100);
            dpadRight.Left = 20;
            dpadRight.Top = -50;
            dpadRight.KeyBinding = Keys.Right;
            dpadRight.BtnBinding = Buttons.DPadRight;
            dpadRight.OnPress += (o, e) => _gameboy.IsButton_Right = true;
            dpadRight.OnRelease += (o, e) => _gameboy.IsButton_Right = false;
            dpad.Add(dpadRight);

            // left
            MenuButton dpadLeft;
            dpadLeft = new MenuButton(dpad, null, "<-", _onScreenStyle, MenuType.StandAlone, "dpad", 4, 100, 100);
            dpadLeft.Left = -120;
            dpadLeft.Top = -50;
            dpadLeft.Image.Rotation = 180;
            dpadLeft.KeyBinding = Keys.Left;
            dpadLeft.BtnBinding = Buttons.DPadLeft;
            dpadLeft.OnPress += (o, e) => _gameboy.IsButton_Left = true;
            dpadLeft.OnRelease += (o, e) => _gameboy.IsButton_Left = false;
            dpad.Add(dpadLeft);


            // buttons A, B
            BaseControl btns = new BaseControl(null) { Left = 1150, Top = 400 };
            onScreenButtons.Add(btns);
            btns.OnDraw += (o, e) =>
            {
                ((BaseControl)o).Top = _screenTop + _screenHeight / 2;
                ((BaseControl)o).Left = Math.Min(_screenWidth + (int)(_screenLeft * 1.5f), _screenWidth + _screenLeft * 2 - 120);
            };

            // A
            MenuButton btnA;
            btnA = new MenuButton(btns, null, "A", _onScreenStyle, MenuType.StandAlone, "btna", 4, 100, 100);
            btnA.Left = 0;
            btnA.Top = -70;
            btnA.Image.Rotation = 20;
            btnA.KeyBinding = Keys.X;
            btnA.BtnBinding = Buttons.B;
            btnA.OnPress += (o, e) => _gameboy.IsButton_A = true;
            btnA.OnRelease += (o, e) => _gameboy.IsButton_A = false;
            btns.Add(btnA);

            // B
            MenuButton btnB;
            btnB = new MenuButton(btns, null, "B", _onScreenStyle, MenuType.StandAlone, "btnb", 4, 100, 100);
            btnB.Left = -120;
            btnB.Top = -30;
            btnB.Image.Rotation = 20;
            btnB.KeyBinding = Keys.Y;
            btnB.BtnBinding = Buttons.A;
            btnB.OnPress += (o, e) => _gameboy.IsButton_B = true;
            btnB.OnRelease += (o, e) => _gameboy.IsButton_B = false;
            btns.Add(btnB);

            // start
            MenuButton btnStart;
            btnStart = new MenuButton(null, null, "Start", _onScreenStyle, MenuType.StandAlone, "stasel", 4, 100, 100);
            btnStart.Image.Rotation = 20;
            btnStart.KeyBinding = Keys.Enter;
            btnStart.BtnBinding = Buttons.Start;
            btnStart.OnPress += (o, e) => _gameboy.IsButton_Start = true;
            btnStart.OnRelease += (o, e) => _gameboy.IsButton_Start = false;
            btnStart.OnDraw += (o, e) =>
            {
                ((BaseControl)o).Top = _screenTop + _screenHeight - ((BaseControl)o).Height;
                ((BaseControl)o).Left = Math.Min(_screenLeft + _screenWidth + 20, btnB.LocationX);
            };
            onScreenButtons.Add(btnStart);

            // select
            MenuButton btnSelect;
            btnSelect = new MenuButton(null, null, "Select", _onScreenStyle, MenuType.StandAlone, "stasel", 4, 100, 100);
            btnSelect.Image.Rotation = 20;
            btnSelect.KeyBinding = Keys.Back;
            btnSelect.BtnBinding = Buttons.Back;
            btnSelect.OnPress += (o, e) => _gameboy.IsButton_Select = true;
            btnSelect.OnRelease += (o, e) => _gameboy.IsButton_Select = false;
            btnSelect.OnDraw += (o, e) =>
            {
                ((BaseControl)o).Top = _screenTop + _screenHeight - ((BaseControl)o).Height;
                ((BaseControl)o).Left = Math.Max(_screenLeft - ((BaseControl)o).Width - 20, dpadRight.LocationX);
            };
            onScreenButtons.Add(btnSelect);

            return onScreenButtons;
        }
        private void changeScreenSize()
        {
            Game1._Graphics.PreferredBackBufferWidth = _settings.ScreenWidth;
            Game1._Graphics.PreferredBackBufferHeight = _settings.ScreenHeight;
            Game1._Graphics.ApplyChanges();
        }

        // Draw Methods
        private void drawEmulator(Viewport viewport, Texture2D[] screens)
        {
            // Screen Position & Size
            float pixelSize = MathHelper.Min(viewport.Height / 144f,
                                             viewport.Width / 160f);

            _screenWidth = (int)(pixelSize * 160);
            _screenHeight = (int)(pixelSize * 144);
            _screenLeft = (viewport.Width - _screenWidth) / 2;
            _screenTop = (viewport.Height - _screenHeight) / 2;

            // Draw Screen
            if (_settings.IsBackgroundVisible) _spriteBatch.Draw(screens[0], new Rectangle(_screenLeft, _screenTop, _screenWidth, _screenHeight), _settings.IsBackgroundHighlighted ? _pixelMarkerTextColor : Color.White);
            if (_settings.IsWindowVisible) _spriteBatch.Draw(screens[1], new Rectangle(_screenLeft, _screenTop, _screenWidth, _screenHeight), _settings.IsWindowHighlighted ? _pixelMarkerTextColor : Color.White);
            if (_settings.IsSpritesVisible) _spriteBatch.Draw(screens[2], new Rectangle(_screenLeft, _screenTop, _screenWidth, _screenHeight), _settings.IsSpritesHighlighted ? _pixelMarkerTextColor : Color.White);

            // Draw Grid
            if (_settings.IsGridVisible)
            {
                drawGrid(pixelSize, _screenLeft, _screenTop, _screenWidth, _screenHeight);
                if (Input.MousePosX >= _screenLeft &&
                    Input.MousePosX < _screenLeft + _screenWidth &&
                    Input.MousePosY >= _screenTop &&
                    Input.MousePosY < _screenTop + _screenHeight)
                {
                    drawMouseMarker(pixelSize, _screenLeft, _screenTop);
                }
            }
        }
        private void drawMouseMarker(float size, int left, int top)
        {
            if (!Game1._Instance.IsMouseVisible) return;
            int pixelX = (int)((Input.MousePosX - left) / size);
            int pixelY = (int)((Input.MousePosY - top) / size);
            int pixelPosX = (int)(left + size * pixelX);
            int pixelPosY = (int)(top + size * pixelY);
            int pixelSize = (int)(size + 1);

            int tileX = pixelX / 8;
            int tileY = pixelY / 8;
            int tilePosX = (int)(left + size * 8 * tileX);
            int tilePosY = (int)(top + size * 8 * tileY);
            int tileSize = (int)(size * 8 + 1);

            // Tile Marker
            _spriteBatch.DrawString(_Font, tileX.ToString(), new Vector2(tilePosX, top), _pixelMarkerTextColor);
            _spriteBatch.DrawString(_Font, tileY.ToString(), new Vector2(left, tilePosY), _pixelMarkerTextColor);

            // Pixel Marker
            _spriteBatch.Draw(_Pixel, new Rectangle(pixelPosX, pixelPosY, pixelSize, pixelSize), _pixelMarkerColor);
            _spriteBatch.DrawString(_Font, string.Format("{0}", pixelX), new Vector2(pixelPosX, pixelPosY - 24), _pixelMarkerTextColor);
            _spriteBatch.DrawString(_Font, string.Format("{0,3}", pixelY), new Vector2(pixelPosX - 40, pixelPosY), _pixelMarkerTextColor);
        }
        private void drawGrid(float pixelSize, int left, int top, int width, int height)
        {
            Color gridColor;

            for (int x = 0; x < 160; x += 1)
            {
                if (x % 8 == 0)
                {
                    gridColor = _gridColorDark;
                }
                else
                {
                    gridColor = _gridColorLight;
                }
                _spriteBatch.Draw(_Pixel, new Rectangle((int)(left + x * pixelSize), top, 1, height), gridColor);
            }
            for (int y = 0; y < 144; y += 1)
            {
                if (y % 8 == 0)
                {
                    gridColor = _gridColorDark;
                }
                else
                {
                    gridColor = _gridColorLight;
                }
                _spriteBatch.Draw(_Pixel, new Rectangle(left, (int)(top + y * pixelSize), width, 1), gridColor);
            }
        }

        // Event Handler
        private void fullscreenHandler(object sender, EventArgs e)
        {
            Game1._Graphics.ToggleFullScreen();
            _settings.IsFullScreen = Game1._Graphics.IsFullScreen;
        }
        private void vsyncHandler(object sender, EventArgs e)
        {
            Game1._Graphics.SynchronizeWithVerticalRetrace = !Game1._Graphics.SynchronizeWithVerticalRetrace;
            Game1._Graphics.ApplyChanges();
        }
        private void exitHandler(object sender, EventArgs e)
        {
            Game1._Instance.Exit();
        }
        private void setPaletteButtonHandler(object sender, EventArgs e)
        {
            MenuButton btn = (MenuButton)sender;
            EmuColorIndex = btn.ButtonData;
        }
        private void openRomHandler(object sender, EventArgs e)
        {
            MenuButton menuButton = (MenuButton)sender;
            int index = menuButton.ButtonData;
            _gameboy.PowerOff();
            if (index < _romList.Count) // error check
            {
                _gameboy.InsertCartridge(_romList[index]);
                _gameboy.PowerOn();
            }
            _menu.Close(null, EventArgs.Empty);
        }
        private void audioSwitchHandler(Object sender, EventArgs e)
        {
            MenuButton btn = (MenuButton)sender;
            _gameboy.MasterSwitch[btn.ButtonData] = !_gameboy.MasterSwitch[btn.ButtonData];
            _settings.AudioChannels = _gameboy.MasterSwitch;
        }
        private void audioIconsHandler(object sender, EventArgs e)
        {
            int i = ((MenuButton)sender).ButtonData;
            if (_gameboy.MasterSwitch[i])
            {
                // MasterSwitch ON
                ((MenuButton)sender).Image.ImageIndex = _gameboy.IsChannelOutput[i] ? 2 : 1;

                if (_gameboy.IsChannelOn[i])
                {
                    ((MenuButton)sender).Style.SetColor(Element.Foreground, State.Idle, _emuPalette[EmuColorIndex][1]);
                    ((MenuButton)sender).Style.SetColor(Element.Foreground, State.Disabled, _emuPalette[EmuColorIndex][1]);
                }
                else
                {
                    ((MenuButton)sender).Style.SetColor(Element.Foreground, State.Idle, _emuPalette[EmuColorIndex][2]);
                    ((MenuButton)sender).Style.SetColor(Element.Foreground, State.Disabled, _emuPalette[EmuColorIndex][2]);
                }
            }
            else
            {
                // Masterswitch OFF
                ((MenuButton)sender).Image.ImageIndex = 0;
                ((MenuButton)sender).Style.SetColor(Element.Foreground, State.Idle, _emuPalette[EmuColorIndex][2]);
                ((MenuButton)sender).Style.SetColor(Element.Foreground, State.Disabled, _emuPalette[EmuColorIndex][2]);
            }
        }
        private void setFocusToFirstEntry(object sender, EventArgs e)
        {
            MenuButton.Focus = ((MenuButton)sender).SubMenu.Values.ToArray()[0]; 
            MenuButton.Focus.Close(sender, e);
        }
        private void setFocusToNull(object sender, EventArgs e)
        {
            MenuButton.Focus = null;
        }
#endregion
    }
}
