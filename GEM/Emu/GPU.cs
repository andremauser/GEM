using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GEM.Emu
{
    public class GPU
    {

        #region Fields

        MMU _mmu;
        Sprite[] _spriteList;

        #endregion

        #region Constructors

        public GPU(MMU mmu, GraphicsDevice graphicsDevice)
        {
            _mmu = mmu;
            ModeClock = 0;
            _mmu.LCDMode = 2;

            Background = new Layer(256, 256, graphicsDevice);
            Window = new Layer(256, 256, graphicsDevice);
            Tileset = new Layer(128, 192, graphicsDevice);
            Screen = new Layer(160, 144, graphicsDevice);

            _spriteList = new Sprite[40];
            for (int i = 0; i < 40; i++)
            {
                _spriteList[i] = new Sprite(_mmu, (ushort)(0xFE00 + i * 4));
            }
        }

        #endregion

        #region Properties

        public int ModeClock { get; private set; }
        public bool IsDrawTime { get; private set; }

        public Layer Screen { get; private set; }
        public Layer Background { get; private set; }
        public Layer Window { get; private set; }
        public Layer Tileset { get; private set; }

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
                        afterVRAMRead(); // <-- renderScanline()
                    break;
                case 0: // Horizontal Blank
                    if (ModeClock >= 204)
                        afterHBlank();
                    break;
                case 1: // Vertical Blank (10 lines)
                    if (ModeClock >= 456)
                        afterVBlank();
                    break;
            }

            if (_mmu.LY == _mmu.LYC)
            {
                _mmu.LYCFlag = 1;                                                       // Set LYC=LY Flag
                if (_mmu.LYCIE == 1) { _mmu.IF |= 0b00000010; }                         // Request LCD STAT Interrupt
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

                IsDrawTime = true;                                          // Synchronize CPU
            }
        }

        private void afterVBlank()
        {
            ModeClock -= 456;
            _mmu.LY++;
            if (_mmu.LY > 153)
            {
                _mmu.LCDMode = 2;                                           // Enter Mode 2 => OAM Search, first line
                if (_mmu.Mode2IE == 1) { _mmu.IF |= 0b00000010; }           // Request LCD STAT Interrupt

                // Refresh Screens
                Background.RawData = renderBGWData(_mmu.BGMap);
                Background.PaletteData = renderPaletteData(Background.RawData, _mmu.BGP);

                Window.RawData = renderBGWData(_mmu.WindowMap);
                Window.PaletteData = renderPaletteData(Window.RawData, _mmu.BGP);

                Tileset.RawData = renderTileset();
                Tileset.PaletteData = renderPaletteData(Tileset.RawData, _mmu.BGP);

                // refresh sprites
                for (int i = 0; i < 40; i++)
                {
                    _spriteList[i].Refresh();
                }

                _mmu.LY = 0;
            }
        }


        private void renderScanline()
        {
            int y = _mmu.LY;
            Layer scanline = new Layer(160, 1);
            List<Sprite> sprites = oamSearch(y);
            // Render each Pixel
            for (int x = 0; x < 160; x++)
            {
                // resolve Background Pixel
                int bgPixelData = 0;
                int bgPixelPalette = 0;
                if (_mmu.IsBGEnabled)
                {
                    // Background
                    int posX = x + _mmu.SCX & 0xFF;
                    int posY = y + _mmu.SCY & 0xFF;
                    int index = 256 * posY + posX;
                    bgPixelData = Background.RawData[index];
                    bgPixelPalette = Background.PaletteData[index];
                }
                if (_mmu.IsWindowEnabled && y >= _mmu.WY && x >= _mmu.WX - 7)
                {
                    // Window overwriting Background Pixel
                    int posX = x - (_mmu.WX - 7);
                    int posY = y - _mmu.WY;
                    int index = 256 * posY + posX;
                    bgPixelData = Window.RawData[index];
                    bgPixelPalette = Window.PaletteData[index];
                }

                // resolve sprite priority
                // list of sprites already sorted depending on CGB-Mode
                // first non-transparent pixel in sprite in List "wins"
                int spritePixelData = 0;
                Sprite spriteOnPixel = null;
                if (_mmu.IsOBJEnabled)                              // TODO: implement 16-pixel Sprites 
                {
                    foreach (Sprite sprite in sprites)
                    {
                        if (sprite.PosX - 8 <= x && sprite.PosX > x)
                        {
                            int pixelData = sprite.SpriteData[y - (sprite.PosY - 16), x - (sprite.PosX - 8)];
                            if (pixelData != 0)
                            {
                                spritePixelData = pixelData;
                                spriteOnPixel = sprite;
                                break;
                            }
                        }
                    }
                }

                // Assign scanline pixel depending on sprite's bg-priority
                if (spriteOnPixel != null && (spriteOnPixel.Priority == 0 ||                      // Sprite on Top
                                             spriteOnPixel.Priority == 1 && bgPixelData == 0))  // Sprite in Background, but BG transparent
                {
                    scanline.RawData[x] = spritePixelData;
                    scanline.PaletteData[x] = spriteOnPixel.PixelPalette(spritePixelData);
                }
                else
                {
                    scanline.RawData[x] = bgPixelData;
                    scanline.PaletteData[x] = bgPixelPalette;
                }
                Screen.PaletteData[160 * y + x] = scanline.PaletteData[x];
            }
        }

        private int[] renderBGWData(int mapLocation)
        {
            // returns a 256 x 256 Pixel Data Layer based on Tile Map Location (BG & Window)

            int[] layer = new int[256 * 256];

            // Map Location
            ushort mapAddress = 0x9800;
            if (mapLocation == 1) mapAddress = 0x9C00;

            // Data Location
            ushort dataAddress = 0x8000;
            if (_mmu.BGWData == 0) dataAddress = 0x9000; // signed

            // Fill Map (32 x 32 Tiles = 256 x 256 Pixels)
            for (int mapY = 0; mapY < 32; mapY++)
            {
                for (int mapX = 0; mapX < 32; mapX++)
                {
                    // Read Tile Index from Map
                    int readIndex = _mmu.Read((ushort)(mapAddress + mapY * 32 + mapX));
                    int tileIndex;
                    if (dataAddress == 0x9000) { tileIndex = unchecked((sbyte)readIndex); } else { tileIndex = readIndex; }

                    // Tile Data Location
                    ushort tileDataAddress = (ushort)(dataAddress + tileIndex * 16);

                    // Decode Tile (8 x 8 Pixels)
                    decodeTile(ref layer, tileDataAddress, mapY, mapX, 256);
                }
            }
            return layer;
        }
        private int[] renderTileset()
        {
            int numRows = 24;
            int numCols = 16;
            int[] tileset = new int[numRows * 8 * numCols * 8];

            for (ushort address = 0x8000; address < 0x9800; address += 0x10)
            {
                int addressIndex = (address - 0x8000) / 0x10;
                int mapY = addressIndex / numCols;
                int mapX = addressIndex % numCols;
                decodeTile(ref tileset, address, mapY, mapX, numCols * 8);
            }
            return tileset;
        }
        private int[] renderPaletteData(int[] dataLayer, int palette)
        {
            // Returns Palette Layer from Data Layer
            int num = dataLayer.GetLength(0);
            int[] paletteData = new int[num];
            for (int i = 0; i < num; i++)
            {
                switch (dataLayer[i])
                {
                    default:
                    case 0:
                        paletteData[i] = (palette & 0b00000011) >> 0; break;
                    case 1:
                        paletteData[i] = (palette & 0b00001100) >> 2; break;
                    case 2:
                        paletteData[i] = (palette & 0b00110000) >> 4; break;
                    case 3:
                        paletteData[i] = (palette & 0b11000000) >> 6; break;
                }
            }
            return paletteData;
        }

        private void decodeTile(ref int[] refData, ushort tileDataAddress, int top, int left, int width)
        {
            // Decode Tile (8 x 8 Pixels)
            int pixelHeight = 8;
            for (int tileY = 0; tileY < pixelHeight; tileY++)
            {
                byte lowerByte = _mmu.Read((ushort)(tileDataAddress + 2 * tileY));
                byte higherByte = _mmu.Read((ushort)(tileDataAddress + 2 * tileY + 1));

                for (int tileX = 0; tileX < 8; tileX++)
                {
                    int lowerBit = lowerByte >> 7 - tileX & 1;
                    int higherBit = higherByte >> 7 - tileX & 1;
                    int pixelData = higherBit << 1 | lowerBit;

                    refData[(top * 8 + tileY) * width + left * 8 + tileX] = pixelData;
                }
            }
        }

        private List<Sprite> oamSearch(int scanLine)
        {
            List<Sprite> visibleSprites = new List<Sprite>();
            int spriteHeight = _mmu.OBJSize == 1 ? 16 : 8;

            for (int i = 0; i < 40; i++)
            {
                // only 10 sprites per line visible
                if (visibleSprites.Count >= 10) break;

                int posX = _spriteList[i].PosX;
                int posY = _spriteList[i].PosY;

                // Check if sprite on current Scanline (X position not being checked)
                if (scanLine >= posY - 16 && scanLine < posY - 16 + spriteHeight)
                {
                    visibleSprites.Add(_spriteList[i]);
                }
            }
            if (_mmu.IsCGB)
            {
                // Non-CGB mode: X-Coordinate Priority
                visibleSprites.OrderBy(x => x.PosX);
            }
            else
            {
                // CGB-Mode: OAM-Loacation Priority (Probably already is... just for clearance)
                visibleSprites.OrderBy(x => x.Address);
            }
            return visibleSprites;
        }

        #endregion

    }
}
