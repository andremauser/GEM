using Microsoft.Xna.Framework;

namespace GEM.Emulation
{
    internal class Tile
    {
        // static
        public static Color[] DMGPalette;

        #region Fields
        MMU _mmu;
        #endregion

        #region Constructors
        public Tile(MMU mmu)
        {
            _mmu = mmu;
            PixelData = new int[8, 8];
            PixelColor = new Color[8, 8];
        }
        #endregion

        #region Properties
        public int[,] PixelData { get; private set; }
        public Color[,] PixelColor { get; private set; }
        #endregion

        #region Methods
        public void ReadTile(int mapAddress)
        {
            // fill tile's PixelData[] and PixelColor[] from given VRAM map address

            // CGB attributes
            Register attributes = _mmu.ReadVRAM((ushort)mapAddress, 1);
            int paletteIndex =  attributes & 0b00000111;
            int tileBankIndex = attributes[3];
            bool hFlip =        attributes[5] == 1;
            bool vFlip =        attributes[6] == 1;
            bool bgPrio =       attributes[7] == 1; // TODO: implement BG Prio

            // color palette
            Color[] palette = new Color[4];
            if (_mmu.IsCGBMode)
            {
                palette = _mmu.CGB_BG_ColorPalettes[paletteIndex];
            }
            else
            {
                Register bgp = _mmu.BGP;
                palette[0] = DMGPalette[bgp[1] << 1 | bgp[0]];
                palette[1] = DMGPalette[bgp[3] << 1 | bgp[2]];
                palette[2] = DMGPalette[bgp[5] << 1 | bgp[4]];
                palette[3] = DMGPalette[bgp[7] << 1 | bgp[6]];
            }
            
            // tile block base address
            int tileDataBase = (_mmu.BGWData == 0) ? 0x9000 : 0x8000;

            // tile index
            int readIndex = _mmu.ReadVRAM((ushort)mapAddress, 0);
            int tileIndex = (tileDataBase == 0x9000) ? unchecked((sbyte)readIndex) : readIndex;

            // tile address
            ushort tileDataAddress = (ushort)(tileDataBase + tileIndex * 16);

            // decode tile
            for (int y = 0; y < 8; y++)
            {
                byte lowerByte  = _mmu.ReadVRAM((ushort)(tileDataAddress + 2 * y)    , tileBankIndex);
                byte higherByte = _mmu.ReadVRAM((ushort)(tileDataAddress + 2 * y + 1), tileBankIndex);

                for (int x = 0; x < 8; x++)
                {
                    int lowerBit = (lowerByte >> (7 - x)) & 1;
                    int higherBit = (higherByte >> (7 - x)) & 1;
                    int pixelData = (higherBit << 1) | lowerBit;

                    int pixelX = hFlip ? (7 - x) : x;
                    int pixelY = vFlip ? (7 - y) : y;
                    
                    PixelData[pixelX, pixelY] = pixelData;
                    PixelColor[pixelX, pixelY] = palette[pixelData];
                }
            }
        }
        #endregion
    }
}
