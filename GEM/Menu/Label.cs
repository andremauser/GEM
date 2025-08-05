using GEM.Emulation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GEM.Menu
{
    internal class Label : BaseControl
    {
        #region Fields
        SpriteFont _font;
        string _caption;
        #endregion

        #region Constructors
        public Label(BaseControl parent, string caption) : base(parent)
        {
            _font = Emulator._Font;
            Caption = caption;
            ForeColor = Color.White;
            MarkColor = Color.Transparent;
            HorizontalAlign = Align.Center;
            VerticalAlign = Align.Center;
        }
        #endregion

        #region Properties
        public string Caption
        {
            get
            {
                return _caption;
            }
            set
            {
                _caption = value;
                Width = (int)_font.MeasureString(_caption).X;
                Height = (int)_font.MeasureString(_caption).Y;
                Height = (int)(Height * 1f); // ground line adjustment
            }
        }
        public Color ForeColor { get; set; }
        public Color MarkColor { get; set; }
        #endregion

        #region Methods
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible) return;
            // text marking
            Rectangle   destinationRectangle =  new Rectangle(Left + Width / 2, Top + Height / 2, Width, Height); // offset position to compensate rotation behaviour (position refers to image origin)
            Rectangle   sourceRectangle =       new Rectangle(0, 0, _pixel.Width, _pixel.Height); // use full texture
            Vector2     origin =                new Vector2(0.5f, 0.5f); // rotation around center of texture
            spriteBatch.Draw(_pixel, destinationRectangle, sourceRectangle, MarkColor, -_rotation, origin, SpriteEffects.None, 1.0f);

            // text
            Vector2     position =              new Vector2(Left + Width / 2, Top + Height / 2);
                        origin =                new Vector2(Width / 2, Height / 2);
            Rectangle tempRectangle = spriteBatch.GraphicsDevice.ScissorRectangle;
            spriteBatch.GraphicsDevice.ScissorRectangle = new Rectangle(ParentControl.Left, ParentControl.Top, ParentControl.Width, ParentControl.Height);
            spriteBatch.DrawString(_font, _caption, position, ForeColor, -_rotation, origin, 1.0f, SpriteEffects.None, 1.0f);
            spriteBatch.GraphicsDevice.ScissorRectangle = tempRectangle;

            base.Draw(spriteBatch);
        }
        #endregion
    }
}
