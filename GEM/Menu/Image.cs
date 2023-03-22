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
        public Image(BaseControl parent, string image, int imagesPerRow = 1) : base(parent)
        {
            // load image
            _image = Game1._Content.Load<Texture2D>(image);
            ImagesPerRow = imagesPerRow;

            // default values
            ForeColor = Color.White;
            Width = _image.Width;
            Height = _image.Height;
        }
        public Image(BaseControl parent, Texture2D image, int imagesPerRow = 1) : base(parent)
        {
            // image from texture2d
            _image = image;
            ImagesPerRow = imagesPerRow;

            // default values
            ForeColor = Color.White;
            Width = _image.Width;
            Height = _image.Height;
        }
        #endregion

        #region Properties
        public Color ForeColor { get; set; }
        public int ImagesPerRow { get; private set; }
        public int ImageIndex { get; set; }
        public float TextureAspectRatio
        {
            get
            {
                return (float)_image.Width / ImagesPerRow / _image.Height;
            }
        }
        #endregion

        #region Methods
        public override void Draw(SpriteBatch spriteBatch)
        {
            Rectangle destinationRectangle = new Rectangle(PosX + Width / 2, PosY + Height / 2, Width, Height); // offset position to compensate rotation behaviour (position refers to image origin)
            int drawWidth = (int)(_image.Height * TextureAspectRatio);
            int drawLeft = (drawWidth * ImageIndex) % _image.Width;
            Rectangle sourceRectangle = new Rectangle(drawLeft, 0, drawWidth, _image.Height); // use full texture
            Vector2 origin = new Vector2(drawWidth / 2f, _image.Height / 2f); // rotation around center of texture

            spriteBatch.Draw(_image, destinationRectangle, sourceRectangle, ForeColor, -_rotation, origin, SpriteEffects.None, 1.0f);

            base.Draw(spriteBatch);
        }

        public void ResizeToParent()
        {
            // resize performed before rotation, so preferably use square images
            float parentRatio = (float)Parent.Width / Parent.Height;

            if (TextureAspectRatio <= parentRatio)
            {
                Height = Parent.Height;
                Width = (int)(Height * TextureAspectRatio);
            }
            else 
            {
                Width = Parent.Width;
                Height = (int)(Width / TextureAspectRatio);
            }
        }

        #endregion
    }
}
