using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEM.Menu
{
    public enum Direction
    {
        Vertical,
        Horizontal
    }

    internal class Panel : BaseControl
    {
        #region Fields
        #endregion

        #region Constructors
        public Panel(BaseControl parent) : base(parent)
        {
            // default values
            Direction = Direction.Vertical;
        }
        #endregion

        #region Properties
        public Direction Direction { get; set; }
        public override int Width 
        {
            get
            {
                switch (Direction)
                {
                    case Direction.Vertical:
                        int w = 0;
                        foreach (BaseControl control in _controls)
                        {
                            w = Math.Max(w, control.Width);
                        }
                        return w;
                    case Direction.Horizontal:
                        w = 0;
                        foreach(BaseControl control in _controls)
                        {
                            w += control.Width;
                        }
                        return w;
                }
                return base.Width;
            }
        }
        public override int Height
        {
            get
            {
                switch (Direction)
                {
                    case Direction.Horizontal:
                        int h = 0;
                        foreach (BaseControl control in _controls)
                        {
                            h = Math.Max(h, control.Height);
                        }
                        return h;
                    case Direction.Vertical:
                        h = 0;
                        foreach (BaseControl control in _controls)
                        {
                            h += control.Height;
                        }
                        return h;
                }
                return base.Height;
            }
        }
        #endregion

        #region Methods
        public override void Update()
        {
            base.Update();
            
            if (Direction == Direction.Vertical)
            {
                // order controls vertically
                int top = 0;
                for (int i = 0; i < _controls.Count; i++)
                {
                    _controls[i].Top = top;
                    top += _controls[i].Height;

                    _controls[i].Left = 0;
                }
            }

            if (Direction == Direction.Horizontal)
            {
                // order controls horizontally
                int left = 0;
                for (int i = 0; i < _controls.Count; i++)
                {
                    _controls[i].Left = left;
                    left += _controls[i].Width;

                    _controls[i].Top = 0;
                }
            }
        }
        #endregion
    }
}
