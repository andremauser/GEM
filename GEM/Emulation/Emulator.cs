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

        // buttonset bases
        BaseControl _onScreenButtonsBase;
        BaseControl _debugInformationsBase;
        BaseControl _audioIconsBase;

        // fields
        List<string> _romList = new List<string>();
        List<BaseControl> _controls;
        float[] _volumeList;
        MenuButton _menu;
        MenuButton _toolTip;
        MenuButton _dpadUp;
        MenuButton _dpadDown;
        MenuButton _dpadLeft;
        MenuButton _dpadRight;
        MenuButton _btnA;
        MenuButton _btnB;
        MenuButton _btnStart;
        MenuButton _btnSelect;
        MenuButton _volMinus;
        MenuButton _vol;
        MenuButton _volPlus;
        MenuButton[] _volChannel = new MenuButton[4];
        MenuButton _fps;
        int _screenWidth;
        int _screenHeight;
        int _screenLeft;
        int _screenTop;
        bool _writeRAM = false;

        #endregion

        #region Constructors
        public Emulator(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _controls = new List<BaseControl>();
            _volumeList = new float[] { 0f, 0.01f, 0.05f, 0.1f, 0.25f, 0.5f, 0.75f, 1f };
            Cartridge.OnWriteRAM += (o, e) => { _writeRAM = true; };
        }
        #endregion

        #region Properties
        public string CartridgeTitle
        {
            get
            {
                string title = _gameboy.CartridgeTitle;
                if (title != "")
                {
                    return title;
                }
                return "(N/A)";
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
                _volumeIndex = Math.Clamp(value, 0, _volumeList.Length-1);
                // update volume
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

                // update ui colors
                Color light = Color.White;
                Color lightColor = new Color(_emuPalette[_emuColorIndex][1], 1f);
                Color darkColor = new Color(_emuPalette[_emuColorIndex][2], 1f);
                Color dark = new Color(_emuPalette[_emuColorIndex][3], 0.9f);
                if (_menu != null)
                {
                    _menu.SetStateColorsR(State.Disabled, dark, Color.Gray);
                    _menu.SetStateColorsR(State.Idle, dark, light);
                    _menu.SetStateColorsR(State.Hover, darkColor, light);
                    _menu.SetStateColorsR(State.Press, lightColor, light);
                    _menu.BackColor[State.Idle] = Color.Transparent;
                    _menu.ForeColor[State.Idle] = darkColor;
                }
                if (_toolTip != null)
                {
                    _toolTip.SetStateColorsR(State.Disabled, dark, light);
                }
                if (_onScreenButtonsBase != null)
                {
                    MenuButton[] onscreenButtons = new MenuButton[]
                    {
                        _dpadUp, _dpadDown, _dpadLeft, _dpadRight, _btnA, _btnB, _btnStart, _btnSelect
                    };
                    foreach (MenuButton btn in onscreenButtons)
                    {
                        btn.SetStateColorsR(State.Disabled, Color.Transparent, darkColor);
                        btn.SetStateColorsR(State.Idle, Color.Transparent, darkColor);
                        btn.SetStateColorsR(State.Hover, Color.Transparent, darkColor);
                        btn.SetStateColorsR(State.Press, Color.Transparent, darkColor);
                    }
                }
                if (_fps != null)
                {
                    _fps.SetStateColorsR(State.Disabled, dark, light);
                    _fps.SetStateColorsR(State.Idle, dark, light);
                    _fps.SetStateColorsR(State.Hover, darkColor, light);
                    _fps.SetStateColorsR(State.Press, lightColor, light);
                }
                if (_debugInformationsBase != null)
                {
                    _vol.SetStateColorsR(State.Disabled, dark, light);
                    _vol.SetStateColorsR(State.Idle, dark, light);
                    _vol.SetStateColorsR(State.Hover, darkColor, light);
                    _vol.SetStateColorsR(State.Press, lightColor, light);

                    _volPlus.SetStateColorsR(State.Disabled, dark, light);
                    _volPlus.SetStateColorsR(State.Idle, dark, light);
                    _volPlus.SetStateColorsR(State.Hover, darkColor, light);
                    _volPlus.SetStateColorsR(State.Press, lightColor, light);

                    _volMinus.SetStateColorsR(State.Disabled, dark, light);
                    _volMinus.SetStateColorsR(State.Idle, dark, light);
                    _volMinus.SetStateColorsR(State.Hover, darkColor, light);
                    _volMinus.SetStateColorsR(State.Press, lightColor, light);

                    for (int i = 0; i < 4; i++)
                    {
                        _volChannel[i].SetStateColorsR(State.Disabled, dark, light);
                        _volChannel[i].SetStateColorsR(State.Idle, dark, light);
                        _volChannel[i].SetStateColorsR(State.Hover, darkColor, light);
                        _volChannel[i].SetStateColorsR(State.Press, lightColor, light);
                    }
                }
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
            updateRomListHandler(null, EventArgs.Empty); // initial call to rom search - updated by click on "Open ROM"

            MenuButton.OnFocusChange += (o, e) => {
                if (MenuButton.Focus == null)
                {
                    _onScreenButtonsBase.Enabled = true;
                    _audioIconsBase.Enabled = true;
                }
                else
                {
                    _onScreenButtonsBase.Enabled = false;
                    _audioIconsBase.Enabled = false;
                }
            };

            _toolTip = new MenuButton(null, null, "", MenuType.StandAlone) { Enabled = false, Height = 40};
            _toolTip.Label.Padding = 15;
            _toolTip.Label.HorizontalAlign = Align.Left;
            _toolTip.OnDraw += (o, e) =>
            {
                // update tooltip line
                _toolTip.Label.Caption = MenuButton.Focus != null ? MenuButton.Focus.ToolTip : "";
                _toolTip.Width = _toolTip.Label.Caption == "" ? 0 : _toolTip.Label.Width + 2 * _toolTip.Label.Padding;
                _toolTip.Top = Game1._Graphics.GraphicsDevice.Viewport.Height - _toolTip.Height;
            };
            _controls.Add(_toolTip);

            MenuButton temp;

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
            _dpadUp = new MenuButton(_dpad, null, "up", MenuType.StandAlone, "dpad", 4) { Width = 100, Height = 100 };
            _dpadUp.Left = -50;
            _dpadUp.Top = -120;
            _dpadUp.Image.ResizeToParent();
            _dpadUp.Image.SetRotation(90);
            _dpadUp.KeyBinding = Keys.Up;
            _dpadUp.BtnBinding = Buttons.DPadUp;
            _dpadUp.OnPress += (o, e) => _gameboy.IsButton_Up = true;
            _dpadUp.OnRelease += (o, e) => _gameboy.IsButton_Up = false;
            _dpad.Add(_dpadUp);

            // down
            _dpadDown = new MenuButton(_dpad, null, "down", MenuType.StandAlone, "dpad", 4) { Width = 100, Height = 100 };
            _dpadDown.Left = -50;
            _dpadDown.Top = 20;
            _dpadDown.Image.ResizeToParent();
            _dpadDown.Image.SetRotation(270);
            _dpadDown.KeyBinding = Keys.Down;
            _dpadDown.BtnBinding = Buttons.DPadDown;
            _dpadDown.OnPress += (o, e) => _gameboy.IsButton_Down = true;
            _dpadDown.OnRelease += (o, e) => _gameboy.IsButton_Down = false;
            _dpad.Add(_dpadDown);

            // right
            _dpadRight = new MenuButton(_dpad, null, "->", MenuType.StandAlone, "dpad", 4) { Width = 100, Height = 100 };
            _dpadRight.Left = 20;
            _dpadRight.Top = -50;
            _dpadRight.Image.ResizeToParent();
            _dpadRight.KeyBinding = Keys.Right;
            _dpadRight.BtnBinding = Buttons.DPadRight;
            _dpadRight.OnPress += (o, e) => _gameboy.IsButton_Right = true;
            _dpadRight.OnRelease += (o, e) => _gameboy.IsButton_Right = false;
            _dpad.Add(_dpadRight);

            // left
            _dpadLeft = new MenuButton(_dpad, null, "<-", MenuType.StandAlone, "dpad", 4) { Width = 100, Height = 100 };
            _dpadLeft.Left = -120;
            _dpadLeft.Top = -50;
            _dpadLeft.Image.ResizeToParent();
            _dpadLeft.Image.SetRotation(180);
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
            _btnA = new MenuButton(_btns, null, "A", MenuType.StandAlone, "btna", 4) { Width = 100, Height = 100 };
            _btnA.Left = 0;
            _btnA.Top = -70;
            _btnA.Image.ResizeToParent();
            _btnA.Image.SetRotation(20);
            _btnA.KeyBinding = Keys.X;
            _btnA.BtnBinding = Buttons.B;
            _btnA.OnPress += (o, e) => _gameboy.IsButton_A = true;
            _btnA.OnRelease += (o, e) => _gameboy.IsButton_A = false;
            _btns.Add(_btnA);

            // B
            _btnB = new MenuButton(_btns, null, "B", MenuType.StandAlone, "btnb", 4) { Width = 100, Height = 100 };
            _btnB.Left = -120;
            _btnB.Top = -30;
            _btnB.Image.ResizeToParent();
            _btnB.Image.SetRotation(20);
            _btnB.KeyBinding = Keys.Y;
            _btnB.BtnBinding = Buttons.A;
            _btnB.OnPress += (o, e) => _gameboy.IsButton_B = true;
            _btnB.OnRelease += (o, e) => _gameboy.IsButton_B = false;
            _btns.Add(_btnB);

            // start
            _btnStart = new MenuButton(null, null, "Start", MenuType.StandAlone, "stasel", 4) { Width = 100, Height = 100 };
            _btnStart.Left = 1040;
            _btnStart.Top = 620;
            _btnStart.Image.ResizeToParent();
            _btnStart.Image.SetRotation(20);
            _btnStart.KeyBinding = Keys.Enter;
            _btnStart.BtnBinding = Buttons.Start;
            _btnStart.OnPress += (o, e) => _gameboy.IsButton_Start = true;
            _btnStart.OnRelease += (o, e) => _gameboy.IsButton_Start = false;
            _btnStart.OnDraw += (o, e) =>
            {
                ((BaseControl)o).Top = _screenTop + _screenHeight - ((BaseControl)o).Height;
                ((BaseControl)o).Left = Math.Min(
                    _screenLeft + _screenWidth + 20, 
                    _btnB.PosX
                    );
            }; 
            _onScreenButtonsBase.Add(_btnStart);

            // select
            _btnSelect = new MenuButton(null, null, "Select", MenuType.StandAlone, "stasel", 4) { Width = 100, Height = 100 };
            _btnSelect.Image.ResizeToParent();
            _btnSelect.Image.SetRotation(20);
            _btnSelect.KeyBinding = Keys.Back;
            _btnSelect.BtnBinding = Buttons.Back;
            _btnSelect.OnPress += (o, e) => _gameboy.IsButton_Select = true;
            _btnSelect.OnRelease += (o, e) => _gameboy.IsButton_Select = false;
            _btnSelect.OnDraw += (o, e) =>
            {
                ((BaseControl)o).Top = _screenTop + _screenHeight - ((BaseControl)o).Height;
                ((BaseControl)o).Left = Math.Max(
                    _screenLeft - ((BaseControl)o).Width - 20, 
                    _dpadRight.PosX
                    );
            }; 
            _onScreenButtonsBase.Add(_btnSelect);

            #endregion

            #region debug infos

            _debugInformationsBase = new BaseControl(null);
            _controls.Add(_debugInformationsBase);

            // fps
            _fps = new MenuButton(_debugInformationsBase, null, "fps", MenuType.Click) { Width = 60, Height = 60 };
            _fps.KeyBinding = Keys.RightControl;
            _fps.BtnBinding = Buttons.RightShoulder;
            _fps.ToolTip = "Current Frame Rate";
            _fps.Visible = false;
            _fps.Label.HorizontalAlign = Align.Center;
            _fps.OnDraw += (o, e) => {
                ((MenuButton)o).Label.Caption = Game1._Instance.FPS.ToString();
                ((BaseControl)o).Left = Game1._Graphics.GraphicsDevice.Viewport.Width - ((BaseControl)o).Width;
                ((BaseControl)o).Top = 0;
            };
            _debugInformationsBase.Add(_fps);
            _fps.Panel.HorizontalAlign = Align.Left;
            _fps.Panel.VerticalAlign = Align.Bottom;
            _fps.Panel.Left = -200 + 60;
            _fps.OnOpen += (o, e) => { MenuButton.Focus = ((MenuButton)o).SubMenu.Values.ToArray<MenuButton>()[0]; MenuButton.Focus.Close(o, e); };
            _fps.OnClose += (o, e) => { MenuButton.Focus = null; };
            Label label;
            temp = _fps.AddClickMenu("update");
            temp.Width = 200;
            temp.ToolTip = "Input / UI";
            temp.OnDraw += (o, e) => { ((MenuButton)o).Label.Caption = String.Format("{0:0 %}", _timespanUpdate * Game1.FRAME_RATE / 1000); };
            temp.Label.HorizontalAlign = Align.Right;
            label = temp.AddLabel(temp.ToolTip + ":");
            label.Padding = 15;
            label.HorizontalAlign = Align.Left;
            temp = _fps.AddClickMenu("emulation");
            temp.Width = 200;
            temp.ToolTip = "Emulation";
            temp.OnDraw += (o, e) => { ((MenuButton)o).Label.Caption = String.Format("{0:0 %}", _timespanEmulation * Game1.FRAME_RATE / 1000); };
            temp.Label.HorizontalAlign = Align.Right;
            label = temp.AddLabel(temp.ToolTip + ":");
            label.Padding = 15;
            label.HorizontalAlign = Align.Left;
            temp = _fps.AddClickMenu("draw");
            temp.Width = 200;
            temp.ToolTip = "Rendering";
            temp.OnDraw += (o, e) => { ((MenuButton)o).Label.Caption = String.Format("{0:0 %}", _timespanDraw * Game1.FRAME_RATE / 1000); };
            temp.Label.HorizontalAlign = Align.Right;
            label = temp.AddLabel(temp.ToolTip + ":");
            label.Padding = 15;
            label.HorizontalAlign = Align.Left;


            // sound channel icons
            _audioIconsBase = new BaseControl(null);
            _audioIconsBase.Visible = false;
            _audioIconsBase.OnDraw += (o, e) => { 
                _audioIconsBase.Top = _screenTop + _screenHeight / 2 - (7 * 60 / 2);
                _audioIconsBase.Left = 0;
            };
            _debugInformationsBase.Add(_audioIconsBase);
            // vol %
            _vol = new MenuButton(_debugInformationsBase, null, "vol", MenuType.Click) { Width = 60, Height = 60, Left = 0, Top = 0 };
            _vol.Label.HorizontalAlign = Align.Center;
            _vol.OnDraw += (o, e) => { ((MenuButton)o).Label.Caption = _volumeList[_volumeIndex].ToString("0%"); };
            _audioIconsBase.Add(_vol);

            for (int i = 0; i < _volumeList.Count(); i++)
            {
                temp = _vol.AddClickMenu(_volumeList[i].ToString("0%"));
                temp.Label.HorizontalAlign = Align.Center;
                temp.Height = 40;
                temp.ButtonData = i;
                temp.OnClick += (o, e) => { VolumeIndex = ((MenuButton)o).ButtonData; };
            }
            // vol plus
            _volPlus = new MenuButton(_audioIconsBase, null, "volplus", MenuType.StandAlone, "volplus", 1) { Width = 60, Height = 60, Left = 0, Top = 60 };
            _volPlus.Image.ResizeToParent();
            _volPlus.OnClick += (o, e) => { VolumeIndex++; };
            _audioIconsBase.Add(_volPlus);
            // vol minus
            _volMinus = new MenuButton(_audioIconsBase, null, "volminus", MenuType.StandAlone, "volminus", 1) { Width = 60, Height = 60, Left = 0, Top = 120 };
            _volMinus.Image.ResizeToParent();
            _volMinus.OnClick += (o, e) => { VolumeIndex--; };
            _audioIconsBase.Add(_volMinus);
            // channels
            for (int i = 0; i < 4; i++)
            {
                _volChannel[i] = new MenuButton(_audioIconsBase, null, "CH" + (i+1).ToString(), MenuType.StandAlone, "sound", 3) { Width = 60, Height = 60, Left = 0 };
                _volChannel[i].Top = (i + 3) * _volChannel[i].Height;
                _volChannel[i].Image.ResizeToParent();
                _volChannel[i].ButtonData = i;
                _volChannel[i].OnClick += audioSwitchHandler;
                _volChannel[i].OnDraw += audioIconsHandler;
                _audioIconsBase.Add(_volChannel[i]);
            }

            #endregion

            #region window buttons
            /*
            // maximize window
            temp = new MenuButton(image: "max") { Width = 60, Height = 60 };
            temp.Image.ResizeToParent();
            temp.Left = 1160;
            temp.BackColor[State.Idle] = Color.Transparent;
            temp.ForeColor[State.Idle] = _menuColor;
            temp.OnClick += fullscreenHandler;
            _controls.Add(temp);

            // quit emulator
            temp = new MenuButton(image: "Quit") { Width = 60, Height = 60 };
            temp.Image.ResizeToParent();
            temp.Left = 1220;
            temp.BackColor[State.Idle] = Color.Transparent;
            temp.ForeColor[State.Idle] = _menuColor;
            temp.OnClick += exitHandler;
            _controls.Add(temp);
            */
            #endregion

            #region menu
            // menu
            _menu = new MenuButton(image: "menu", menuType: MenuType.Click) { Width = 60, Height = 60 };
            _menu.Image.ResizeToParent();
            _menu.Panel.HorizontalAlign = Align.Left;
            _menu.Panel.VerticalAlign = Align.Bottom;
            _menu.KeyBinding = Keys.LeftControl;
            _menu.BtnBinding = Buttons.LeftShoulder;
            _menu.OnOpen += (o, e) => { MenuButton.Focus = ((MenuButton)o).SubMenu.Values.ToArray<MenuButton>()[0]; MenuButton.Focus.Close(o, e); };
            _menu.OnClose += (o, e) => { MenuButton.Focus = null; };
            _controls.Add(_menu);
            // add menu entries
            _menu.AddClickMenu("ROM");
            _menu.AddClickMenu("Game Boy");
            _menu.AddClickMenu("Display");
            _menu.AddClickMenu("Sound");
            _menu.AddClickMenu("Debug");
            _menu.AddClickMenu("Quit");

            // ROM
            MenuButton romBrowser = _menu["ROM"].AddClickMenu("Open ROM");
            romBrowser.OnOpen += updateRomListHandler;
            romBrowser.OnOpen += (o, e) => { fillOpenDialog(romBrowser); };

            temp = _menu["ROM"]["Open ROM"].AddClickMenu("up", "arrow", 300, 40);
            temp.OnClick += (o, e) => 
            { 
                _openStartIndex -= OPEN_ENTRIES; 
                fillOpenDialog(_menu["ROM"]["Open ROM"]); 
                MenuButton.Focus = _menu["ROM"]["Open ROM"].SubMenu.Values.ToArray<MenuButton>()[OPEN_ENTRIES]; 
            };
            temp.Image.SetRotation(90);
            temp.ToolTip = "Scroll up";
            for (int i = 0; i < OPEN_ENTRIES; i++)
            {
                _menu["ROM"]["Open ROM"].AddClickMenu(i.ToString(), null, 300, 40).OnClick += openRomHandler; // add empty entry dummies
            }
            temp = _menu["ROM"]["Open ROM"].AddClickMenu("down", "arrow", 300, 40);
            temp.OnClick += (o, e) => { _openStartIndex += OPEN_ENTRIES; fillOpenDialog(_menu["ROM"]["Open ROM"]); MenuButton.Focus = _menu["ROM"]["Open ROM"].SubMenu.Values.ToArray<MenuButton>()[1]; };
            temp.Image.SetRotation(-90);
            temp.ToolTip = "Scroll down";
            _menu["ROM"].AddClickMenu("Reset ROM").OnClick += _gameboy.Reset;
            _menu["ROM"].AddClickMenu("Exit ROM").OnClick += _gameboy.EjectCartridge;

            // Game Boy
            _menu["Game Boy"].AddClickMenu("Colors");
            for (int i = 0; i < _emuPalette.Count(); i++)
            {
                temp = _menu["Game Boy"]["Colors"].AddClickMenu("color" + i.ToString());
                temp.Label.Caption = "";
                if (_emuPaletteNames.Count() > i) temp.ToolTip = _emuPaletteNames[i];
                Panel colorPanel = temp.AddPanel();
                colorPanel.Direction = Direction.Horizontal;
                for (int j = 0; j < 4; j++)
                {
                    Label tile = colorPanel.AddLabel(j.ToString());
                    tile.Caption = "";
                    tile.Width = 30;
                    tile.Height = 30;
                    tile.MarkColor = _emuPalette[i][j];
                }
                colorPanel.UpdateAlignPosition();
                temp.ButtonData = i; // set button data to color index
                temp.OnClick += setPaletteButtonHandler;
            }

            temp = _menu["Game Boy"].AddClickMenu("Buttons");
            temp.AddSwitch().OnDraw += (o, e) => { ((SwitchControl)o).SetSwitch(_onScreenButtonsBase.Visible); };
            temp.ToolTip = "Show onscreen buttons";
            temp.OnClick += (o, e) => { _onScreenButtonsBase.Visible = !_onScreenButtonsBase.Visible; };

            temp = _menu["Game Boy"].AddClickMenu("Running");
            temp.AddSwitch().OnDraw += (o, e) => { ((SwitchControl)o).SetSwitch(_gameboy.IsRunning); };
            temp.OnClick += (o, e) => { _gameboy.PauseToggle(this, EventArgs.Empty); };
            temp.ToolTip = "Pause/Unpause Game Boy";

            // display

            temp = _menu["Display"].AddClickMenu("Resolution");
            temp = _menu["Display"]["Resolution"].AddClickMenu("800x720");
            temp.OnClick += (o, e) =>
            {
                Game1._Graphics.PreferredBackBufferWidth = 800;
                Game1._Graphics.PreferredBackBufferHeight = 720;
                Game1._Graphics.ApplyChanges();
            };
            temp = _menu["Display"]["Resolution"].AddClickMenu("1280x720");
            temp.OnClick += (o, e) =>
            {
                Game1._Graphics.PreferredBackBufferWidth = 1280;
                Game1._Graphics.PreferredBackBufferHeight = 720;
                Game1._Graphics.ApplyChanges();
            };
            temp = _menu["Display"]["Resolution"].AddClickMenu("1920x1080");
            temp.OnClick += (o, e) =>
            {
                Game1._Graphics.PreferredBackBufferWidth = 1920;
                Game1._Graphics.PreferredBackBufferHeight = 1080;
                Game1._Graphics.ApplyChanges();
            };

            temp = _menu["Display"].AddClickMenu("Full");
            temp.AddSwitch().OnDraw += (o, e) => { ((SwitchControl)o).SetSwitch(Game1._Graphics.IsFullScreen); };
            temp.ToolTip = "Toggle fullscreen mode";
            temp.OnClick += fullscreenHandler;

            temp = _menu["Display"].AddClickMenu("VSync");
            temp.AddSwitch().OnDraw += (o, e) => { ((SwitchControl)o).SetSwitch(Game1._Graphics.SynchronizeWithVerticalRetrace); };
            temp.ToolTip = "Toggle VSync";
            temp.OnClick += vsyncHandler;

            // Sound
            _menu["Sound"].AddClickMenu("Volume").OnDraw += (o, e) => { ((MenuButton)o).Label.Caption = "Volume: " + _volumeList[_volumeIndex].ToString("0%"); };

            for (int i = 0; i < _volumeList.Count(); i++)
            {
                temp = _menu["Sound"]["Volume"].AddClickMenu(_volumeList[i].ToString("0%"));
                temp.Height = 40;
                temp.Label.HorizontalAlign = Align.Center;
                temp.ButtonData = i;
                temp.OnClick += (o, e) => { VolumeIndex = ((MenuButton)o).ButtonData; };
            }
            
            temp = _menu["Sound"].AddClickMenu("vol +", "volplus");
            temp.OnClick += (o, e) => { VolumeIndex++; };
            temp.ToolTip = "Volume up";

            temp = _menu["Sound"].AddClickMenu("vol -", "volminus");
            temp.OnClick += (o, e) => { VolumeIndex--; };
            temp.ToolTip = "Volume down";

            // Debug

            _menu["Debug"].AddClickMenu("Layers");

            _menu["Debug"]["Layers"].AddClickMenu("Draw");

            temp = _menu["Debug"]["Layers"]["Draw"].AddClickMenu("Background");
            temp.AddSwitch().OnDraw += (o, e) => { ((SwitchControl)o).SetSwitch(_drawBackground); };
            temp.OnClick += (o, e) => { _drawBackground = !_drawBackground; };
            temp = _menu["Debug"]["Layers"]["Draw"].AddClickMenu("Window");
            temp.AddSwitch().OnDraw += (o, e) => { ((SwitchControl)o).SetSwitch(_drawWindow); };
            temp.OnClick += (o, e) => { _drawWindow = !_drawWindow; };
            temp = _menu["Debug"]["Layers"]["Draw"].AddClickMenu("Sprites");
            temp.AddSwitch().OnDraw += (o, e) => { ((SwitchControl)o).SetSwitch(_drawSprites); };
            temp.OnClick += (o, e) => { _drawSprites = !_drawSprites; };

            _menu["Debug"]["Layers"].AddClickMenu("Mark");

            temp = _menu["Debug"]["Layers"]["Mark"].AddClickMenu("Background");
            temp.AddSwitch().OnDraw += (o, e) => { ((SwitchControl)o).SetSwitch(_markBackground); };
            temp.OnClick += (o, e) => { _markBackground = !_markBackground; };
            temp = _menu["Debug"]["Layers"]["Mark"].AddClickMenu("Window");
            temp.AddSwitch().OnDraw += (o, e) => { ((SwitchControl)o).SetSwitch(_markWindow); };
            temp.OnClick += (o, e) => { _markWindow = !_markWindow; };
            temp = _menu["Debug"]["Layers"]["Mark"].AddClickMenu("Sprites");
            temp.AddSwitch().OnDraw += (o, e) => { ((SwitchControl)o).SetSwitch(_markSprites); };
            temp.OnClick += (o, e) => { _markSprites = !_markSprites; };



            temp = _menu["Debug"].AddClickMenu("FPS");
            temp.AddSwitch().OnDraw += (o, e) => { ((SwitchControl)o).SetSwitch(_fps.Visible); };
            temp.ToolTip = "Show FPS";
            temp.OnClick += (o, e) => { _fps.Visible = !_fps.Visible; };


            temp = _menu["Debug"].AddClickMenu("Audio");
            temp.AddSwitch().OnDraw += (o, e) => { ((SwitchControl)o).SetSwitch(_audioIconsBase.Visible); };
            temp.ToolTip = "Show audio sidebar";
            temp.OnClick += (o, e) => { _audioIconsBase.Visible = !_audioIconsBase.Visible; };
            for (int i = 0; i < 4; i++)
            {
                temp = _menu["Debug"]["Audio"].AddClickMenu("CH" + (i + 1).ToString());
                temp.ButtonData = i;
                temp.AddSwitch().OnDraw += (o, e) => { ((SwitchControl)o).SetSwitch(_gameboy.MasterSwitch[((MenuButton)((SwitchControl)o).Parent).ButtonData]); };
                temp.OnClick += audioSwitchHandler;
            }
            _menu["Debug"]["Audio"]["CH1"].ToolTip = "Channel 1: Square wave 1";
            _menu["Debug"]["Audio"]["CH2"].ToolTip = "Channel 2: Square wave 2";
            _menu["Debug"]["Audio"]["CH3"].ToolTip = "Channel 3: Custom wave";
            _menu["Debug"]["Audio"]["CH4"].ToolTip = "Channel 4: Noise";

            temp = _menu["Debug"].AddClickMenu("Grid");
            temp.AddSwitch().OnDraw += (o, e) => { ((SwitchControl)o).SetSwitch(_showGrid); };
            temp.OnClick += (o, e) => { _showGrid = !_showGrid; };
            temp.ToolTip = "Show pixel grid";

            // quit
            _menu["Quit"].AddClickMenu("Quit GEM").OnClick += exitHandler;

            #endregion


            // initial color palette
            EmuColorIndex = 0;
        }

        public void Update(Viewport viewport)
        {
            DateTime start = DateTime.Now;

            // update input
            Input.Update();

            // update controls' states
            foreach (BaseControl control in _controls)
            {
                control.Update();
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
            if (_writeRAM)
            {
                _gameboy.SaveRAM();
                _writeRAM = false;
            }

            DateTime end = DateTime.Now;
            _timespanEmulation = (afterEmulation - start).TotalMilliseconds;
            _timespanDraw = (end - afterEmulation).TotalMilliseconds;
        }


        // Private Helper Methods
        private void fillOpenDialog(MenuButton parent)
        {
            parent["up"].Enabled = (_openStartIndex - OPEN_ENTRIES) >= 0;
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
                    parent[i.ToString()].Label.Left = 20;
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
            parent["down"].Enabled = (_openStartIndex + OPEN_ENTRIES) < _romList.Count;
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
        private void updateRomListHandler(object sender, EventArgs e)
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
                    ((MenuButton)sender).ForeColor[State.Idle] = _emuPalette[EmuColorIndex][1];
                    ((MenuButton)sender).ForeColor[State.Disabled] = _emuPalette[EmuColorIndex][1];
                }
                else
                {
                    ((MenuButton)sender).ForeColor[State.Idle] = _emuPalette[EmuColorIndex][2];
                    ((MenuButton)sender).ForeColor[State.Disabled] = _emuPalette[EmuColorIndex][2];
                }
            }
            else
            {
                // Masterswitch OFF
                ((MenuButton)sender).Image.ImageIndex = 0;
                ((MenuButton)sender).ForeColor[State.Idle] = _emuPalette[EmuColorIndex][2];
                ((MenuButton)sender).ForeColor[State.Disabled] = _emuPalette[EmuColorIndex][2];
            }
        }
#endregion
    }
}
