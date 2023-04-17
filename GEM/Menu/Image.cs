using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GEM.Menu
{
    internal class Image : BaseControl
    {
        #region Fields
        public Texture2D Texture;
        #endregion

        #region Constructors
        public Image(BaseControl parent, string image, int imagesPerRow = 1) : base(parent)
        {
            // load image
            Texture = Game1._Content.Load<Texture2D>(image);
            ImagesPerRow = imagesPerRow;

            // default values
            ForeColor = Color.White;
            Width = Texture.Width / imagesPerRow;
            Height = Texture.Height;
        }
        public Image(BaseControl parent, Texture2D image, int imagesPerRow = 1) : base(parent)
        {
            // image from texture2d
            Texture = image;
            ImagesPerRow = imagesPerRow;

            // default values
            ForeColor = Color.White;
            Width = Texture.Width / imagesPerRow;
            Height = Texture.Height;
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
                return (float)Texture.Width / ImagesPerRow / Texture.Height;
            }
        }
        #endregion

        #region Methods
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible) return;
            Rectangle destinationRectangle = new Rectangle(PosX + Width / 2, PosY + Height / 2, Width, Height); // offset position to compensate rotation behaviour (position refers to image origin)
            int drawWidth = (int)(Texture.Height * TextureAspectRatio);
            int drawLeft = (drawWidth * ImageIndex) % Texture.Width;
            Rectangle sourceRectangle = new Rectangle(drawLeft, 0, drawWidth, Texture.Height); // use full texture
            Vector2 origin = new Vector2(drawWidth / 2f, Texture.Height / 2f); // rotation around center of texture

            spriteBatch.Draw(Texture, destinationRectangle, sourceRectangle, ForeColor, -_rotation, origin, SpriteEffects.None, 1.0f);

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
