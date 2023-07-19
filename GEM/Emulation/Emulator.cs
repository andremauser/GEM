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

        Style _menuStyle;
        Style _menuButtonStyle;
        Style _onScreenStyle;
        Style _toolTipStyle;
        Style _notificationStyle;

        // events
        public delegate void PaletteChange(Color[] colors);
        public event PaletteChange OnPaletteChange;

        // states
        int _volumeIndex = 0;
        int _emuColorIndex = 0;
        int _openStartIndex = 0;
        bool _showGrid = false;
        bool _drawBackground = true;
        bool _drawWindow = true;
        bool _drawSprites = true;
        bool _markBackground = false;
        bool _markWindow = false;
        bool _markSprites = false;

        // timespans
        double _timespanUpdate;
        double _timespanDraw;
        double _timespanEmulation;

        // buttonset base
        BaseControl _onScreenButtonsBase;

        // fields
        List<string> _romList = new List<string>();
        List<BaseControl> _controls;
        float[] _volumeList;
        MenuButton _menu;
        MenuButton _audioBar;
        MenuButton _toolTip;
        NotificationPanel _notifications;
        MenuButton _dpadUp;
        MenuButton _dpadDown;
        MenuButton _dpadLeft;
        MenuButton _dpadRight;
        MenuButton _btnA;
        MenuButton _btnB;
        MenuButton _btnStart;
        MenuButton _btnSelect;
        MenuButton _fps;
        int _screenWidth;
        int _screenHeight;
        int _screenLeft;
        int _screenTop;
        bool _writeRAM = false;
        DateTime _saveTime;
        const int SAVE_DELAY_MS = 500;
        #endregion

        #region Constructors
        public Emulator(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _controls = new List<BaseControl>();
            _volumeList = new float[] { 0f, 0.01f, 0.05f, 0.1f, 0.25f, 0.5f, 0.75f, 1f };
            Cartridge.OnWriteRAM += (o, e) => { _writeRAM = true; _saveTime = DateTime.Now + TimeSpan.FromMilliseconds(SAVE_DELAY_MS); };
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
                return _volumeIndex;
            }
            set
            {
                _volumeIndex = Math.Clamp(value, 0, _volumeList.Length - 1);
                _gameboy.SetVolume(_volumeList[_volumeIndex]);
            }
        }
        public int EmuColorIndex
        {
            get
            {
                return _emuColorIndex;
            }
            set
            {
                _emuColorIndex = value;
                OnPaletteChange?.Invoke(_emuPalette[_emuColorIndex]);
            }
        }
        #endregion

        #region Methods
        public void LoadContent(ContentManager content)
        {
            _gameboy = new Gameboy(_graphicsDevice);
            _spriteBatch = new SpriteBatch(_graphicsDevice);
            _Font = content.Load<SpriteFont>("Console");
            _Pixel = new Texture2D(_graphicsDevice, 1, 1);
            _Pixel.SetData(new Color[] { Color.White });
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
            updateRomList(); // initial call to rom search - updated by click on "Open ROM"

            _gameboy.OnPowerOn += (o, e) => { _notifications.Push(CartridgeTitle, _notificationStyle, NotificationType.Information); };

            // disable and re-enable background controls
            MenuButton.OnFocusChange += (o, e) => {
                if (!MenuButton.IsFocusSet)
                {
                    _onScreenButtonsBase.Enabled = true;
                    _audioBar.Enabled = true;
                }
                if (MenuButton.IsFocusSet && MenuButton.Focus.RootControl != _onScreenButtonsBase)
                {
                    _onScreenButtonsBase.Enabled = false;
                }
                if (MenuButton.IsFocusSet && MenuButton.Focus.RootControl != _audioBar)
                {
                    _audioBar.Enabled = false;
                }
            };

            // styles
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

            #region onscreen buttons

            _onScreenButtonsBase = new BaseControl(null);
            _onScreenButtonsBase.Visible = false; // hide OnScreenButtons on start
            _controls.Add(_onScreenButtonsBase);

            // dpad
            BaseControl _dpad = new BaseControl(null);
            _onScreenButtonsBase.Add(_dpad);
            _dpad.OnDraw += (o, e) => 
            {
                ((BaseControl)o).Top = _screenTop + _screenHeight / 2;
                ((BaseControl)o).Left = Math.Max(_screenLeft / 2, 120);
            };

            // up
            _dpadUp = new MenuButton(_dpad, null, "up", _onScreenStyle, MenuType.StandAlone, "dpad", 4) { Width = 100, Height = 100 };
            _dpadUp.Left = -50;
            _dpadUp.Top = -120;
            _dpadUp.Image.ResizeToParent();
            _dpadUp.Image.Rotation = 90;
            _dpadUp.KeyBinding = Keys.Up;
            _dpadUp.BtnBinding = Buttons.DPadUp;
            _dpadUp.OnPress += (o, e) =>  _gameboy.IsButton_Up = true;
            _dpadUp.OnRelease += (o, e) => _gameboy.IsButton_Up = false;
            _dpad.Add(_dpadUp);

            // down
            _dpadDown = new MenuButton(_dpad, null, "down", _onScreenStyle, MenuType.StandAlone, "dpad", 4) { Width = 100, Height = 100 };
            _dpadDown.Left = -50;
            _dpadDown.Top = 20;
            _dpadDown.Image.ResizeToParent();
            _dpadDown.Image.Rotation = 270;
            _dpadDown.KeyBinding = Keys.Down;
            _dpadDown.BtnBinding = Buttons.DPadDown;
            _dpadDown.OnPress += (o, e) => _gameboy.IsButton_Down = true;
            _dpadDown.OnRelease += (o, e) => _gameboy.IsButton_Down = false;
            _dpad.Add(_dpadDown);

            // right
            _dpadRight = new MenuButton(_dpad, null, "->", _onScreenStyle, MenuType.StandAlone, "dpad", 4) { Width = 100, Height = 100 };
            _dpadRight.Left = 20;
            _dpadRight.Top = -50;
            _dpadRight.Image.ResizeToParent();
            _dpadRight.KeyBinding = Keys.Right;
            _dpadRight.BtnBinding = Buttons.DPadRight;
            _dpadRight.OnPress += (o, e) => _gameboy.IsButton_Right = true;
            _dpadRight.OnRelease += (o, e) => _gameboy.IsButton_Right = false;
            _dpad.Add(_dpadRight);

            // left
            _dpadLeft = new MenuButton(_dpad, null, "<-", _onScreenStyle, MenuType.StandAlone, "dpad", 4) { Width = 100, Height = 100 };
            _dpadLeft.Left = -120;
            _dpadLeft.Top = -50;
            _dpadLeft.Image.ResizeToParent();
            _dpadLeft.Image.Rotation = 180;
            _dpadLeft.KeyBinding = Keys.Left;
            _dpadLeft.BtnBinding = Buttons.DPadLeft;
            _dpadLeft.OnPress += (o, e) => _gameboy.IsButton_Left = true;
            _dpadLeft.OnRelease += (o, e) => _gameboy.IsButton_Left = false;
            _dpad.Add(_dpadLeft);


            // buttons A, B
            BaseControl _btns = new BaseControl(null) { Left = 1150, Top = 400 };
            _onScreenButtonsBase.Add(_btns);
            _btns.OnDraw += (o, e) =>
            {
                ((BaseControl)o).Top = _screenTop + _screenHeight / 2;
                ((BaseControl)o).Left = Math.Min(
                    _screenWidth + (int)(_screenLeft * 1.5f),
                    _screenWidth + _screenLeft * 2 - 120
                    );
            };

            // A
            _btnA = new MenuButton(_btns, null, "A", _onScreenStyle, MenuType.StandAlone, "btna", 4) { Width = 100, Height = 100 };
            _btnA.Left = 0;
            _btnA.Top = -70;
            _btnA.Image.ResizeToParent();
            _btnA.Image.Rotation = 20;
            _btnA.KeyBinding = Keys.X;
            _btnA.BtnBinding = Buttons.B;
            _btnA.OnPress += (o, e) => _gameboy.IsButton_A = true;
            _btnA.OnRelease += (o, e) => _gameboy.IsButton_A = false;
            _btns.Add(_btnA);

            // B
            _btnB = new MenuButton(_btns, null, "B", _onScreenStyle, MenuType.StandAlone, "btnb", 4) { Width = 100, Height = 100 };
            _btnB.Left = -120;
            _btnB.Top = -30;
            _btnB.Image.ResizeToParent();
            _btnB.Image.Rotation = 20;
            _btnB.KeyBinding = Keys.Y;
            _btnB.BtnBinding = Buttons.A;
            _btnB.OnPress += (o, e) => _gameboy.IsButton_B = true;
            _btnB.OnRelease += (o, e) => _gameboy.IsButton_B = false;
            _btns.Add(_btnB);

            // start
            _btnStart = new MenuButton(null, null, "Start", _onScreenStyle, MenuType.StandAlone, "stasel", 4) { Width = 100, Height = 100 };
            _btnStart.Left = 1040;
            _btnStart.Top = 620;
            _btnStart.Image.ResizeToParent();
            _btnStart.Image.Rotation = 20;
            _btnStart.KeyBinding = Keys.Enter;
            _btnStart.BtnBinding = Buttons.Start;
            _btnStart.OnPress += (o, e) => _gameboy.IsButton_Start = true;
            _btnStart.OnRelease += (o, e) => _gameboy.IsButton_Start = false;
            _btnStart.OnDraw += (o, e) =>
            {
                ((BaseControl)o).Top = _screenTop + _screenHeight - ((BaseControl)o).Height;
                ((BaseControl)o).Left = Math.Min(
                    _screenLeft + _screenWidth + 20, 
                    _btnB.LocationX
                    );
            }; 
            _onScreenButtonsBase.Add(_btnStart);

            // select
            _btnSelect = new MenuButton(null, null, "Select", _onScreenStyle, MenuType.StandAlone, "stasel", 4) { Width = 100, Height = 100 };
            _btnSelect.Image.ResizeToParent();
            _btnSelect.Image.Rotation = 20;
            _btnSelect.KeyBinding = Keys.Back;
            _btnSelect.BtnBinding = Buttons.Back;
            _btnSelect.OnPress += (o, e) => _gameboy.IsButton_Select = true;
            _btnSelect.OnRelease += (o, e) => _gameboy.IsButton_Select = false;
            _btnSelect.OnDraw += (o, e) =>
            {
                ((BaseControl)o).Top = _screenTop + _screenHeight - ((BaseControl)o).Height;
                ((BaseControl)o).Left = Math.Max(
                    _screenLeft - ((BaseControl)o).Width - 20, 
                    _dpadRight.LocationX
                    );
            }; 
            _onScreenButtonsBase.Add(_btnSelect);

            #endregion

            _audioBar = audioMenu();
            _controls.Add(_audioBar);

            _fps = fpsMenu();
            _controls.Add(_fps);

            _menu = mainMenu();
            _controls.Add(_menu);

            #region tooltip

            _toolTip = new MenuButton(null, null, "", _toolTipStyle, MenuType.StandAlone) { Enabled = false, Height = 40 };
            _toolTip.Label.Padding = 15;
            _toolTip.Label.HorizontalAlign = Align.Left;
            _toolTip.OnDraw += (o, e) =>
            {
                // update tooltip line
                _toolTip.Label.Caption = MenuButton.Focus != null ? MenuButton.Focus.ToolTip : "";
                _toolTip.Width = _toolTip.Label.Caption == "" ? 0 : _toolTip.Label.Width + 2 * _toolTip.Label.Padding.Left;
                _toolTip.Top = Game1._Graphics.GraphicsDevice.Viewport.Height - _toolTip.Height;
            };
            _controls.Add(_toolTip);

            #endregion


            // initial color palette
            EmuColorIndex = 0;


            _notifications = new NotificationPanel(null);
            _controls.Add(_notifications);

            _notifications.Push("Welcome to GEM", _notificationStyle, NotificationType.Information, 10);
        }

        public void Update(GameTime gameTime)
        {
            DateTime start = DateTime.Now;

            _notifications.Left = _screenWidth + 2 * _screenLeft;
            _notifications.Top = _screenHeight + 2 * _screenTop;

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

            // save RAM to file
            if (_writeRAM && DateTime.Now >= _saveTime)
            {
                _gameboy.SaveRAM();
                _notifications.Push("Game saved", _notificationStyle, NotificationType.Success);
                _writeRAM = false;
            }

            DateTime end = DateTime.Now;
            _timespanEmulation = (afterEmulation - start).TotalMilliseconds;
            _timespanDraw = (end - afterEmulation).TotalMilliseconds;
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
            mainMenu.AddSubMenu("ROM");
            mainMenu.AddSubMenu("Game Boy");
            mainMenu.AddSubMenu("Display");
            mainMenu.AddSubMenu("Sound");
            mainMenu.AddSubMenu("Debug");
            mainMenu.AddSubMenu("Quit");

            mainMenu.Style = _menuButtonStyle;

            // ROM
            current = mainMenu["ROM"];

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
      

            // Game Boy
            current = mainMenu["Game Boy"];

            MenuButton colorList = current.AddSubMenu("Colors");
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

            temp = current.AddSubMenu("Buttons");
            temp.AddSwitch().OnDraw += (o, e) => { ((SwitchControl)o).UpdateSwitch(_onScreenButtonsBase.Visible); };
            temp.OnClick += (o, e) => { _onScreenButtonsBase.Visible = !_onScreenButtonsBase.Visible; };
            temp.ToolTip = "Show onscreen buttons";

            temp = mainMenu["Game Boy"].AddSubMenu("Running");
            temp.AddSwitch().OnDraw += (o, e) => { ((SwitchControl)o).UpdateSwitch(_gameboy.IsRunning); };
            temp.OnClick += (o, e) => { _gameboy.PauseToggle(this, EventArgs.Empty); };
            temp.ToolTip = "Pause/Unpause Game Boy";

            // display
            current = mainMenu["Display"];

            temp = current.AddSubMenu("Resolution");
            temp.AddSubMenu("800x720").OnClick += (o, e) => {
                Game1._Graphics.PreferredBackBufferWidth = 800;
                Game1._Graphics.PreferredBackBufferHeight = 720;
                Game1._Graphics.ApplyChanges();
            };
            temp.AddSubMenu("1280x720").OnClick += (o, e) => {
                Game1._Graphics.PreferredBackBufferWidth = 1280;
                Game1._Graphics.PreferredBackBufferHeight = 720;
                Game1._Graphics.ApplyChanges();
            };
            temp.AddSubMenu("1920x1080").OnClick += (o, e) => {
                Game1._Graphics.PreferredBackBufferWidth = 1920;
                Game1._Graphics.PreferredBackBufferHeight = 1080;
                Game1._Graphics.ApplyChanges();
            };

            temp = current.AddSubMenu("Full");
            temp.AddSwitch().OnDraw += (o, e) => { ((SwitchControl)o).UpdateSwitch(Game1._Graphics.IsFullScreen); };
            temp.OnClick += fullscreenHandler;
            temp.ToolTip = "Toggle fullscreen mode";

            temp = current.AddSubMenu("VSync");
            temp.AddSwitch().OnDraw += (o, e) => { ((SwitchControl)o).UpdateSwitch(Game1._Graphics.SynchronizeWithVerticalRetrace); };
            temp.OnClick += vsyncHandler;
            temp.ToolTip = "Toggle VSync";

            // Sound
            current = mainMenu["Sound"];

            MenuButton volume = current.AddSubMenu("Volume");
            volume.OnDraw += (o, e) => { ((MenuButton)o).Label.Caption = "Volume: " + _volumeList[_volumeIndex].ToString("0%"); };

            for (int i = 0; i < _volumeList.Count(); i++)
            {
                temp = volume.AddSubMenu(_volumeList[i].ToString("0%"));
                temp.Height = 40;
                temp.Label.HorizontalAlign = Align.Center;
                temp.ButtonData = i;
                temp.OnClick += (o, e) => { VolumeIndex = ((MenuButton)o).ButtonData; };
            }

            temp = current.AddSubMenu("Volume up", "volplus");
            temp.OnClick += (o, e) => { VolumeIndex++; };

            temp = current.AddSubMenu("Volume down", "volminus");
            temp.OnClick += (o, e) => { VolumeIndex--; };

            // Debug
            current = mainMenu["Debug"];

            MenuButton layers = current.AddSubMenu("Layers");

            MenuButton draw = layers.AddSubMenu("Draw");
            temp = draw.AddSubMenu("Background");
            temp.AddSwitch().OnDraw += (o, e) => { ((SwitchControl)o).UpdateSwitch(_drawBackground); };
            temp.OnClick += (o, e) => { _drawBackground = !_drawBackground; };
            temp = draw.AddSubMenu("Window");
            temp.AddSwitch().OnDraw += (o, e) => { ((SwitchControl)o).UpdateSwitch(_drawWindow); };
            temp.OnClick += (o, e) => { _drawWindow = !_drawWindow; };
            temp = draw.AddSubMenu("Sprites");
            temp.AddSwitch().OnDraw += (o, e) => { ((SwitchControl)o).UpdateSwitch(_drawSprites); };
            temp.OnClick += (o, e) => { _drawSprites = !_drawSprites; };

            MenuButton mark = layers.AddSubMenu("Mark");
            temp = mark.AddSubMenu("Background");
            temp.AddSwitch().OnDraw += (o, e) => { ((SwitchControl)o).UpdateSwitch(_markBackground); };
            temp.OnClick += (o, e) => { _markBackground = !_markBackground; };
            temp = mark.AddSubMenu("Window");
            temp.AddSwitch().OnDraw += (o, e) => { ((SwitchControl)o).UpdateSwitch(_markWindow); };
            temp.OnClick += (o, e) => { _markWindow = !_markWindow; };
            temp = mark.AddSubMenu("Sprites");
            temp.AddSwitch().OnDraw += (o, e) => { ((SwitchControl)o).UpdateSwitch(_markSprites); };
            temp.OnClick += (o, e) => { _markSprites = !_markSprites; };


            temp = current.AddSubMenu("FPS");
            temp.AddSwitch().OnDraw += (o, e) => { ((SwitchControl)o).UpdateSwitch(_fps.Visible); };
            temp.OnClick += (o, e) => { _fps.Visible = !_fps.Visible; };
            temp.ToolTip = "Show FPS";

            MenuButton audio = current.AddSubMenu("Audio");
            audio.AddSwitch().OnDraw += (o, e) => { ((SwitchControl)o).UpdateSwitch(_audioBar.Panel.Visible); };
            audio.OnClick += (o, e) => { _audioBar.Panel.Visible = !_audioBar.Panel.Visible; };
            audio.ToolTip = "Show audio sidebar";
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

            temp = current.AddSubMenu("Grid");
            temp.AddSwitch().OnDraw += (o, e) => { ((SwitchControl)o).UpdateSwitch(_showGrid); };
            temp.OnClick += (o, e) => { _showGrid = !_showGrid; };
            temp.ToolTip = "Show pixel grid";

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
            audioMenu.OnClose += setFocusToNull;
            audioMenu.OnDraw += (o, e) => {
                audioMenu.Top = _screenTop + _screenHeight / 2 - (7 * 60 / 2);
                audioMenu.Left = -audioMenu.Width;
            };


            // vol %
            current = audioMenu.AddSubMenu("Volume", null, 60);
            current.Label.HorizontalAlign = Align.Center;
            current.OnDraw += (o, e) => { ((MenuButton)o).Label.Caption = _volumeList[_volumeIndex].ToString("0%"); };
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

        // Draw Methods
        private void drawEmulator(Viewport viewport, Texture2D[] screens)
        {
            
            // Screen Position & Size
            float pixelSize = MathHelper.Min(viewport.Height / 144f,
                                             viewport.Width / 160f);

            _screenWidth = (int)pixelSize * 160;
            _screenHeight = (int)pixelSize * 144;
            _screenLeft = (viewport.Width - _screenWidth) / 2;
            _screenTop = (viewport.Height - _screenHeight) / 2;

            // Draw Screen
            if (_drawBackground) _spriteBatch.Draw(screens[0], new Rectangle(_screenLeft, _screenTop, _screenWidth, _screenHeight), _markBackground ? _pixelMarkerTextColor : Color.White);
            if (_drawWindow) _spriteBatch.Draw(screens[1], new Rectangle(_screenLeft, _screenTop, _screenWidth, _screenHeight), _markWindow ? _pixelMarkerTextColor : Color.White);
            if (_drawSprites) _spriteBatch.Draw(screens[2], new Rectangle(_screenLeft, _screenTop, _screenWidth, _screenHeight), _markSprites ? _pixelMarkerTextColor : Color.White);

            // Draw Grid
            if (_showGrid)
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

            foreach (BaseControl control in _controls)
            {
                control.Draw(_spriteBatch);
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
