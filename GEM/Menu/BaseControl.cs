﻿using GEM.Emulation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        public BaseControl Parent;
        protected List<BaseControl> _controls;
        protected Texture2D _pixel;
        Align _horizontalAlign;
        Align _verticalAlign;
        int _width;
        int _height;
        protected float _rotation;
        bool _enabled;
        #endregion

        #region Constructors
        public BaseControl(BaseControl parent)
        {
            Parent = parent;
            _controls = new List<BaseControl>();
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
                foreach (BaseControl child in _controls)
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
                foreach (BaseControl child in _controls)
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
                if (Parent != null)
                {
                    return Parent.PosX + Left;
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
                if (Parent != null)
                {
                    return Parent.PosY + Top;
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

        public bool Visible { get; set; }
        public bool Enabled 
        { 
            get
            {
                bool tmp = true;
                if (Parent != null)
                {
                    tmp = Parent.Enabled;
                }
                return tmp && _enabled;
            }
            set
            {
                _enabled = value;
            }
        }
        #endregion

        #region Methods
        public virtual void Update()
        {
            // update-calculations (defined in inherited class)

            // embedded controls should be updated first (e.g. click priority from top to bottom)
            if (!Visible) return;
            foreach (BaseControl control in _controls)
            {
                control.Update();
            }
        }
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            // draw-method (defined in inherited class)

            // embedded controls drawn last (from bottom to top)
            if (!Visible) return;
            foreach (BaseControl control in _controls)
            {
                control.Draw(spriteBatch);
            }
        }

        public BaseControl Add(BaseControl control)
        {
            _controls.Add(control);
            control.Parent = this;
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

        public void UpdateAlignPosition()
        {
            // align performed before rotation, so preferably use square images
            if (Parent != null)
            {
                switch (HorizontalAlign)
                {
                    case Align.Left:
                        Left = 0;
                        break;
                    case Align.Center:
                        Left = (Parent.Width - Width) / 2;
                        break;
                    case Align.Right:
                        Left = Parent.Width - Width;
                        break;
                }
                switch (VerticalAlign)
                {
                    case Align.Top:
                        Top = 0;
                        break;
                    case Align.Center:
                        Top = (Parent.Height - Height) / 2;
                        break;
                    case Align.Bottom:
                        Top = Parent.Height - Height;
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
