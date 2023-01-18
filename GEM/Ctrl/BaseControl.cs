using GEM.Emu;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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

        protected bool _visible;
        protected bool _enabled;

        public CustomAction ClickAction;

        public Align Align;

        protected List<BaseControl> _controls;

        protected BaseControl _parent;

        protected bool _hoverEnabled;
        protected bool _clickEnabled;

        public Color BackColorDisabled;
        public Color BackColorIdle;
        public Color BackColorHover;
        public Color BackColorPress;
        public Color TextColorDisabled;
        public Color TextColorIdle;
        public Color TextColorHover;
        public Color TextColorPress;

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
            Align = Align.Center;
            Visible = true;
            Enabled = true;
            _left = 0;
            _top = 0;
            _width = 100;
            _height = 100;
            _hoverEnabled = true;
            _clickEnabled = true;
            BackColorDisabled = Color.Gray;
            BackColorIdle = Color.White;
            BackColorHover = Color.Yellow;
            BackColorPress = Color.Red;
            TextColorDisabled = Color.DarkGray;
            TextColorIdle = Color.Black;
            TextColorHover = Color.Black;
            TextColorPress = Color.White;
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

        public bool Visible
        {
            get
            {
                return _visible;
            }
            set
            {
                _visible = value;
                foreach (BaseControl control in _controls)
                {
                    control.Visible = value;
                }
            }
        }
        public bool Enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                _enabled = value;
                foreach (BaseControl control in _controls)
                {
                    control.Enabled = value;
                }
            }
        }

        public virtual int Padding { get; set; }
        public List<BaseControl> Controls
        {
            get
            {
                return _controls;
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
                Input.MousePosX < GlobalPosition.X + Width &&
                Input.MousePosY >= GlobalPosition.Y &&
                Input.MousePosY < GlobalPosition.Y + Height)
            {
                if (_hoverEnabled)
                {
                    _backColor = BackColorHover;
                    _textColor = TextColorHover;
                    onHover();
                }
                if (_clickEnabled)
                {
                    if (Input.IsLeftButtonPressed)
                    {
                        // Button down
                        _backColor = BackColorPress;
                        _textColor = TextColorPress;
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
            switch (Align)
            {
                case Align.Left:
                    captionPos = new Vector2(GlobalPosition.X + Padding,
                                             GlobalPosition.Y + (Height - captionSize.Y) / 2 + captionSize.Y / 6);
                    break;
                case Align.Center:
                    captionPos = new Vector2(GlobalPosition.X + (Width - captionSize.X) / 2,
                                             GlobalPosition.Y + (Height - captionSize.Y) / 2 + captionSize.Y / 6);
                    break;
                case Align.Right:
                    captionPos = new Vector2(GlobalPosition.X + Width - captionSize.X - Padding,
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

        public void AddControl(BaseControl control)
        {
            _controls.Add(control);
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
