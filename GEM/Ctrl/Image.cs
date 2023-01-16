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
        Color _imageColorIdle;
        Color _imageColorHover;
        Color _imageColorPress;

        public int Padding;
        

        public Image(BaseControl parent, Emulator emulator, string content) : base(parent, emulator)
        {
            _backColorIdle = Color.Transparent;
            _backColorHover = Color.Transparent;
            _backColorPress = Color.Transparent;

            _imageColorIdle = Color.White;
            _imageColorHover = Color.White;
            _imageColorPress = Color.DarkMagenta;
            _imageColor = _imageColorIdle;

            _image = Game1._Content.Load<Texture2D>(content);
            Padding = 0;
        }

        public override void Update()
        {
            switch (_customState)
            {
                case CustomState.Collapsed:
                    _imageColorPress = Color.DarkMagenta;
                    break;
                case CustomState.Expanded:
                    _imageColorPress = Color.White;
                    break;
                default:
                    break;
            }
            base.Update();
        }


        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_image, new Rectangle((int)GlobalPosition.X + Padding, (int)GlobalPosition.Y + Padding, Width - 2 * Padding, Height - 2 * Padding), _imageColor);
            base.Draw(spriteBatch);
        }

        internal override void onHover()
        {
            _imageColor = _imageColorIdle;
            base.onHover();
        }

        internal override void onHoverOut()
        {
            _imageColor = _imageColorIdle;
            base.onHoverOut();
        }

        internal override void onPress()
        {
            _imageColor = _imageColorPress;
            base.onPress();
        }

    }
}
