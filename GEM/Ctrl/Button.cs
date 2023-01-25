using GEM.Emu;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEM.Ctrl
{
    internal class Button : BaseControl
    {
        #region Fields
        Texture2D _pixel;
        #endregion

        #region Constructors
        public Button(BaseControl parent) : base(parent)
        {
            _pixel = Emulator._Pixel;
        }
        public Button(BaseControl parent, string caption) : base(parent)
        {
            _pixel = Emulator._Pixel;
            AddLabel(caption);
        }
        #endregion

        #region Properties
        public Color BackColor { get; set; }
        #endregion

        #region Methods
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_pixel, new Rectangle(PosX, PosY, Width, Height), BackColor);
            base.Draw(spriteBatch);
        }
        #endregion
    }
}
