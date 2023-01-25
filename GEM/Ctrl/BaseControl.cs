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
        protected List<BaseControl> _childs;
        Align _horizontalAlign;
        Align _verticalAlign;
        int _width;
        int _height;
        #endregion

        #region Constructors
        public BaseControl(BaseControl parent)
        {
            _parent = parent;
            _childs = new List<BaseControl>();
            HorizontalAlign = Align.Center;
            VerticalAlign = Align.Center;
        }
        #endregion

        #region Properties
        // offset to parent control
        public int Left { get; set; }
        public int Top { get; set; }

        // control size
        public int Width 
        {
            get
            {
                return _width;
            }
            set
            {
                _width = value;
                updateAlignPosition();
                foreach (BaseControl child in _childs)
                {
                    child.updateAlignPosition();
                }
            }
        }
        public int Height 
        {
            get
            {
                return _height;
            }
            set
            {
                _height = value;
                updateAlignPosition();
                foreach (BaseControl child in _childs)
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
        #endregion

        #region Methods
        public virtual void Update()
        {
            // update calculations defined in inherited class

            // childs updated first - call base.Update first! (e.g. click priority from top to bottom)
            foreach (BaseControl child in _childs)
            {
                child.Update();
            }
        }
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            // draw method defined in inherited class

            // childs drawn last (from bottom to top)
            foreach (BaseControl child in _childs)
            {
                child.Draw(spriteBatch);
            }
        }

        public void Add(BaseControl control)
        {
            _childs.Add(control);
        }
        public void AddLabel(string caption)
        {
            Add(new Label(this, caption));
        }
        public void AddButton()
        {
            Add(new Button(this));
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
