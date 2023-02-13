using GEM.Emu;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace GEM.Ctrl
{
    public enum Align
    {
        Left, Center, Right, Top, Bottom
    }
    /// <summary>
    /// abstract base class for nested controls
    /// </summary>
    internal abstract class BaseControl
    {
        #region Fields
        protected BaseControl _parent;
        protected List<BaseControl> _controls;
        protected Texture2D _pixel;
        Align _horizontalAlign;
        Align _verticalAlign;
        int _width;
        int _height;
        #endregion

        #region Constructors
        public BaseControl(BaseControl parent)
        {
            _parent = parent;
            _controls = new List<BaseControl>();
            _pixel = Emulator._Pixel;
            HorizontalAlign = Align.Center;
            VerticalAlign = Align.Center;
            Visible = true;
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
                updateAlignPosition();
                foreach (BaseControl child in _controls)
                {
                    child.updateAlignPosition();
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
                updateAlignPosition();
                foreach (BaseControl child in _controls)
                {
                    child.updateAlignPosition();
                }
            }
        }

        // absolute position on canvas
        public int PosX
        {
            get
            {
                if (_parent != null)
                {
                    return _parent.PosX + Left;
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
                if (_parent != null)
                {
                    return _parent.PosY + Top;
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
                updateAlignPosition();
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
                updateAlignPosition();
            }
        }

        public bool Visible { get; set; }
        #endregion

        #region Methods
        public virtual void Update()
        {
            if (!Visible) return;
            // update-calculations defined in inherited class

            // embedded controls should be updated first (e.g. click priority from top to bottom)
            foreach (BaseControl control in _controls)
            {
                control.Update();
            }
        }
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible) return;
            // draw-method defined in inherited class

            // embedded controls drawn last (from bottom to top)
            foreach (BaseControl control in _controls)
            {
                control.Draw(spriteBatch);
            }
        }

        public BaseControl Add(BaseControl control)
        {
            _controls.Add(control);
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
        public Image AddImage(string image)
        {
            return (Image)Add(new Image(this, image));
        }

        protected void updateAlignPosition()
        {
            if (_parent != null)
            {
                switch (HorizontalAlign)
                {
                    case Align.Left:
                        Left = 0;
                        break;
                    case Align.Center:
                        Left = (_parent.Width - Width) / 2;
                        break;
                    case Align.Right:
                        Left = _parent.Width - Width;
                        break;
                }
                switch (VerticalAlign)
                {
                    case Align.Top:
                        Top = 0;
                        break;
                    case Align.Center:
                        Top = (_parent.Height - Height) / 2;
                        break;
                    case Align.Bottom:
                        Top = _parent.Height - Height;
                        break;
                }
            }
        }
        #endregion
    }
}
