using GEM.Menu;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml;
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

        MenuButton _leftMenu;
        BaseControl _dpad;

        MenuButton _buttonA;
        MenuButton _buttonB;
        MenuButton _buttonStart;
        MenuButton _buttonSelect;

        MenuButton _buttonLeft;
        MenuButton _buttonRight;
        MenuButton _buttonUp;
        MenuButton _buttonDown;

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

            // add "left menu" button
            _leftMenu = new MenuButton(null, null, "LB", MenuType.Click, "Left Bumper") { Width = 60, Height = 60 };
            _leftMenu.Image.ResizeToParent();
            _leftMenu.BackColor[State.Idle] = Color.Transparent;
            _leftMenu.Panel.HorizontalAlign = Align.Left;
            _leftMenu.Panel.VerticalAlign = Align.Bottom;
            _leftMenu.KeyBinding = Keys.LeftControl;
            _controls.Add(_leftMenu);
            // add menu entries
            _leftMenu.AddMenu("cart", width: 60, height: 60, image: "cartridge");
            _leftMenu.AddMenu("set", width: 60, height: 60);
            _leftMenu.AddMenu("3", width: 60, height: 60);
            _leftMenu.AddMenu("4", width: 60, height: 60);
            _leftMenu.AddMenu("5", width: 60, height: 60).OnClick += () => {
                _gameboy.PowerOff();
                _gameboy.InsertCartridge(_romList[0]);
                _gameboy.PowerOn();
            };
            _leftMenu["cart"].AddMenu("open rom").OnClick += () => { _leftMenu["cart"]["pause/start"].Enabled = !_leftMenu["cart"]["pause/start"].Enabled; }; ;
            _leftMenu["cart"].AddMenu("pause/start").OnClick += _gameboy.PauseToggle;
            _leftMenu["cart"]["pause/start"].Enabled = false;
            _leftMenu["cart"].AddMenu("reset").OnClick += _gameboy.Reset;
            _leftMenu["cart"].AddMenu("exit rom").OnClick += _gameboy.EjectCartridge;
            _leftMenu["set"].AddMenu("palette");
            _leftMenu["set"].AddMenu("fullscreen").OnClick += Game1._Graphics.ToggleFullScreen;
            _leftMenu["set"]["palette"].AddMenu("green").OnClick += () => { _emuColorIndex = 0; };
            _leftMenu["set"]["palette"].AddMenu("yellow").OnClick += () => { _emuColorIndex = 1; };
            _leftMenu["set"]["palette"].AddMenu("grey").OnClick += () => { _emuColorIndex = 2; };
            _leftMenu["set"]["palette"].AddMenu("original").OnClick += () => { _emuColorIndex = 3; };

            _dpad = new BaseControl(null) { Left = 120, Top = 500 };
            MenuButton btn = new MenuButton(_dpad, null, "->", MenuType.StandAlone, "dpad") { Width=100, Height=100};
            btn.Left = 20;
            btn.Top = -50;
            btn.Image.ResizeToParent();
            btn.KeyBinding = Keys.Right;
            btn.BtnBinding = Buttons.DPadRight;
            btn.OnPress += () => _gameboy.IsButton_Right = true;
            btn.OnRelease += () => _gameboy.IsButton_Right = false;
            _dpad.Add(btn);
            btn = new MenuButton(_dpad, null, "<-", MenuType.StandAlone, "dpad") { Width = 100, Height = 100 };
            btn.Left = -120;
            btn.Top = -50;
            btn.Image.ResizeToParent();
            btn.Image.SetRotation(180);
            btn.KeyBinding = Keys.Left;
            btn.BtnBinding = Buttons.DPadLeft;
            btn.OnPress += () => _gameboy.IsButton_Left = true;
            btn.OnRelease += () => _gameboy.IsButton_Left = false;
            _dpad.Add(btn);
            btn = new MenuButton(_dpad, null, "up", MenuType.StandAlone, "dpad") { Width = 100, Height = 100 };
            btn.Left = -50;
            btn.Top = -120;
            btn.Image.ResizeToParent();
            btn.Image.SetRotation(90);
            btn.KeyBinding = Keys.Up;
            btn.BtnBinding = Buttons.DPadUp;
            btn.OnPress += () => _gameboy.IsButton_Up = true;
            btn.OnRelease += () => _gameboy.IsButton_Up = false;
            _dpad.Add(btn);
            btn = new MenuButton(_dpad, null, "down", MenuType.StandAlone, "dpad") { Width = 100, Height = 100 };
            btn.Left = -50;
            btn.Top = 20;
            btn.Image.SetRotation(270);
            btn.KeyBinding = Keys.Down;
            btn.BtnBinding = Buttons.DPadDown;
            btn.OnPress += () => _gameboy.IsButton_Down = true;
            btn.OnRelease += () => _gameboy.IsButton_Down = false;
            _dpad.Add(btn);
            _controls.Add(_dpad);


            btn = new MenuButton(null, null, "A", MenuType.StandAlone, "btn") { Width = 100, Height = 100 };
            btn.Left = 1165;
            btn.Top = 420;
            btn.Image.ResizeToParent();
            btn.KeyBinding = Keys.X;
            btn.BtnBinding = Buttons.B;
            btn.OnPress += () => _gameboy.IsButton_A = true;
            btn.OnRelease += () => _gameboy.IsButton_A = false;
            _dpad.Add(btn);

            btn = new MenuButton(null, null, "B", MenuType.StandAlone, "btn") { Width = 100, Height = 100 };
            btn.Left = 1055;
            btn.Top = 460;
            btn.Image.ResizeToParent();
            btn.KeyBinding = Keys.Y;
            btn.BtnBinding = Buttons.A;
            btn.OnPress += () => _gameboy.IsButton_B = true;
            btn.OnRelease += () => _gameboy.IsButton_B = false;
            _dpad.Add(btn);

            btn = new MenuButton(null, null, "Start", MenuType.StandAlone, "btn") { Width = 100, Height = 100 };
            btn.Left = 1040;
            btn.Top = 620;
            btn.Image.ResizeToParent();
            btn.KeyBinding = Keys.Enter;
            btn.BtnBinding = Buttons.Start;
            btn.OnPress += () => _gameboy.IsButton_Start = true;
            btn.OnRelease += () => _gameboy.IsButton_Start = false;
            _dpad.Add(btn);

            btn = new MenuButton(null, null, "Select", MenuType.StandAlone, "btn") { Width = 100, Height = 100 };
            btn.Left = 140;
            btn.Top = 620;
            btn.Image.ResizeToParent();
            btn.KeyBinding = Keys.Back;
            btn.BtnBinding = Buttons.Back;
            btn.OnPress += () => _gameboy.IsButton_Select = true;
            btn.OnRelease += () => _gameboy.IsButton_Select = false;
            _dpad.Add(btn);
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

            foreach (BaseControl control1 in _controls)
            {
                control1.Update();
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

            //Temporary File Browser
            string fileBrowser = "Rom browser: 'roms/' - currently max. 5 games\n\n";
            if (!Directory.Exists("roms/"))
                Directory.CreateDirectory("roms/");
            int i = 0;
            _romList.Clear();
            foreach (var file in Directory.EnumerateFiles("roms/"))
            {
                int dotPos = file.LastIndexOf('.');
                if (file.Substring(dotPos, file.Length - dotPos) == ".gb")
                {
                    i++;
                    int slashPos = file.LastIndexOf('/') + 1;
                    fileBrowser += string.Format("{0} - {1}\n", i, file.Substring(slashPos, file.Length - slashPos));
                    _romList.Add(file);
                }
                // Temporarily max. 5 games - TODO: Better File Browser 
                if (i == 5)
                    break;
            }

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

        #endregion
    }
}
