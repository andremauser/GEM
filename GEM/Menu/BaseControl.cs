using GEM.Emulation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace GEM.Menu
{
    public enum Align
    {
        Left, Center, Right, Top, Bottom
    }
    /// <summary>
    /// base class for nested controls
    /// </summary>
    internal class BaseControl
    {
        #region Fields
        public BaseControl ParentControl;
        public List<BaseControl> Controls;
        protected Texture2D _pixel;
        Align _horizontalAlign;
        Align _verticalAlign;
        int _width;
        int _height;
        protected float _rotation;
        bool _enabled;
        bool _visible;

        public event EventHandler OnDraw;
        #endregion

        #region Constructors
        public BaseControl(BaseControl parent)
        {
            ParentControl = parent;
            Controls = new List<BaseControl>();
            _pixel = Emulator._Pixel;
            HorizontalAlign = Align.Center;
            VerticalAlign = Align.Center;
            Visible = true;
            Enabled = true;
        }
        #endregion

        #region Properties
        // offset to parent control
        public int Left { get; set; }
        public int Top { get; set; }

        // control size
        public virtual int Width 
        {
            get
            {
                return _width;
            }
            set
            {
                _width = value;
                UpdateAlignPosition();
                foreach (BaseControl child in Controls)
                {
                    child.UpdateAlignPosition();
                }
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
                UpdateAlignPosition();
                foreach (BaseControl child in Controls)
                {
                    child.UpdateAlignPosition();
                }
            }
        }

        // absolute position on canvas
        public int PosX
        {
            get
            {
                if (ParentControl != null)
                {
                    return ParentControl.PosX + Left;
                }
                else
                {
                    return Left;
                }
            }
        }
        public int PosY
        {
            get
            {
                if (ParentControl != null)
                {
                    return ParentControl.PosY + Top;
                }
                else
                {
                    return Top;
                }
            }
        }

        // align within parent control
        public Align HorizontalAlign
        {
            get
            {
                return _horizontalAlign;
            }
            set
            {
                _horizontalAlign = value;
                UpdateAlignPosition();
            }
        }
        public Align VerticalAlign
        {
            get
            {
                return _verticalAlign;
            }
            set
            {
                _verticalAlign = value;
                UpdateAlignPosition();
            }
        }
        public int Padding { get; set; }

        public bool Visible
        {
            get
            {
                if (!_visible)
                {
                    // if not visible -> return this
                    return _visible;
                }
                else
                {
                    if (ParentControl != null)
                    {
                        // if visible and has parent, both must be visible
                        return _visible && ParentControl.Visible;
                    }
                    else
                    {
                        // visible and top level -> return this
                        return _visible;
                    }
                }
            }
            set
            {
                _visible = value;
            }
        }
        public bool Enabled 
        { 
            get
            {
                bool tmp = true;
                if (ParentControl != null)
                {
                    tmp = ParentControl.Enabled;
                }
                return tmp && _enabled;
            }
            set
            {
                _enabled = value;
            }
        }

        public BaseControl RootControl
        {
            get
            {
                if (ParentControl == null)
                {
                    return this;
                }
                else
                {
                    return ParentControl.RootControl;
                }
            }
        }
        #endregion

        #region Methods
        public virtual void Update(GameTime gameTime)
        {
            // update-calculations (defined in inherited class)

            // embedded controls should be updated first (e.g. click priority from top to bottom)
            foreach (BaseControl control in Controls)
            {
                control.Update(gameTime);
            }
        }
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            // draw-method (defined in inherited class)

            // embedded controls drawn last (from bottom to top)
            if (!Visible) return;
            OnDraw?.Invoke(this, EventArgs.Empty);
            foreach (BaseControl control in Controls)
            {
                control.Draw(spriteBatch);
            }
        }

        public BaseControl Add(BaseControl control)
        {
            Controls.Add(control);
            control.ParentControl = this;
            return control;
        }
        public Label AddLabel(string caption)
        {
            return (Label)Add(new Label(this, caption));
        }
        public Panel AddPanel()
        {
            return (Panel)Add(new Panel(this));
        }
        public Image AddImage(string image, int imagesPerRow = 1)
        {
            return (Image)Add(new Image(this, image, imagesPerRow));
        }
        public Image AddImage(Texture2D image, int imagesPerRow = 1)
        {
            return (Image)Add(new Image(this, image, imagesPerRow));
        }
        public SwitchControl AddSwitch()
        {
            return (SwitchControl)Add(new SwitchControl(this));
        }

        public void UpdateAlignPosition()
        {
            // align performed before rotation, so preferably use square images
            if (ParentControl != null)
            {
                switch (HorizontalAlign)
                {
                    case Align.Left:
                        Left = Padding;
                        break;
                    case Align.Center:
                        Left = (ParentControl.Width - Width) / 2;
                        break;
                    case Align.Right:
                        Left = ParentControl.Width - Width - Padding;
                        break;
                }
                switch (VerticalAlign)
                {
                    case Align.Top:
                        Top = Padding;
                        break;
                    case Align.Center:
                        Top = (ParentControl.Height - Height) / 2;
                        break;
                    case Align.Bottom:
                        Top = ParentControl.Height - Height - Padding;
                        break;
                }
            }
        }

        public void SetRotation(float radian)
        {
            _rotation = radian;
        }
        public void SetRotation(int degree)
        {
            _rotation = degree / 180f * MathHelper.Pi;
        }
                
        #endregion
    }
}
