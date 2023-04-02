using GEM.Menu;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static GEM.Menu.MenuButton;

namespace GEM.Emulation
{
    /// <summary>
    /// Connection between user and gameboy instance: 
    /// Handles user input and draws emulated Gameboy screen to window including debug informations
    /// </summary>
    internal class Emulator
    {
        #region Fields
        Gameboy _gameboy;

        GraphicsDevice _graphicsDevice;
        SpriteBatch _spriteBatch;

        Color[][] _emuPalette;
        int _emuColorIndex;
        Color _gridColorDark;
        Color _gridColorLight;
        Color _pixelMarkerTextColor;
        Color _pixelMarkerColor;

        int DebugMode;

        List<string> _romList = new List<string>();
        List<BaseControl> _controls;

        static public Texture2D _Pixel;
        static public SpriteFont _Font;

        Color onscreenColor = new Color(0.1f, 0.1f, 0.1f);
        Color menuColor = new Color(0.1f, 0.1f, 0.1f);

        BaseControl OnScreenButtons;
        BaseControl DebugInformations;
        MenuButton _ch1;
        MenuButton _ch2;

        MenuButton _menu;
        int _openStartIndex = 0;
        const int OPEN_ENTRIES = 12;

        #endregion

        #region Constructors
        public Emulator(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            DebugMode = 0;
            _controls = new List<BaseControl>();
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
                    // Grey
                    new Color(255, 255, 255),
                    new Color(192, 192, 192),
                    new Color(105, 106, 106),
                    new Color(7, 9, 9)
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
            _gridColorDark = new Color(0, 0, 0, 128);
            _gridColorLight = new Color(0, 0, 0, 32);
            _pixelMarkerTextColor = new Color(255, 0, 255, 255);
            _pixelMarkerColor = new Color(255, 0, 255, 255);
            updateRomList(null, EventArgs.Empty); // initial call to rom search - updated by click on "open rom"

            MenuButton temp;
            MenuButton btn;

            #region onscreen buttons

            OnScreenButtons = new BaseControl(null);
            _controls.Add(OnScreenButtons);

            // dpad
            BaseControl _dpad = new BaseControl(null) { Left = 120, Top = 400 };
            OnScreenButtons.Add(_dpad);

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
            btn.SetButtonColors(Color.Transparent, onscreenColor);
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
            btn.SetButtonColors(Color.Transparent, onscreenColor);
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
            btn.SetButtonColors(Color.Transparent, onscreenColor);
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
            btn.SetButtonColors(Color.Transparent, onscreenColor);
            _dpad.Add(btn);


            // buttons A, B
            BaseControl _btns = new BaseControl(null) { Left = 1150, Top = 400 };
            OnScreenButtons.Add(_btns);

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
            btn.SetButtonColors(Color.Transparent, onscreenColor);
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
            btn.SetButtonColors(Color.Transparent, onscreenColor);
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
            btn.SetButtonColors(Color.Transparent, onscreenColor);
            OnScreenButtons.Add(btn);

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
            btn.SetButtonColors(Color.Transparent, onscreenColor);
            OnScreenButtons.Add(btn);
            #endregion

            #region debug infos

            DebugInformations = new BaseControl(null);
            _controls.Add(DebugInformations);

            _ch1 = new MenuButton(DebugInformations, null, "CH1", MenuType.StandAlone, "sound", 2) { Width = 60, Height = 60 };
            _ch1.Left = 180;
            _ch1.Top = 0;
            _ch1.Image.ResizeToParent();
            _ch1.SetButtonColors(Color.Transparent, onscreenColor);
            DebugInformations.Add(_ch1);

            _ch2 = new MenuButton(DebugInformations, null, "CH2", MenuType.StandAlone, "sound", 2) { Width = 60, Height = 60 };
            _ch2.Left = 180;
            _ch2.Top = 60;
            _ch2.Image.ResizeToParent();
            _ch2.SetButtonColors(Color.Transparent, onscreenColor);
            DebugInformations.Add(_ch2);

            #endregion

            #region window buttons
            // maximize window
            temp = new MenuButton(image: "max") { Width = 60, Height = 60 };
            temp.Image.ResizeToParent();
            temp.Left = 1160;
            temp.BackColor[State.Idle] = Color.Transparent;
            temp.ForeColor[State.Idle] = menuColor;
            temp.OnClick += toggleFullScreen;
            _controls.Add(temp);

            // quit emulator
            temp = new MenuButton(image: "quit") { Width = 60, Height = 60 };
            temp.Image.ResizeToParent();
            temp.Left = 1220;
            temp.BackColor[State.Idle] = Color.Transparent;
            temp.ForeColor[State.Idle] = menuColor;
            temp.OnClick += exit;
            _controls.Add(temp);
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
            _menu.OnOpen += (o, e) => { OnScreenButtons.Enabled = false; Fokus = ((MenuButton)o).SubMenu.Values.ToArray<MenuButton>()[0]; Fokus.Close(o, e); };
            _menu.OnClose += (o, e) => { OnScreenButtons.Enabled = true; Fokus = null; };
            _menu.ForeColor[State.Idle] = menuColor;
            _controls.Add(_menu);
            // add menu entries
            _menu.AddClickMenu("cart").Label.Caption = "game";
            _menu.AddClickMenu("set").Label.Caption = "settings";
            _menu.AddClickMenu("debug").Enabled = false;
            _menu.AddClickMenu("quit");
            // cart
            MenuButton romBrowser = _menu["cart"].AddClickMenu("open rom");
            romBrowser.OnOpen += updateRomList;
            romBrowser.OnOpen += (o, e) => { fillOpenDialog(romBrowser); };
            _menu["cart"]["open rom"].AddClickMenu("up", "arrow", 300, 40).OnClick += (o, e) => { _openStartIndex -= OPEN_ENTRIES; fillOpenDialog(_menu["cart"]["open rom"]); };
            _menu["cart"]["open rom"]["up"].Image.SetRotation(90);
            for (int i = 0; i < OPEN_ENTRIES; i++)
            {
                _menu["cart"]["open rom"].AddClickMenu(i.ToString(), null, 300, 40).OnClick += openRomIndex; // add empty entry dummies
            }
            _menu["cart"]["open rom"].AddClickMenu("down", "arrow", 300, 40).OnClick += (o, e) => { _openStartIndex += OPEN_ENTRIES; fillOpenDialog(_menu["cart"]["open rom"]); };
            _menu["cart"]["open rom"]["down"].Image.SetRotation(-90);
            _menu["cart"].AddClickMenu("reset rom").OnClick += _gameboy.Reset;
            _menu["cart"].AddClickMenu("exit rom").OnClick += _gameboy.EjectCartridge;
            // set
            _menu["set"].AddClickMenu("palette");
            for (int i = 0; i < _emuPalette.Count(); i++)
            {
                temp = _menu["set"]["palette"].AddClickMenu("color" + i.ToString());
                temp.Label.Caption = "";
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
                temp.OnClick += setColorIndex;
            }
            _menu["set"].AddClickMenu("screen buttons").OnClick += (o, e) => { OnScreenButtons.Visible = !OnScreenButtons.Visible; };
            _menu["set"].AddClickMenu("fullscreen").OnClick += toggleFullScreen;
            // quit
            _menu["quit"].AddClickMenu("quit GEM").OnClick += exit;

            #endregion

        }

        public void Reset()
        {
            _gameboy.PowerOff();
            _gameboy.PowerOn();
        }

        public void ShutDown<EventArgs>(object sender, EventArgs e)
        {
            // Eventhandler to save game when closing window
            _gameboy.PowerOff();
        }

        public void Update(Viewport viewport)
        {
            Input.Update();

            foreach (BaseControl control in _controls)
            {
                control.Update();
            }
        }
        public void Draw(Viewport viewport)
        {
            _spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);

            _gameboy.UpdateFrame();
            Texture2D gbScreen = _gameboy.GetScreen(_emuPalette[_emuColorIndex]);

            drawEmulator(viewport, gbScreen);

            _spriteBatch.End();
        }


        // Private Helper Methods
        private void drawEmulator(Viewport viewport, Texture2D screen)
        {
            // Screen Position & Size
            float pixelSize = MathHelper.Min(viewport.Height / 144f,
                                             viewport.Width / 160f);

            int screenWidth = (int)pixelSize * 160;
            int screenHeight = (int)pixelSize * 144;
            int screenLeft = (viewport.Width - screenWidth) / 2;
            int screenTop = (viewport.Height - screenHeight) / 2;

            // Draw Screen
            _spriteBatch.Draw(screen, new Rectangle(screenLeft, screenTop, screenWidth, screenHeight), Color.White);

            // Debug Mode
            if (DebugMode >= 1)
            {
                if (DebugMode >= 2)
                {
                    drawGrid(pixelSize, screenLeft, screenTop, screenWidth, screenHeight);
                    if (Input.MousePosX >= screenLeft &&
                        Input.MousePosX < screenLeft + screenWidth &&
                        Input.MousePosY >= screenTop &&
                        Input.MousePosY < screenTop + screenHeight)
                    {
                        drawMouseMarker(pixelSize, screenLeft, screenTop);
                    }
                }

                //drawWindow      (screenLeft + screenWidth + 20, screenTop);
                //drawBackground  (screenLeft + screenWidth + 20, screenTop + 286);
                //drawTileset     (screenLeft + screenWidth + 20, screenTop + 576);
            }

            _ch1.Image.ImageIndex = Convert.ToInt32(_gameboy.IsCH1On);
            _ch1.SetButtonColors(Color.Transparent, _gameboy.IsCH1On?Color.White:onscreenColor);
            _ch2.Image.ImageIndex = Convert.ToInt32(_gameboy.IsCH2On);
            _ch2.SetButtonColors(Color.Transparent, _gameboy.IsCH2On ? Color.White : onscreenColor);

            foreach (BaseControl control in _controls)
            {
                control.Draw(_spriteBatch);
            }
        }

        private void drawMouseMarker(float size, int left, int top)
        {
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
            _spriteBatch.DrawString(_Font, "Window:", new Vector2(posX, posY), _emuPalette[_emuColorIndex][1]);
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

        private void toggleFullScreen(object sender, EventArgs e)
        {
            Game1._Graphics.ToggleFullScreen();
        }
        private void exit(object sender, EventArgs e)
        {
            Game1._Instance.Exit();
        }

        private void setColorIndex(object sender, EventArgs e)
        {
            MenuButton btn = (MenuButton)sender;
            _emuColorIndex = btn.ButtonData;
        }

        private void updateRomList(object sender, EventArgs e)
        {
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
        private void openRomIndex(object sender, EventArgs e)
        {
            MenuButton menuButton = (MenuButton)sender;
            int index = menuButton.ButtonData;
            _gameboy.PowerOff();
            updateRomList(null, EventArgs.Empty);
            if (index < _romList.Count) // error check
            {
                _gameboy.InsertCartridge(_romList[index]);
                _gameboy.PowerOn();
            }
            _menu.Close(null, EventArgs.Empty);
        }
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
            parent["down"].Enabled = (_openStartIndex + OPEN_ENTRIES) < _romList.Count;
        }
        #endregion
    }
}
