using GEM.Emulation;
using GEM.Struct;
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
    
    internal class BaseControl
    {
        #region Fields
        protected Texture2D _pixel;

        // property fields
        protected float _rotation;
        bool _enabled;
        bool _visible;

        // events
        public event EventHandler OnDraw;
        #endregion

        #region Constructors
        public BaseControl(BaseControl parent)
        {
            _pixel = Emulator._Pixel;

            // hierarchy
            ParentControl = parent;
            EmbeddedControls = new List<BaseControl>();

            // default values
            HorizontalAlign = Align.Left;
            VerticalAlign = Align.Top;
            Visible = true;
            Enabled = true;
            RotationPivotX = 0.5f;
            RotationPivotY = 0.5f;
            Padding = new Offset(0);
            Margin = new Offset(0);
        }
        #endregion

        #region Properties
        // location (top left corner)
        public int LocationX
        {
            get
            {
                int locX = Left;
                if (ParentControl != null)
                {
                    if (HorizontalAlign == Align.Left)
                    {
                        locX += ParentControl.LocationX
                                + ParentControl.Padding.Left
                                + Margin.Left;
                    }
                    if (HorizontalAlign == Align.Center)
                    {
                        locX += ParentControl.LocationX
                                + (ParentControl.Width / 2)
                                - (Width / 2);
                    }
                    if (HorizontalAlign == Align.Right)
                    {
                        locX += ParentControl.LocationX
                                + ParentControl.Width
                                - ParentControl.Padding.Right
                                - Margin.Right
                                - Width;
                    }
                }
                return locX;
            }
        }
        public int LocationY
        {
            get
            {
                int locY = Top;
                if (ParentControl != null)
                {
                    if (VerticalAlign == Align.Top)
                    {
                        locY += ParentControl.LocationY
                                + ParentControl.Padding.Top
                                + Margin.Top;
                    }
                    if (VerticalAlign == Align.Center)
                    {
                        locY += ParentControl.LocationY
                                + (ParentControl.Height / 2)
                                - (Height / 2);
                    }
                    if (VerticalAlign == Align.Bottom)
                    {
                        locY += ParentControl.LocationY
                                + ParentControl.Height
                                - ParentControl.Padding.Bottom
                                - Margin.Bottom
                                - Height;
                    }
                }
                return locY;
            }
        }
        public int Left { get; set; }
        public int Top { get; set; }
        public Offset Padding { get; set; }
        public Offset Margin { get; set; }
        public Align HorizontalAlign { get; set; }
        public Align VerticalAlign { get; set; }

        // rotation
        public float Rotation
        {
            get { return _rotation * 180f / MathHelper.Pi; }
            set { _rotation = value / 180f * MathHelper.Pi; }
        }
        public float RotationRad
        {
            get { return _rotation; }
            set { _rotation = value; }
        }
        public float RotationPivotX { get; set; }
        public float RotationPivotY { get; set; }

        // scale
        public virtual int Width { get; set; }
        public virtual int Height { get; set; }

        // state
        public bool Visible
        {
            get
            {
                if (ParentControl != null)
                {
                    return _visible && ParentControl.Visible;
                }
                else
                {
                    return _visible;
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
                if (ParentControl != null)
                {
                    return _enabled && ParentControl.Enabled;
                }
                else
                {
                    return _enabled;
                }
            }
            set
            {
                _enabled = value;
            }
        }

        // hierarchy
        public BaseControl ParentControl { get; set; }
        public BaseControl RootControl
        {
            get
            {
                if (ParentControl != null)
                {
                    return ParentControl.RootControl;
                }
                else
                {
                    return this;
                }
            }
        }
        public List<BaseControl> EmbeddedControls { get; set; }
        #endregion

        #region Methods
        public virtual void Update(GameTime gameTime)
        {
            // embedded controls should be updated first (e.g. click priority from top to bottom)
            foreach (BaseControl control in EmbeddedControls)
            {
                control.Update(gameTime);
            }
        }
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible) return;
            OnDraw?.Invoke(this, EventArgs.Empty);

            // embedded controls drawn last (from bottom to top)
            foreach (BaseControl control in EmbeddedControls)
            {
                control.Draw(spriteBatch);
            }
        }

        public BaseControl Add(BaseControl control)
        {
            EmbeddedControls.Add(control);
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

     
        #endregion
    }
}
