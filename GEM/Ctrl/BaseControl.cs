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
    // Delegate for click-, hover-, etc. methods
    public delegate void CustomAction();

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

    internal abstract class BaseControl
    {

        #region Fields

        protected Color _backColor;
        protected Color _textColor;
        protected bool _wasPressed;
        protected CustomState _customState;
        protected string _caption;
        protected Emulator _emulator;

        protected int _left;
        protected int _top;
        protected int _width;
        protected int _height;

        public bool Visible;
        public bool Enabled;
        public CustomAction ClickAction;

        protected Align _horizontalTextAlign;

        protected List<BaseControl> _controls;

        protected BaseControl _parent;

        protected bool _hoverEnabled;
        protected bool _clickEnabled;
        protected Color _backColorDisabled;
        protected Color _backColorIdle;
        protected Color _backColorHover;
        protected Color _backColorPress;
        protected Color _textColorDisabled;
        protected Color _textColorIdle;
        protected Color _textColorHover;
        protected Color _textColorPress;

        protected Texture2D _texture;
        protected SpriteFont _font;

        #endregion


        #region Constructors

        protected BaseControl(BaseControl parent, Emulator emulator)
        {
            _texture = Emulator._Pixel;
            _font = Emulator._Font;
            _parent = parent;
            _controls = new List<BaseControl>();
            _emulator = emulator;

            // Default values
            _horizontalTextAlign = Align.Center;
            Visible = true;
            Enabled = true;
            _left = 0;
            _top = 0;
            _width = 100;
            _height = 100;
            _hoverEnabled = true;
            _clickEnabled = true;
            _backColorDisabled = Color.Gray;
            _backColorIdle = Color.White;
            _backColorHover = Color.Yellow;
            _backColorPress = Color.Red;
            _textColorDisabled = Color.DarkGray;
            _textColorIdle = Color.Black;
            _textColorHover = Color.Black;
            _textColorPress = Color.White;
            CustomState = CustomState.Collapsed;
        }

        #endregion


        #region Properties

        public int Left
        {
            get
            {
                return _left;
            }
            set
            {
                _left = value;
            }
        }
        public int Top
        {
            get
            {
                return _top;
            }
            set
            {
                _top = value;
            }
        }
        public virtual int Width
        {
            get
            {
                return _width;
            }
            set
            {
                _width = value;
            }
        }
        public virtual int Height
        {
            get
            {
                return _height;
            }
            set
            {
                _height = value;
            }
        }
        public string Caption
        {
            get
            {
                return _caption;
            }
            set
            {
                _caption = value;
            }
        }

        public Vector2 GlobalPosition
        {
            get
            {
                if (_parent != null)
                {
                    return _parent.GlobalPosition + new Vector2(_left, Top);
                }
                else
                {
                    return new Vector2(_left, Top);
                }
            }
        }

        public CustomState CustomState
        {
            get
            {
                return _customState;
            }
            set
            {
                _customState = value;
                foreach (BaseControl control in _controls)
                {
                    control.CustomState = _customState;
                }
            }
        }

        #endregion


        #region Methods

        public virtual void Update()
        {
            if (!Visible)
            {
                return;
            }
            // Update embedded controls first (click priority from top to bottom)
            if (_controls.Count > 0)
            {
                foreach (BaseControl control in _controls)
                {
                    control.Update();
                }
            }

            // Default
            _backColor = _backColorIdle;
            _textColor = _textColorIdle;

            // Disabled
            if (!Enabled)
            {
                _backColor = _backColorDisabled;
                _textColor = _textColorDisabled;
                return;
            }

            // Hover
            if (Input.MousePosX >= GlobalPosition.X &&
                Input.MousePosX < GlobalPosition.X + Width &&
                Input.MousePosY >= GlobalPosition.Y &&
                Input.MousePosY < GlobalPosition.Y + Height)
            {
                if (_hoverEnabled)
                {
                    _backColor = _backColorHover;
                    _textColor = _textColorHover;
                    onHover();
                }
                if (_clickEnabled)
                {
                    if (Input.IsLeftButtonPressed)
                    {
                        // Button down
                        _backColor = _backColorPress;
                        _textColor = _textColorPress;
                        onPress();
                        _wasPressed = true;
                    }
                    else
                    {
                        if (_wasPressed)
                        {
                            // Click performed
                            onClick();
                            _wasPressed = false;
                        }
                    }
                }
            }
            else
            {
                // Un-Hover
                onHoverOut();
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
            spriteBatch.Draw(_texture, new Rectangle((int)GlobalPosition.X, (int)GlobalPosition.Y, Width, Height), _backColor);

            // Caption
            if (_caption == null)
            {
                _caption = "";
            }
            Vector2 captionSize = _font.MeasureString(_caption);
            Vector2 captionPos;
            switch (_horizontalTextAlign)
            {
                case Align.Left:
                    captionPos = new Vector2(GlobalPosition.X,
                                             GlobalPosition.Y + (Height - captionSize.Y) / 2 + captionSize.Y / 6);
                    break;
                case Align.Center:
                    captionPos = new Vector2(GlobalPosition.X + (Width - captionSize.X) / 2,
                                             GlobalPosition.Y + (Height - captionSize.Y) / 2 + captionSize.Y / 6);
                    break;
                case Align.Right:
                    captionPos = new Vector2(GlobalPosition.X + Width - captionSize.X,
                                             GlobalPosition.Y + (Height - captionSize.Y) / 2 + captionSize.Y / 6);
                    break;
                default:
                    captionPos = Vector2.Zero;
                    break;
            }
            spriteBatch.DrawString(_font, _caption, captionPos, _textColor);

            // Draw embedded controls last (draw from bottom to top)
            if (_controls.Count > 0)
            {
                foreach (BaseControl control in _controls)
                {
                    control.Draw(spriteBatch);
                }
            }
        }

        internal virtual void onHover()
        { }

        internal virtual void onPress()
        { }

        internal virtual void onClick()
        {
            if (ClickAction == null)
            {
                return;
            }
            ClickAction();
        }

        internal virtual void onHoverOut()
        { }

        #endregion

    }
}
