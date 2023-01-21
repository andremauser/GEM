using GEM.Ctrl;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace GEM.Emu
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
        bool _isHandled_Color;
        bool _isHandled_Debug;
        bool _isHandled_Pause;
        bool _isHandled_Frame;
        bool _isHandled_Step;
        bool _isHandled_Reset;
        bool _isHandled_Quit;
        bool _isHandled_1;
        bool _isHandled_2;
        bool _isHandled_3;
        bool _isHandled_4;
        bool _isHandled_5;

        int _screenOffsetTop;
        int _screenOffsetBottom;
        int _screenOffsetLeft;
        int _screenOffsetRight;

        List<string> _romList = new List<string>();
        List<BaseControl> _controls = new List<BaseControl>();
        BaseControl _sidePanel;



        static public Texture2D _Pixel;
        static public SpriteFont _Font;

        #endregion

        #region Constructors

        public Emulator(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            DebugMode = 0;
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

            _sidePanel = new LeftPanel(null, this);
            _controls.Add(_sidePanel);

            Button i = _sidePanel.AddButton();
            i.Caption = "Test";
            i.ClickAction = DoNothing;

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

            _sidePanel.Height = viewport.Height;
            foreach (BaseControl control in _controls)
            {
                control.Update();
            }

            checkInput_Color();
            checkInput_Debug();

            _isHandled_Pause = checkInput(Input.IsButton_Pause, _isHandled_Pause, _gameboy.PauseSwitch);
            _isHandled_Frame = checkInput(Input.IsButton_Frame, _isHandled_Frame, _gameboy.PauseAfterFrame);
            _isHandled_Step = checkInput(Input.IsButton_Step, _isHandled_Step, _gameboy.PauseAfterStep);

            checkInput_Reset();
            checkInput_Quit();

            checkInput_1();
            checkInput_2();
            checkInput_3();
            checkInput_4();
            checkInput_5();
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
            float pixelSize = MathHelper.Min((viewport.Height - _screenOffsetTop - _screenOffsetBottom) / 144f,
                                             (viewport.Width - _screenOffsetLeft - _screenOffsetRight) / 160f);

            int screenWidth = (int)(pixelSize * 160);
            int screenHeight = (int)(pixelSize * 144);
            int screenLeft = _screenOffsetLeft + (viewport.Width - screenWidth - _screenOffsetLeft - _screenOffsetRight) / 2;
            int screenTop = _screenOffsetTop;

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
                //printDebugInfo(screenLeft - 180, screenTop);

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

        private void printDebugInfo(int posX, int posY)
        {
            _spriteBatch.DrawString(_Font, "Debug:\n\n" + _gameboy.DebugInfo, new Vector2(posX, posY), _emuPalette[_emuColorIndex][1]);
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

        private void checkInput_Color()
        {
            if (Input.IsButton_Color && !_isHandled_Color)
            {
                _emuColorIndex++;
                _emuColorIndex %= _emuPalette.GetLength(0);
                _isHandled_Color = true;
            }
            else if (!Input.IsButton_Color && _isHandled_Color)
            {
                _isHandled_Color = false;
            }
        }
        private void checkInput_Debug()
        {
            if (Input.IsButton_Debug && !_isHandled_Debug)
            {
                DebugMode++;
                DebugMode %= 2;
                _isHandled_Debug = true;
            }
            else if (!Input.IsButton_Debug && _isHandled_Debug)
            {
                _isHandled_Debug = false;
            }
        }

        private bool checkInput(bool button, bool isHandled, Action action)
        {
            // Handles Actions via Key or Button
            if (button && !isHandled)
            {
                // button pressed and not handled yet -> perform action and disable
                action();
                return true;
            }
            else if (!button && isHandled)
            {
                // button released -> enable again
                return false;
            }
            // keep status
            return isHandled;
        }

        private void doNothing() { }

        private void checkInput_Reset()
        {
            if (Input.IsButton_Reset && !_isHandled_Reset)
            {
                Reset();
                _isHandled_Reset = true;
            }
            else if (!Input.IsButton_Reset && _isHandled_Reset)
            {
                _isHandled_Reset = false;
            }
        }
        private void checkInput_Quit()
        {
            if (Input.IsButton_Quit && !_isHandled_Quit)
            {
                _gameboy.PowerOff();
                _isHandled_Quit = true;
            }
            else if (!Input.IsButton_Quit && _isHandled_Quit)
            {
                _isHandled_Quit = false;
            }
        }
        private void checkInput_1()
        {
            if (Input.IsButton_1 && !_isHandled_1 && _romList.Count >= 1)
            {
                _gameboy.PowerOff();
                _gameboy.InsertCartridge(_romList[0]);
                _gameboy.PowerOn();
                _isHandled_1 = true;
            }
            else if (!Input.IsButton_1 && _isHandled_1)
            {
                _isHandled_1 = false;
            }
        }
        private void checkInput_2()
        {
            if (Input.IsButton_2 && !_isHandled_2 && _romList.Count >= 2)
            {
                _gameboy.PowerOff();
                _gameboy.InsertCartridge(_romList[1]);
                _gameboy.PowerOn();
                _isHandled_2 = true;
            }
            else if (!Input.IsButton_2 && _isHandled_2)
            {
                _isHandled_2 = false;
            }
        }
        private void checkInput_3()
        {
            if (Input.IsButton_3 && !_isHandled_3 && _romList.Count >= 3)
            {
                _gameboy.PowerOff();
                _gameboy.InsertCartridge(_romList[2]);
                _gameboy.PowerOn();
                _isHandled_3 = true;
            }
            else if (!Input.IsButton_3 && _isHandled_3)
            {
                _isHandled_3 = false;
            }
        }
        private void checkInput_4()
        {
            if (Input.IsButton_4 && !_isHandled_4 && _romList.Count >= 4)
            {
                _gameboy.PowerOff();
                _gameboy.InsertCartridge(_romList[3]);
                _gameboy.PowerOn();
                _isHandled_4 = true;
            }
            else if (!Input.IsButton_4 && _isHandled_4)
            {
                _isHandled_4 = false;
            }
        }
        private void checkInput_5()
        {
            if (Input.IsButton_5 && !_isHandled_5 && _romList.Count >= 5)
            {
                _gameboy.PowerOff();
                _gameboy.InsertCartridge(_romList[4]);
                _gameboy.PowerOn();
                _isHandled_5 = true;
            }
            else if (!Input.IsButton_5 && _isHandled_5)
            {
                _isHandled_5 = false;
            }
        }

        // UI functions

        public void GameboyPowerOn()
        {
            _gameboy.PowerOn();
        }

        public void GameboyPowerOff()
        {
            _gameboy.PowerOff();
        }

        public void EjectCartridge()
        {
            _gameboy.EjectCartridge();
        }

        public bool IsGameboyOn()
        {
            return _gameboy.IsPowerOn;
        }

        public void DoNothing()
        {

        }


        #endregion
    }
}
