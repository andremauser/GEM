using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace GEM.Emulation
{
    public class GPU
    {
        #region Fields
        MMU _mmu;
        Sprite[] _obList;

        Tile[,] _bgMap;
        Tile[,] _wdMap;

        Color[] _bgColorList;
        Color[] _wdColorList;
        Color[] _obColorList;
        #endregion

        #region Constructors
        public GPU(MMU mmu, GraphicsDevice graphicsDevice)
        {
            _mmu = mmu;
            ModeClock = 0;
            _mmu.LCDMode = 2;

            // initialize tilesets
            _bgMap = new Tile[32, 32];
            _wdMap = new Tile[32, 32];
            for (int x = 0; x < 32; x++)
            {
                for (int y = 0; y < 32; y++)
                {
                    _bgMap[x, y] = new Tile(_mmu);
                    _wdMap[x, y] = new Tile(_mmu);
                }
            }
            _obList = new Sprite[40];
            for (int i = 0; i < 40; i++)
            {
                _obList[i] = new Sprite(_mmu, (ushort)(0xFE00 + i * 4));
            }

            // texture sources
            _bgColorList = new Color[160 * 144];
            _wdColorList = new Color[160 * 144];
            _obColorList = new Color[160 * 144];

            // screen textures
            BGTexture = new Texture2D(graphicsDevice, 160, 144);
            WDTexture = new Texture2D(graphicsDevice, 160, 144);
            OBTexture = new Texture2D(graphicsDevice, 160, 144);
        }
        #endregion

        #region Properties
        public int ModeClock { get; private set; }
        public bool IsDrawTime { get; private set; }

        public Texture2D BGTexture { get; private set; }
        public Texture2D WDTexture { get; private set; }
        public Texture2D OBTexture { get; private set; }
        #endregion

        #region Methods
        public void Reset()
        {
            ModeClock = 0;
            _mmu.LCDMode = 2;
        }

        public void Update(int instructionCycles)
        {
            // Gets called after every CPU instruction

            // skip if LCD is off
            if (!_mmu.IsLCDOn)
            {
                ModeClock = 0;
                _mmu.LCDMode = 2;
                _mmu.LY = 0;
                return;
            }

            ModeClock += instructionCycles;
            IsDrawTime = false;

            switch (_mmu.LCDMode)
            {
                case 2: // Scanline: OAM Search
                    if (ModeClock >= 80)
                        afterOAMSearch();
                    break;
                case 3: // Scanline: VRAM Read
                    if (ModeClock >= 172)
                        afterVRAMRead();    // <-- renderScanline() incl. OAM search
                    break;
                case 0: // Horizontal Blank
                    if (ModeClock >= 204)
                        afterHBlank();      // <-- Draw screen after last Line
                    break;
                case 1: // Vertical Blank (10 lines)
                    if (ModeClock >= 456)
                        afterVBlank();      // <-- build tileMaps for next frame
                    break;
            }

            if (_mmu.LY == _mmu.LYC)
            {
                // Set LYC=LY Flag
                _mmu.LYCFlag = 1;
                // Request LCD STAT Interrupt
                if (_mmu.LYCIE == 1) { _mmu.IF |= 0b00000010; }
            }
            else
            {
                _mmu.LYCFlag = 0;
            }
        }

        private void afterOAMSearch()
        {
            ModeClock -= 80;
            _mmu.LCDMode = 3;
        }
        private void afterVRAMRead()
        {

            // End of Scanline
            ModeClock -= 172;
            _mmu.LCDMode = 0;                                               // Enter Mode 0 => HBlank
            if (_mmu.Mode0IE == 1) { _mmu.IF |= 0b00000010; }               // Request LCD STAT Interrupt
            // ---------- //
            renderScanline();                                               // Render Scanline
            // ---------- //
        }
        private void afterHBlank()
        {
            ModeClock -= 204;
            _mmu.LY++;
            if (_mmu.LY <= 143)
            {
                _mmu.LCDMode = 2;                                           // Enter Mode 2 => OAM Search, next line
                if (_mmu.Mode2IE == 1) { _mmu.IF |= 0b00000010; }           // Request LCD STAT Interrupt
            }
            else
            {
                // End of Screen
                _mmu.LCDMode = 1;                                           // Enter Mode 1 => VBlank
                _mmu.IF |= 0b00000001;                                      // Request VBlank Interrupt
                if (_mmu.Mode1IE == 1) { _mmu.IF |= 0b00000010; }           // Request LCD STAT Interrupt

                // create textures from lists (built during scanlines)
                BGTexture.SetData(_bgColorList);
                WDTexture.SetData(_wdColorList);
                OBTexture.SetData(_obColorList);

                IsDrawTime = true;                                          // Synchronize CPU
            }
        }
        private void afterVBlank()
        {
            ModeClock -= 456;
            _mmu.LY++;
            if (_mmu.LY > 153)
            {
                // Enter Mode 2 => OAM Search, first line
                _mmu.LCDMode = 2;
                // Request LCD STAT Interrupt
                if (_mmu.Mode2IE == 1) { _mmu.IF |= 0b00000010; }

                // prepare next frame
                // build bg/wd maps from VRAM
                int bgMapBase = (_mmu.BGMap == 1) ? 0x9C00 : 0x9800;
                int wdMapBase = (_mmu.WDMap == 1) ? 0x9C00 : 0x9800;
                for (int x = 0; x < 32; x++)
                {
                    for (int y = 0; y < 32; y++)
                    {
                        _bgMap[x, y].ReadTile(bgMapBase + y * 32 + x);
                        _wdMap[x, y].ReadTile(wdMapBase + y * 32 + x);

                    }
                }
                // refresh sprite list
                for (int i = 0; i < 40; i++)
                {
                    _obList[i].ReadSprite();
                }

                _mmu.LY = 0;
            }
        }

        private void renderScanline()
        {
            int screenPixelY = _mmu.LY;
            List<Sprite> sprites = oamSearch(screenPixelY);

            // Render each Pixel
            for (int screenPixelX = 0; screenPixelX < 160; screenPixelX++)
            {
                // initial values
                Color bgPixelColor = Color.Transparent;
                Color wdPixelColor = Color.Transparent;
                Color obPixelColor = Color.Transparent;
                // priority check
                int bgPixelData = 0;

                Sprite spriteOnPixel = null;

                // Background
                if (_mmu.IsBGEnabled)
                {
                    int bgMapPixelX = (screenPixelX + _mmu.SCX) % 256;
                    int bgMapPixelY = (screenPixelY + _mmu.SCY) % 256;
                    int bgTileX = bgMapPixelX / 8;
                    int bgTileY = bgMapPixelY / 8;
                    int bgTilePixelX = bgMapPixelX % 8;
                    int bgTilePixelY = bgMapPixelY % 8;
                    // bg pixel for ob priority check
                    bgPixelData  = _bgMap[bgTileX, bgTileY].PixelData[bgTilePixelX, bgTilePixelY];
                    // set bg pixel
                    bgPixelColor = _bgMap[bgTileX, bgTileY].PixelColor[bgTilePixelX, bgTilePixelY];
                }

                // Window
                if (_mmu.IsWindowEnabled && screenPixelY >= _mmu.WY && screenPixelX >= _mmu.WX - 7)
                {
                    int wdMapPixelX = screenPixelX - (_mmu.WX - 7);
                    int wdMapPixelY = screenPixelY - _mmu.WY;
                    int wdTileX = wdMapPixelX / 8;
                    int wdTileY = wdMapPixelY / 8;
                    int wdTilePixelX = wdMapPixelX % 8;
                    int wdTilePixelY = wdMapPixelY % 8;
                    // overwrite bg priority pixel
                    bgPixelData  = _wdMap[wdTileX, wdTileY].PixelData[wdTilePixelX, wdTilePixelY];
                    // set wd pixel
                    wdPixelColor = _wdMap[wdTileX, wdTileY].PixelColor[wdTilePixelX, wdTilePixelY];
                }

                // Objects (Sprites)
                // get pixel data for priority check
                Color obPixelColorTemp = Color.Transparent;
                if (_mmu.IsOBJEnabled)  // TODO: implement 16-pixel Sprites 
                {
                    foreach (Sprite sprite in sprites) // list is priority sorted by oamSearch
                    {
                        if (screenPixelX >= sprite.PosX - 8 && screenPixelX < sprite.PosX) // sprite over current pixel
                        {
                            int pixelData = sprite.PixelData[screenPixelX - (sprite.PosX - 8), screenPixelY - (sprite.PosY - 16)];
                            obPixelColorTemp = sprite.PixelColor[screenPixelX - (sprite.PosX - 8), screenPixelY - (sprite.PosY - 16)];
                            if (pixelData != 0) // and pixel not transparent
                            {
                                spriteOnPixel = sprite;
                                break; // first sprite wins
                            }
                        }
                    }
                }
                // priority check &  set ob pixel 
                // TODO: add CGB priority
                if (spriteOnPixel != null && (spriteOnPixel.BGPrio == 0 ||                     // Sprite on Top
                                              spriteOnPixel.BGPrio == 1 && bgPixelData == 0))  // Sprite in Background, but BG transparent
                {
                    //spritePixel = spriteOnPixel.PixelPalette(obPixelData);
                    obPixelColor = obPixelColorTemp;
                }

                int pos = 160 * screenPixelY + screenPixelX;
                _bgColorList[pos] = bgPixelColor;
                _wdColorList[pos] = wdPixelColor;
                _obColorList[pos] = obPixelColor;
            }
        }

        private List<Sprite> oamSearch(int scanLine)
        {
            // returns a priority-sorted list of visible sprites per scanline
            List<Sprite> visibleSprites = new List<Sprite>();
            int spriteHeight = _mmu.OBJSize == 1 ? 16 : 8;

            // check if sprite on current scanline
            for (int i = 0; i < 40; i++)
            {
                int posY = _obList[i].PosY - 16;
                if (scanLine >= posY && scanLine < posY + spriteHeight)
                {
                    visibleSprites.Add(_obList[i]);
                }
            }

            // sprite priority
            if (_mmu.IsCGBMode)
            {
                // CGB-Mode: OAM-Location Priority
                visibleSprites = visibleSprites.OrderBy(x => x.OAMAddress).ToList();
            }
            else
            {
                // Non-CGB mode: X-Coordinate Priority
                visibleSprites = visibleSprites.OrderBy(x => x.PosX).ToList();
            }

            // only 10 sprites per line visible
            int maxSprites = 10;
            if (visibleSprites.Count > maxSprites)
            { 
                visibleSprites.RemoveRange(maxSprites, visibleSprites.Count - maxSprites);
            }
            return visibleSprites;
        }
        #endregion

    }
}
