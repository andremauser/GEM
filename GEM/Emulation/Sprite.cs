using Microsoft.Xna.Framework;

namespace GEM.Emulation
{
    class Sprite
    {
        // static
        public static Color[] DMGPalette;

        #region Fields
        MMU _mmu;
        #endregion

        #region Constructors
        public Sprite(MMU mmu, ushort oamAddress)
        {
            _mmu = mmu;
            PixelData = new int[8, 8];
            PixelColor = new Color[8, 8];
            OAMAddress = oamAddress;
        }
        #endregion

        #region Properties
        public int[,] PixelData { get; private set; }
        public Color[,] PixelColor { get; private set; }
        // GPU access
        public ushort OAMAddress { get; private set; }
        public int PosX { get; private set; }
        public int PosY { get; private set; }
        public int BGPrio { get; private set; }
        #endregion

        #region Methods
        public void ReadSprite()
        {
            // re-create arrays on sprite height change
            int spriteHeight = _mmu.OBJSize == 1 ? 16 : 8;
            if (PixelData.GetLength(1) != spriteHeight)
            {
                PixelData = new int[8, spriteHeight];
                PixelColor = new Color[8, spriteHeight];
            }

            // attributes
            PosY =                  _mmu.Read((ushort)(OAMAddress + 0));
            PosX =                  _mmu.Read((ushort)(OAMAddress + 1));
            int tileIndex =         _mmu.Read((ushort)(OAMAddress + 2));
            Register attributes =   _mmu.Read((ushort)(OAMAddress + 3));
            int paletteIndex =  attributes & 0b00000111;
            int tileBankIndex = _mmu.IsCGBMode ? attributes[3] : 0;
            int dmgPalette =    attributes[4];
            bool hFlip =        attributes[5] == 1;
            bool vFlip =        attributes[6] == 1;
            BGPrio =            attributes[7];

            // color palette
            Color[] palette = new Color[4];
            if (_mmu.IsCGBMode)
            {
                palette = _mmu.CGB_OB_ColorPalettes[paletteIndex];
            }
            else
            {
                Register bgp = (dmgPalette == 0) ? _mmu.OBP0 : _mmu.OBP1;
                palette[0] = DMGPalette[bgp[1] << 1 | bgp[0]];
                palette[1] = DMGPalette[bgp[3] << 1 | bgp[2]];
                palette[2] = DMGPalette[bgp[5] << 1 | bgp[4]];
                palette[3] = DMGPalette[bgp[7] << 1 | bgp[6]];
            }
            // Tile Data Location
            ushort tileDataAddress = (ushort)(0x8000 + tileIndex * 16);

            // Decode Tile
            for (int y = 0; y < spriteHeight; y++)
            {
                byte lowerByte  = _mmu.ReadVRAM((ushort)(tileDataAddress + 2 * y + 0), tileBankIndex);
                byte higherByte = _mmu.ReadVRAM((ushort)(tileDataAddress + 2 * y + 1), tileBankIndex);

                for (int x = 0; x < 8; x++)
                {
                    int lowerBit  = (lowerByte  >> (7 - x)) & 1;
                    int higherBit = (higherByte >> (7 - x)) & 1;
                    int pixelData = higherBit << 1 | lowerBit;

                    int pixelX = hFlip ? (7 - x) : x;
                    int pixelY = vFlip ? (spriteHeight - 1 - y) : y;

                    PixelData[pixelX, pixelY] = pixelData;
                    PixelColor[pixelX, pixelY] = palette[pixelData];
                }
            }
        }
        #endregion
    }
}
