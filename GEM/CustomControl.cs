using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEM
{

    public enum CustomState
    {
        Collapsed,
        Expanded
    }

    public enum Align
    {
        Left,
        Center,
        Right
    }
    
    internal abstract class CustomControl
    {

        #region Fields

        // Private
        Color _backColor;
        Color _textColor;
        bool _wasPressed;

        // Public
        public int Left;
        public int Top;

        public int Width;
        public int Height;

        public bool Visible;
        public bool Enabled;

        public string Caption;
        public Align HorizontalTextAlign;

        public List<CustomControl> Controls;

        public CustomControl Parent;

        public bool HoverEnabled;
        public bool ClickEnabled;

        public Color BackColorDisabled;
        public Color BackColorIdle;
        public Color BackColorHover;
        public Color BackColorPress;

        public Color TextColorDisabled;
        public Color TextColorIdle;
        public Color TextColorHover;
        public Color TextColorPress;

        public Texture2D Texture;
        public SpriteFont Font;

        public delegate void CustomAction();
        public CustomAction ClickAction;

        public CustomState CustomState;

        #endregion


        #region Constructors

        protected CustomControl(Texture2D texture, SpriteFont spriteFont, CustomControl parent)
        {
            Texture = texture;
            Font = spriteFont;
            Parent = parent;
            Controls = new List<CustomControl>();

            // Default values
            Caption = "";
            HorizontalTextAlign = Align.Center;
            Visible = true;
            Enabled = true;
            Left = 0;
            Top = 0;
            Width = 100;
            Height = 100;
            HoverEnabled = true;
            ClickEnabled = true;
            BackColorDisabled = Color.Gray;
            BackColorIdle = Color.White;
            BackColorHover = Color.Yellow;
            BackColorPress = Color.Red;
            TextColorDisabled = Color.DarkGray;
            TextColorIdle = Color.Black;
            TextColorHover = Color.Black;
            TextColorPress = Color.White;
            ClickAction = noAction;
            CustomState = CustomState.Collapsed;
        }

        #endregion


        #region Properties

        public Vector2 GlobalPosition
        {
            get
            {
                if (Parent != null)
                {
                    return Parent.GlobalPosition + new Vector2(Left, Top);
                }
                else
                {
                    return new Vector2(Left, Top);
                }
            }
        }

        #endregion


        #region Methods

        public virtual void Update()
        {
            // Update embedded controls first (click priority from top to bottom)
            if (Controls.Count > 0)
            {
                foreach (CustomControl control in Controls)
                {
                    control.Update();
                }
            }

            // Default
            _backColor = BackColorIdle;
            _textColor = TextColorIdle;

            // Disabled
            if (!Enabled)
            {
                _backColor = BackColorDisabled;
                _textColor = TextColorDisabled;
                return;
            }

            // Hover
            if (Input.MousePosX >= GlobalPosition.X &&
                Input.MousePosX <  GlobalPosition.X + Width &&
                Input.MousePosY >= GlobalPosition.Y &&
                Input.MousePosY <  GlobalPosition.Y + Height)
            {
                if (HoverEnabled)
                {
                    _backColor = BackColorHover;
                    _textColor = TextColorHover;
                    customHoverAction();
                }
                if (ClickEnabled)
                {
                    if (Input.IsLeftButtonPressed)
                    {
                        // Button down
                        _backColor = BackColorPress;
                        _textColor = TextColorPress;
                        customPressAction();
                        _wasPressed = true;
                    }
                    else
                    {
                        if (_wasPressed)
                        {
                            // Click performed
                            customClickAction();
                            ClickAction();
                            _wasPressed = false;
                        }
                    }
                }
            }
            else
            {
                // Un-Hover
                customUnHoverAction();
                _wasPressed = false;
            }
        }
        
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible)
            {
                return;
            }

            // Rectangle
            spriteBatch.Draw(Texture, new Rectangle((int)GlobalPosition.X, (int)GlobalPosition.Y, Width, Height), _backColor);

            // Caption
            Vector2 captionSize = Font.MeasureString(Caption);
            Vector2 captionPos;
            Switch(HorizontalTextAlign)
            {
                case Left:
                    captionPos = new Vector2(GlobalPosition.X,
                                             GlobalPosition.Y + (Height - captionSize.Y) / 2 + captionSize.Y / 6);
                    break;
                case Center:
                    captionPos = new Vector2(GlobalPosition.X + (Width -  captionSize.X) / 2,
                                             GlobalPosition.Y + (Height - captionSize.Y) / 2 + captionSize.Y / 6);
                    break;
                case Right:
                    captionPos = new Vector2(GlobalPosition.X + Width -  captionSize.X,
                                             GlobalPosition.Y + (Height - captionSize.Y) / 2 + captionSize.Y / 6);
                    break
                default:
                    break;
            }
            spriteBatch.DrawString(Font, Caption, captionPos, _textColor);

            // Draw embedded controls last (draw from bottom to top)
            if (Controls.Count > 0)
            {
                foreach (CustomControl control in Controls)
                {
                    control.Draw(spriteBatch);
                }
            }
        }

        internal virtual void customHoverAction()
        { }

        internal virtual void customPressAction()
        { }

        internal virtual void customClickAction()
        { }

        internal virtual void customUnHoverAction()
        { }

        private void noAction()
        { }

        #endregion

    }
}
