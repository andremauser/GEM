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
    internal class Image : BaseControl
    {
        Texture2D _image;
        Color _imageColor;                      

        public Image(BaseControl parent, Emulator emulator, string content) : base(parent, emulator)
        {
            BackColorIdle = Color.Transparent;

            _imageColor = Color.White;

            _clickEnabled = false;
            _hoverEnabled = false;

            _image = Game1._Content.Load<Texture2D>(content);
            Padding = 0;
        }


        public override int Padding { get; set; }


        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_image, new Rectangle((int)GlobalPosition.X + Padding, (int)GlobalPosition.Y + Padding, Width - 2 * Padding, Height - 2 * Padding), _imageColor);
            base.Draw(spriteBatch);
        }
    }
}
