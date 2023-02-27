using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GEM.Menu
{
    internal class Image : BaseControl
    {
        #region Fields
        Texture2D _image;
        #endregion

        #region Constructors
        public Image(BaseControl parent, string image) : base(parent)
        {
            // load image
            _image = Game1._Content.Load<Texture2D>(image);

            // default values
            ForeColor = Color.White;
            Width = _image.Width;
            Height = _image.Height;
        }
        #endregion

        #region Properties
        public Color ForeColor { get; set; }
        #endregion

        #region Methods
        public override void Draw(SpriteBatch spriteBatch)
        {
            Rectangle destinationRectangle = new Rectangle(PosX + Width / 2, PosY + Height / 2, Width, Height); // offset position to compensate rotation behaviour (position refers to image origin)
            Rectangle sourceRectangle = new Rectangle(0, 0, _image.Width, _image.Height); // use full texture
            Vector2 origin = new Vector2(_image.Width / 2f, _image.Height / 2f); // rotation around center of texture

            spriteBatch.Draw(_image, destinationRectangle, sourceRectangle, ForeColor, -_rotation, origin, SpriteEffects.None, 1.0f);

            base.Draw(spriteBatch);
        }

        public void ResizeToParent()
        {
            // resize performed before rotation, so preferably use square images
            float aspectRatio = (float)_image.Width / _image.Height;
            float parentRatio = (float)_parent.Width / _parent.Height;

            if (aspectRatio <= parentRatio)
            {
                Height = _parent.Height;
                Width = (int)(Height * aspectRatio);
            }
            else 
            {
                Width = _parent.Width;
                Height = (int)(Width / aspectRatio);
            }
        }

        #endregion
    }
}
