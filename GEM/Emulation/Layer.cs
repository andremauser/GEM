using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace GEM.Emulation
{
    public class Layer
    {

        #region Constructors

        public Layer(int width, int height, GraphicsDevice graphicsDevice = null)
        {
            RawData = new int[width * height];
            PaletteData = new int[width * height];
            ColorGrid = new Color[width * height];
            if (graphicsDevice != null)
            {
                Texture = new Texture2D(graphicsDevice, width, height);
            }
        }

        #endregion

        #region Properties

        public int[] RawData { get; set; }
        public int[] PaletteData { get; set; }
        public Color[] ColorGrid { get; set; }
        public Texture2D Texture { get; set; }

        #endregion

    }
}
