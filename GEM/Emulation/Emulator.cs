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
        Color _menuColor = new Color(0.1f, 0.1f, 0.1f);
        Color _gridColorDark;
        Color _gridColorLight;
        Color _pixelMarkerTextColor;
        Color _pixelMarkerColor;

        // states
        int _emuColorIndex;
        bool _debugShowAudioIndicators;
        int _volumeIndex = 0;
        int _openStartIndex = 0;
        bool _showGrid = false;

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
        int _screenWidth;
        int _screenHeight;
        int _screenLeft;
        int _screenTop;

        Random _random = new Random();
        #endregion

        #region Constructors
        public Emulator(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _controls = new List<BaseControl>();
            _volumeList = new float[] { 0f, 0.1f, 0.25f, 0.5f, 0.75f, 1f };
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
            _emuColorIndex = _random.Next(_emuPalette.Length);
            _gridColorDark = new Color(0, 0, 0, 128);
            _gridColorLight = new Color(0, 0, 0, 32);
            _pixelMarkerTextColor = new Color(255, 0, 255, 255);
            _pixelMarkerColor = new Color(255, 0, 255, 255);
            updateRomListHandler(null, EventArgs.Empty); // initial call to rom search - updated by click on "open rom"

            _toolTip = new MenuButton(null, null, "", MenuType.StandAlone) { Enabled = false, Top = 680, Height = 40};
            _toolTip.Width = _toolTip.Label.Width;
            _toolTip.OnDraw += (o, e) =>
            {
                // update tooltip line
                _toolTip.Label.Caption = MenuButton.Focus != null ? MenuButton.Focus.ToolTip : "";
                _toolTip.Width = _toolTip.Label.Caption == "" ? 0 : _toolTip.Label.Width + 20;
                _toolTip.Top = Game1._Graphics.GraphicsDevice.Viewport.Height - _toolTip.Height;
            };
            _controls.Add(_toolTip);

            MenuButton temp;
            MenuButton btn;

            #region onscreen buttons

            _onScreenButtonsBase = new BaseControl(null);
            _onScreenButtonsBase.Visible = false; // hide OnScreenButtons on start
            _controls.Add(_onScreenButtonsBase);

            // dpad
            BaseControl _dpad = new BaseControl(null) { Left = 120, Top = 400 };
            _onScreenButtonsBase.Add(_dpad);
            _dpad.OnDraw += (o, e) => { ((BaseControl)o).Top = Game1._Graphics.GraphicsDevice.Viewport.Height / 2;  };

            // up
            btn = new MenuButton(_dpad, null, "up", MenuType.StandAlone, "dpad", 4) { Width = 100, Height = 100 };
            btn.Left = -50;
            btn.Top = -120;
            btn.Image.ResizeToParent();
            btn.Image.SetRotation(90);
            btn.KeyBinding = Keys.Up;
            btn.BtnBinding = Buttons.DPadUp;
            btn.OnPress += (o, e) => _gameboy.IsButton_Up = true;
            btn.OnRelease += (o, e) => _gameboy.IsButton_Up = false;
            btn.OnDraw += (o, e) => { ((MenuButton)o).SetButtonColors(Color.Transparent, _emuPalette[_emuColorIndex][2]); };
            _dpad.Add(btn);

            // down
            btn = new MenuButton(_dpad, null, "down", MenuType.StandAlone, "dpad", 4) { Width = 100, Height = 100 };
            btn.Left = -50;
            btn.Top = 20;
            btn.Image.ResizeToParent();
            btn.Image.SetRotation(270);
            btn.KeyBinding = Keys.Down;
            btn.BtnBinding = Buttons.DPadDown;
            btn.OnPress += (o, e) => _gameboy.IsButton_Down = true;
            btn.OnRelease += (o, e) => _gameboy.IsButton_Down = false;
            btn.OnDraw += (o, e) => { ((MenuButton)o).SetButtonColors(Color.Transparent, _emuPalette[_emuColorIndex][2]); };
            _dpad.Add(btn);

            // right
            btn = new MenuButton(_dpad, null, "->", MenuType.StandAlone, "dpad", 4) { Width = 100, Height = 100 };
            btn.Left = 20;
            btn.Top = -50;
            btn.Image.ResizeToParent();
            btn.KeyBinding = Keys.Right;
            btn.BtnBinding = Buttons.DPadRight;
            btn.OnPress += (o, e) => _gameboy.IsButton_Right = true;
            btn.OnRelease += (o, e) => _gameboy.IsButton_Right = false;
            btn.OnDraw += (o, e) => { ((MenuButton)o).SetButtonColors(Color.Transparent, _emuPalette[_emuColorIndex][2]); };
            _dpad.Add(btn);

            // left
            btn = new MenuButton(_dpad, null, "<-", MenuType.StandAlone, "dpad", 4) { Width = 100, Height = 100 };
            btn.Left = -120;
            btn.Top = -50;
            btn.Image.ResizeToParent();
            btn.Image.SetRotation(180);
            btn.KeyBinding = Keys.Left;
            btn.BtnBinding = Buttons.DPadLeft;
            btn.OnPress += (o, e) => _gameboy.IsButton_Left = true;
            btn.OnRelease += (o, e) => _gameboy.IsButton_Left = false;
            btn.OnDraw += (o, e) => { ((MenuButton)o).SetButtonColors(Color.Transparent, _emuPalette[_emuColorIndex][2]); };
            _dpad.Add(btn);


            // buttons A, B
            BaseControl _btns = new BaseControl(null) { Left = 1150, Top = 400 };
            _onScreenButtonsBase.Add(_btns);
            _btns.OnDraw += (o, e) => 
            { 
                ((BaseControl)o).Top = Game1._Graphics.GraphicsDevice.Viewport.Height / 2;
                ((BaseControl)o).Left = Game1._Graphics.GraphicsDevice.Viewport.Width - 120;
            };

            // A
            btn = new MenuButton(_btns, null, "A", MenuType.StandAlone, "btna", 4) { Width = 100, Height = 100 };
            btn.Left = 10;
            btn.Top = -70;
            btn.Image.ResizeToParent();
            btn.Image.SetRotation(20);
            btn.KeyBinding = Keys.X;
            btn.BtnBinding = Buttons.B;
            btn.OnPress += (o, e) => _gameboy.IsButton_A = true;
            btn.OnRelease += (o, e) => _gameboy.IsButton_A = false;
            btn.OnDraw += (o, e) => { ((MenuButton)o).SetButtonColors(Color.Transparent, _emuPalette[_emuColorIndex][2]); };
            _btns.Add(btn);

            // B
            btn = new MenuButton(_btns, null, "B", MenuType.StandAlone, "btnb", 4) { Width = 100, Height = 100 };
            btn.Left = -110;
            btn.Top = -30;
            btn.Image.ResizeToParent();
            btn.Image.SetRotation(20);
            btn.KeyBinding = Keys.Y;
            btn.BtnBinding = Buttons.A;
            btn.OnPress += (o, e) => _gameboy.IsButton_B = true;
            btn.OnRelease += (o, e) => _gameboy.IsButton_B = false;
            btn.OnDraw += (o, e) => { ((MenuButton)o).SetButtonColors(Color.Transparent, _emuPalette[_emuColorIndex][2]); };
            _btns.Add(btn);

            // start
            btn = new MenuButton(null, null, "Start", MenuType.StandAlone, "stasel", 4) { Width = 100, Height = 100 };
            btn.Left = 1040;
            btn.Top = 620;
            btn.Image.ResizeToParent();
            btn.Image.SetRotation(20);
            btn.KeyBinding = Keys.Enter;
            btn.BtnBinding = Buttons.Start;
            btn.OnPress += (o, e) => _gameboy.IsButton_Start = true;
            btn.OnRelease += (o, e) => _gameboy.IsButton_Start = false;
            btn.OnDraw += (o, e) => { ((MenuButton)o).SetButtonColors(Color.Transparent, _emuPalette[_emuColorIndex][2]); };
            _onScreenButtonsBase.Add(btn);
            btn.OnDraw += (o, e) =>
            {
                ((BaseControl)o).Top = Game1._Graphics.GraphicsDevice.Viewport.Height - ((BaseControl)o).Height;
                ((BaseControl)o).Left = Game1._Graphics.GraphicsDevice.Viewport.Width - 240;
            };

            // select
            btn = new MenuButton(null, null, "Select", MenuType.StandAlone, "stasel", 4) { Width = 100, Height = 100 };
            btn.Left = 140;
            btn.Top = 620;
            btn.Image.ResizeToParent();
            btn.Image.SetRotation(20);
            btn.KeyBinding = Keys.Back;
            btn.BtnBinding = Buttons.Back;
            btn.OnPress += (o, e) => _gameboy.IsButton_Select = true;
            btn.OnRelease += (o, e) => _gameboy.IsButton_Select = false;
            btn.OnDraw += (o, e) => { ((MenuButton)o).SetButtonColors(Color.Transparent, _emuPalette[_emuColorIndex][2]); };
            _onScreenButtonsBase.Add(btn);
            btn.OnDraw += (o, e) =>
            {
                ((BaseControl)o).Top = Game1._Graphics.GraphicsDevice.Viewport.Height - ((BaseControl)o).Height;
            };

            #endregion

            #region debug infos

            _debugInformationsBase = new BaseControl(null);
            _controls.Add(_debugInformationsBase);


            // fps
            temp = new MenuButton(_debugInformationsBase, null, "fps", MenuType.StandAlone) { Width = 60, Height = 60 };
            temp.Left = 1040;
            temp.Top = 0;
            temp.BackColor[State.Idle] = Color.Transparent;
            temp.BackColor[State.Press] = temp.BackColor[State.Hover]; // disable press
            temp.OnDraw += (o, e) => { ((MenuButton)o).Label.Caption = Game1._Instance.FPS.ToString(); };
            temp.OnDraw += (o, e) => { ((BaseControl)o).Left = Game1._Graphics.GraphicsDevice.Viewport.Width - ((BaseControl)o).Width; };
            temp.OnDraw += (o, e) => { ((MenuButton)o).ForeColor[State.Idle] = _emuPalette[_emuColorIndex][2]; };
            _debugInformationsBase.Add(temp);


            // sound channel icons
            _audioIconsBase = new BaseControl(null);
            _debugInformationsBase.Add(_audioIconsBase);
            // vol %
            temp = new MenuButton(_debugInformationsBase, null, "vol", MenuType.StandAlone) { Width = 60, Height = 60 };
            temp.Left = 160;
            temp.Top = 0;
            temp.BackColor[State.Idle] = Color.Transparent;
            temp.BackColor[State.Disabled] = Color.Transparent;
            temp.BackColor[State.Press] = temp.BackColor[State.Hover]; // disable press
            temp.OnDraw += (o, e) => { ((MenuButton)o).Label.Caption = _volumeList[_volumeIndex].ToString("0%"); };
            temp.OnDraw += (o, e) =>
            {
                ((MenuButton)o).ForeColor[State.Idle] = _emuPalette[_emuColorIndex][2];
                ((MenuButton)o).ForeColor[State.Disabled] = _emuPalette[_emuColorIndex][2];
            };
            _audioIconsBase.Add(temp);
            // vol plus
            temp = new MenuButton(_audioIconsBase, null, "volplus", MenuType.StandAlone, "volplus", 1) { Width = 60, Height = 60 };
            temp.Left = 220;
            temp.Top = 0;
            temp.Image.ResizeToParent();
            temp.OnClick += (o, e) => { VolumeIndex++; };
            temp.ApplyDefaultColors();
            temp.BackColor[State.Idle] = Color.Transparent;
            temp.BackColor[State.Disabled] = Color.Transparent;
            temp.ForeColor[State.Idle] = _menuColor;
            temp.ForeColor[State.Disabled] = _menuColor;
            _audioIconsBase.Add(temp);
            temp.OnDraw += (o, e) =>
            {
                ((MenuButton)o).ForeColor[State.Idle] = _emuPalette[_emuColorIndex][2];
                ((MenuButton)o).ForeColor[State.Disabled] = _emuPalette[_emuColorIndex][2];
            };
            // vol minus
            temp = new MenuButton(_audioIconsBase, null, "volminus", MenuType.StandAlone, "volminus", 1) { Width = 60, Height = 60 };
            temp.Left = 100;
            temp.Top = 0;
            temp.Image.ResizeToParent();
            temp.OnClick += (o, e) => { VolumeIndex--; };
            temp.ApplyDefaultColors();
            temp.BackColor[State.Idle] = Color.Transparent;
            temp.BackColor[State.Disabled] = Color.Transparent;
            temp.ForeColor[State.Idle] = _menuColor;
            temp.ForeColor[State.Disabled] = _menuColor;
            _audioIconsBase.Add(temp);
            temp.OnDraw += (o, e) =>
            {
                ((MenuButton)o).ForeColor[State.Idle] = _emuPalette[_emuColorIndex][2];
                ((MenuButton)o).ForeColor[State.Disabled] = _emuPalette[_emuColorIndex][2];
            };
            // channels
            for (int i = 0; i < 4; i++)
            {
                temp = new MenuButton(_audioIconsBase, null, "CH" + (i+1).ToString(), MenuType.StandAlone, "sound", 3) { Width = 60, Height = 60 };
                //temp.Left = 180;
                //temp.Top = i * temp.Height;
                temp.Left = 320 + i * temp.Width;
                temp.Top = 0;
                temp.Image.ResizeToParent();
                temp.ButtonData = i;
                temp.OnClick += audioSwitchHandler;
                temp.ApplyDefaultColors();
                temp.BackColor[State.Idle] = Color.Transparent;
                temp.BackColor[State.Disabled] = Color.Transparent;
                temp.OnDraw += audioIconsHandler;
                _audioIconsBase.Add(temp);
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
            temp = new MenuButton(image: "quit") { Width = 60, Height = 60 };
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
            _menu.BackColor[State.Idle] = Color.Transparent;
            _menu.Panel.HorizontalAlign = Align.Left;
            _menu.Panel.VerticalAlign = Align.Bottom;
            _menu.KeyBinding = Keys.LeftControl;
            _menu.BtnBinding = Buttons.LeftShoulder;
            _menu.OnOpen += (o, e) => { _onScreenButtonsBase.Enabled = false; _audioIconsBase.Enabled = false; MenuButton.Focus = ((MenuButton)o).SubMenu.Values.ToArray<MenuButton>()[0]; MenuButton.Focus.Close(o, e); };
            _menu.OnClose += (o, e) => { _onScreenButtonsBase.Enabled = true; _audioIconsBase.Enabled = true; MenuButton.Focus = null; };
            _menu.ForeColor[State.Idle] = _menuColor;
            _controls.Add(_menu);
            _menu.OnDraw += (o, e) => { ((MenuButton)o).ForeColor[State.Idle] = _emuPalette[_emuColorIndex][2]; };
            // add menu entries
            _menu.AddClickMenu("game");
            _menu.AddClickMenu("graphics");
            _menu.AddClickMenu("audio");
            _menu.AddClickMenu("controls");
            _menu.AddClickMenu("quit");

            // cart
            MenuButton romBrowser = _menu["game"].AddClickMenu("open rom");
            romBrowser.OnOpen += updateRomListHandler;
            romBrowser.OnOpen += (o, e) => { fillOpenDialog(romBrowser); };
            temp = _menu["game"]["open rom"].AddClickMenu("up", "arrow", 300, 40);
            temp.OnClick += (o, e) => { _openStartIndex -= OPEN_ENTRIES; fillOpenDialog(_menu["game"]["open rom"]); MenuButton.Focus = _menu["game"]["open rom"].SubMenu.Values.ToArray<MenuButton>()[OPEN_ENTRIES]; };
            temp.Image.SetRotation(90);
            temp.ToolTip = "scroll up";
            for (int i = 0; i < OPEN_ENTRIES; i++)
            {
                _menu["game"]["open rom"].AddClickMenu(i.ToString(), null, 300, 40).OnClick += openRomHandler; // add empty entry dummies
            }
            temp = _menu["game"]["open rom"].AddClickMenu("down", "arrow", 300, 40);
            temp.OnClick += (o, e) => { _openStartIndex += OPEN_ENTRIES; fillOpenDialog(_menu["game"]["open rom"]); MenuButton.Focus = _menu["game"]["open rom"].SubMenu.Values.ToArray<MenuButton>()[1]; };
            temp.Image.SetRotation(-90);
            temp.ToolTip = "scroll down";
            _menu["game"].AddClickMenu("reset rom").OnClick += _gameboy.Reset;
            _menu["game"].AddClickMenu("exit rom").OnClick += _gameboy.EjectCartridge;
            // graphics
            _menu["graphics"].AddClickMenu("palette");
            for (int i = 0; i < _emuPalette.Count(); i++)
            {
                temp = _menu["graphics"]["palette"].AddClickMenu("color" + i.ToString());
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
            temp = _menu["graphics"].AddClickMenu("grid");
            temp.AddSwitch("grid").OnDraw += (o, e) => { ((SwitchControl)o).SetSwitch(_showGrid); };
            temp.ToolTip = "show pixel grid";
            temp.OnClick += (o, e) => { _showGrid = !_showGrid; };
            temp = _menu["graphics"].AddClickMenu("fullscreen");
            temp.AddSwitch("fscreen").OnDraw += (o, e) => { ((SwitchControl)o).SetSwitch(Game1._Graphics.IsFullScreen); };
            temp.ToolTip = "toggle fullscreen mode";
            temp.OnClick += fullscreenHandler;
            temp = _menu["graphics"].AddClickMenu("vsync");
            temp.AddSwitch("vsync").OnDraw += (o, e) => { ((SwitchControl)o).SetSwitch(Game1._Graphics.SynchronizeWithVerticalRetrace); };
            temp.ToolTip = "toggle vsync";
            temp.OnClick += vsyncHandler;
            // audio
            _menu["audio"].AddClickMenu("volume").OnDraw += (o, e) => { ((MenuButton)o).Label.Caption = "volume: " + _volumeList[_volumeIndex].ToString("0%"); };
            temp = _menu["audio"]["volume"].AddClickMenu("vol +", "volplus");
            temp.OnClick += (o, e) => { VolumeIndex++; };
            temp.ToolTip = "volume up";
            temp = _menu["audio"]["volume"].AddClickMenu("vol -", "volminus");
            temp.OnClick += (o, e) => { VolumeIndex--; };
            temp.ToolTip = "volume down";
            temp = _menu["audio"].AddClickMenu("show");
            temp.AddSwitch("icons").OnDraw += (o, e) => { ((SwitchControl)o).SetSwitch(_debugShowAudioIndicators); };
            temp.ToolTip = "show audio channel icons";
            temp.OnClick += (o, e) => { _debugShowAudioIndicators = !_debugShowAudioIndicators; };
            _menu["audio"].AddClickMenu("channels");
            for (int i = 0; i < 4; i++)
            {
                temp = _menu["audio"]["channels"].AddClickMenu("CH" + (i + 1).ToString());
                temp.ButtonData = i;
                temp.AddSwitch("ch" + (i + 1).ToString()).OnDraw += (o, e) => { ((SwitchControl)o).SetSwitch(_gameboy.MasterSwitch[((MenuButton)((SwitchControl)o).Parent).ButtonData]); };
                temp.OnClick += audioSwitchHandler;
            }
            _menu["audio"]["channels"]["CH1"].ToolTip = "channel 1: square wave 1";
            _menu["audio"]["channels"]["CH2"].ToolTip = "channel 2: square wave 2";
            _menu["audio"]["channels"]["CH3"].ToolTip = "channel 3: custom wave";
            _menu["audio"]["channels"]["CH4"].ToolTip = "channel 4: noise";
            // controls
            temp = _menu["controls"].AddClickMenu("buttons");
            temp.AddSwitch("buttons").OnDraw += (o, e) => { ((SwitchControl)o).SetSwitch(_onScreenButtonsBase.Visible); };
            temp.ToolTip = "show onscreen buttons";
            temp.OnClick += (o, e) => { _onScreenButtonsBase.Visible = !_onScreenButtonsBase.Visible; };

            // quit
            _menu["quit"].AddClickMenu("quit GEM").OnClick += exitHandler;

            #endregion

        }

        public void Update(Viewport viewport)
        {
            // update input
            Input.Update();

            // update controls' states
            foreach (BaseControl control in _controls)
            {
                control.Update();
            }
        }

        public void Draw(Viewport viewport)
        {
            // update emulator
            _gameboy.UpdateFrame();
            Texture2D gbScreen = _gameboy.GetScreen(_emuPalette[_emuColorIndex]);

            // draw emulator
            _spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);
            drawEmulator(viewport, gbScreen);
            _spriteBatch.End();
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
                parent["0"].Label.Caption = "no roms found";
                parent["0"].ToolTip = "put rom files in /roms folder";
            }
            parent["down"].Enabled = (_openStartIndex + OPEN_ENTRIES) < _romList.Count;
        }

        // Draw Methods
        private void drawEmulator(Viewport viewport, Texture2D screen)
        {
            // Screen Position & Size
            float pixelSize = MathHelper.Min(viewport.Height / 144f,
                                             viewport.Width / 160f);

            _screenWidth = (int)pixelSize * 160;
            _screenHeight = (int)pixelSize * 144;
            _screenLeft = (viewport.Width - _screenWidth) / 2;
            _screenTop = (viewport.Height - _screenHeight) / 2;

            // Draw Screen
            _spriteBatch.Draw(screen, new Rectangle(_screenLeft, _screenTop, _screenWidth, _screenHeight), Color.White);

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

            // update audio icon visibility
            _audioIconsBase.Visible = _debugShowAudioIndicators;

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
            //_spriteBatch.Draw(_pixel, new Rectangle(left, tilePosY, (int)(size*160), tileSize), _gridColorLight);
            //_spriteBatch.Draw(_pixel, new Rectangle(tilePosX, top, tileSize, (int)(size*144)), _gridColorLight);
            _spriteBatch.DrawString(_Font, tileX.ToString(), new Vector2(tilePosX, top), _pixelMarkerTextColor);
            _spriteBatch.DrawString(_Font, tileY.ToString(), new Vector2(left, tilePosY), _pixelMarkerTextColor);

            // Pixel Marker
            //_spriteBatch.Draw(_pixel, new Rectangle(left, pixelPosY, (int)(size * 160), pixelSize), _gridColorLight);
            //_spriteBatch.Draw(_pixel, new Rectangle(pixelPosX, top, pixelSize, (int)(size * 160)), _gridColorLight);
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
        private void drawWindow(int posX, int posY)
        {
            float scale = 1f;
            //_spriteBatch.DrawString(_Font, "Window:", new Vector2(posX, posY), _emuPalette[_emuColorIndex][1]);
            _spriteBatch.Draw(_gameboy.WindowTexture(_emuPalette[_emuColorIndex]), new Rectangle(posX, posY + 20, (int)(256 * scale), (int)(256 * scale)), Color.White);
        }
        private void drawBackground(int posX, int posY)
        {
            float scale = 1f;
            _spriteBatch.DrawString(_Font, "Background:", new Vector2(posX, posY), _emuPalette[_emuColorIndex][1]);
            _spriteBatch.Draw(_gameboy.BackgroundTexture(_emuPalette[_emuColorIndex]), new Rectangle(posX, posY + 20, (int)(256 * scale), (int)(256 * scale)), Color.White);
        }
        private void drawTileset(int posX, int posY)
        {
            float scale = 2f;
            _spriteBatch.DrawString(_Font, "Tileset:", new Vector2(posX, posY), _emuPalette[_emuColorIndex][1]);
            _spriteBatch.Draw(_gameboy.TilesetTexture(_emuPalette[_emuColorIndex]), new Rectangle(posX, posY + 20, (int)(128 * scale), (int)(192 * scale)), Color.White);
        }

        // Event Handler
        public void ShutDownHandler<EventArgs>(object sender, EventArgs e)
        {
            // Eventhandler to save game when closing window
            _gameboy.PowerOff();
        }
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
            _emuColorIndex = btn.ButtonData;
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
                    ((MenuButton)sender).ForeColor[State.Idle] = _emuPalette[_emuColorIndex][1];
                    ((MenuButton)sender).ForeColor[State.Disabled] = _emuPalette[_emuColorIndex][1];
                }
                else
                {
                    ((MenuButton)sender).ForeColor[State.Idle] = _emuPalette[_emuColorIndex][2];
                    ((MenuButton)sender).ForeColor[State.Disabled] = _emuPalette[_emuColorIndex][2];
                }
            }
            else
            {
                // Masterswitch OFF
                ((MenuButton)sender).Image.ImageIndex = 0;
                ((MenuButton)sender).ForeColor[State.Idle] = _emuPalette[_emuColorIndex][3];
                ((MenuButton)sender).ForeColor[State.Disabled] = _emuPalette[_emuColorIndex][3];
            }
        }
#endregion
    }
}
