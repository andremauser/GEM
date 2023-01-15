using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace GEM.Emu
{
    class Sprite
    {
        private MMU _mmu;
        public ushort Address;
        private Register _flags;
        private int _flipY;
        private int _flipX;
        private byte _palette;
        private int _spriteHeight;
        public int[,] SpriteData { get; private set; }
        public int PosY { get; private set; }
        public int PosX { get; private set; }
        public int TileNumber { get; private set; }
        public int Priority { get; private set; }

        public Sprite(MMU mmu, ushort address)
        {
            _mmu = mmu;
            Address = address;
            Refresh();
        }

        public int PixelPalette(int pixelData)
        {
            int pixel;
            switch (pixelData)
            {
                default:
                case 0:
                    pixel = (_palette & 0b00000011) >> 0; break;
                case 1:
                    pixel = (_palette & 0b00001100) >> 2; break;
                case 2:
                    pixel = (_palette & 0b00110000) >> 4; break;
                case 3:
                    pixel = (_palette & 0b11000000) >> 6; break;
            }
            return pixel;
        }

        public void Refresh()
        {
            _spriteHeight = _mmu.OBJSize == 1 ? 16 : 8;
            SpriteData = new int[_spriteHeight, 8];

            PosY = _mmu.Read(Address);
            PosX = _mmu.Read((ushort)(Address + 1));
            TileNumber = _mmu.Read((ushort)(Address + 2));
            _flags = _mmu.Read((ushort)(Address + 3));
            Priority = _flags[7];
            _flipY = _flags[6];
            _flipX = _flags[5];
            switch (_flags[4])
            {
                default:
                case 0:
                    _palette = _mmu.OBP0; break;
                case 1:
                    _palette = _mmu.OBP1; break;
            }
            // Tile Data Location
            ushort tileDataAddress = (ushort)(0x8000 + TileNumber * 16);

            // Decode Tile
            for (int y = 0; y < _spriteHeight; y++)
            {
                byte lowerByte = _mmu.Read((ushort)(tileDataAddress + 2 * y));
                byte higherByte = _mmu.Read((ushort)(tileDataAddress + 2 * y + 1));

                for (int x = 0; x < 8; x++)
                {
                    int lowerBit = lowerByte >> 7 - x & 1;
                    int higherBit = higherByte >> 7 - x & 1;
                    int pixelData = higherBit << 1 | lowerBit;

                    // set pixel data and apply flip
                    SpriteData[_flipY == 1 ? _spriteHeight - 1 - y : y, _flipX == 1 ? 7 - x : x] = pixelData;
                }
            }
        }
    }
}
