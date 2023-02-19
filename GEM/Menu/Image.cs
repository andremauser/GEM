using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
            spriteBatch.Draw(_image, new Rectangle(PosX, PosY, Width, Height), ForeColor);
            base.Draw(spriteBatch);
        }

        public void ResizeToParent()
        {
            float aspectRatio = _image.Width / _image.Height;
            float parentRatio = _parent.Width / _parent.Height;

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
