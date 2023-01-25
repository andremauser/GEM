using GEM.Emu;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GEM.Ctrl
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
            TextColor = Color.White;
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
                updateAlignPosition();
            }
        }
        public Color TextColor { get; set; }
        #endregion

        #region Methods
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(_font, _caption, new Vector2(PosX, PosY), TextColor);
            base.Draw(spriteBatch);
        }
        #endregion
    }
}
