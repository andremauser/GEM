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
    public delegate void CustomAction();

    internal class old_BaseControl
    {

        #region Fields

        protected Color _backColor;
        protected Color _textColor;
        protected bool _wasPressed;
        protected Emulator _emulator;

        public CustomAction ClickAction;

        protected List<old_BaseControl> _controls;

        protected old_BaseControl _parent;

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

        protected old_BaseControl(old_BaseControl parent, Emulator emulator)
        {
            _texture = Emulator._Pixel;
            _font = Emulator._Font;
            _parent = parent;
            _controls = new List<old_BaseControl>();
            _emulator = emulator;

            if (parent != null)
            {
                Width = parent.Width;
                Height= parent.Height;
            }
            else
            {
                Width = 100; 
                Height = 100;
            }

            // Default values
            Visible = true;
            Enabled = true;
            Left = 0;
            Top = 0;
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
        }

        #endregion


        #region Properties

        public int Left { get; set; }
        public int Top { get; set; }
        public virtual int Width { get; set; }
        public virtual int Height { get; set; }
        public string Caption { get; set; }

        public bool Visible { get; set; }
        public bool Enabled { get; set; }
        public virtual int Padding { get; set; }

        public List<old_BaseControl> Controls
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
                    return _parent.GlobalPosition + new Vector2(Left, Top);
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
            if (!Visible)
            {
                return;
            }
            // Update embedded controls first (click priority from top to bottom)
            if (_controls.Count > 0)
            {
                foreach (old_BaseControl control in _controls)
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
            if (IsHover())
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
            if (Caption == null)
            {
                Caption = "";
            }
            Vector2 captionSize = _font.MeasureString(Caption);
            Vector2 captionPos = new Vector2(GlobalPosition.X + (Width - captionSize.X) / 2,
                                             GlobalPosition.Y + (Height - captionSize.Y) / 2 + captionSize.Y / 6);
            spriteBatch.DrawString(_font, Caption, captionPos, _textColor);

            // Draw embedded controls last (draw from bottom to top)
            if (_controls.Count > 0)
            {
                foreach (old_BaseControl control in _controls)
                {
                    control.Draw(spriteBatch);
                }
            }
        }

        public bool IsHover()
        {
            bool hover = false;
            if (Input.MousePosX >= GlobalPosition.X &&
                Input.MousePosX < GlobalPosition.X + Width &&
                Input.MousePosY >= GlobalPosition.Y &&
                Input.MousePosY < GlobalPosition.Y + Height)
            {
                hover = true;
            }
            
            foreach (old_BaseControl control in _controls)
            {
                if (control.Visible)
                    hover |= control.IsHover();
            }
            
            return hover;
        }


        public old_Button AddButton(int width, int height, string caption="")
        {
            old_Button newControl = new old_Button(this, _emulator, width, height, caption);
            _controls.Add(newControl);
            return newControl;
        }

        public old_MenuButton AddMenuButton(int width, int height, string caption="")
        {
            old_MenuButton newControl = new old_MenuButton(this, _emulator, width, height, caption);
            _controls.Add(newControl);
            return newControl;
        }

        public old_Label AddLabel(string text, int width, int height)
        {
            old_Label newControl = new old_Label(this, _emulator, text, width, height);
            _controls.Add(newControl);
            return newControl;
        }

        public old_Image AddImage(string content, int width, int height)
        {
            old_Image newControl = new old_Image(this, _emulator, content, width, height);
            _controls.Add(newControl);
            return newControl;
        }

        public old_Panel AddPanel(Orientation orientation)
        {
            old_Panel newControl = new old_Panel(this, _emulator, orientation);
            _controls.Add(newControl);
            return newControl;
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
