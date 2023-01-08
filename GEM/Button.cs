using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEM
{
    internal class Button : Control
    {
        Texture2D _texture;
        SpriteFont _font;
        Color _bgColor;
        public delegate void Action();
        Action _action;
        bool _wasPressed;

        public string Caption { get; set; }
        public Color TextColor { get; set; }

        public Button(string caption, Action action, Texture2D texture, SpriteFont font):base()
        {
            Caption = caption;
            _action= action;
            _texture = texture;
            _font= font;
            _wasPressed= false;
            TextColor = Color.White;
            Enabled = true;
        }

        public override void Update()
        {
            if (!Enabled)
            {
                return;
            }
            // Hover
            if (Input.MousePosX >= Left &&
                Input.MousePosX < Left + Width &&
                Input.MousePosY >= Top &&
                Input.MousePosY < Top + Height)
            {
                _bgColor = Color.DarkMagenta;
                if (Input.IsLeftButtonPressed)
                {
                    // Button down
                    _bgColor = Color.Magenta;
                    _wasPressed= true;
                }
                else
                {
                    if (_wasPressed)
                    {
                        // Click performed
                        _action();
                        _wasPressed = false;
                    }
                }
            }
            else
            {
                _bgColor = Color.Black;
                _wasPressed= false;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, new Rectangle(Left, Top, Width, Height), _bgColor);
            Vector2 captionSize = _font.MeasureString(Caption);
            Vector2 captionPos = new Vector2(Left + (Width - captionSize.X) / 2,
                                             Top + (Height - captionSize.Y) / 2 + captionSize.Y/6);
            spriteBatch.DrawString(_font, Caption, captionPos, TextColor);
        }

    }
}
