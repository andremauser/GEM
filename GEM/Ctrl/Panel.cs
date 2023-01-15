using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GEM.Ctrl
{
    public enum Orientation
    {
        Vertical,
        Horizontal
    }

    internal class Panel : BaseControl
    {
        Orientation _orientation;
        int _padding;

        public Panel(Texture2D texture, SpriteFont spriteFont, BaseControl parent, Orientation orientation, int padding, CustomAction[] actions) : base(texture, spriteFont, parent, actions)
        {
            _orientation = orientation;
            _padding = padding;
            _hoverEnabled = false;
            _clickEnabled = false;
        }

        public override void Update()
        {
            // arrange embedded controls
            switch (_orientation)
            {
                case Orientation.Vertical:
                    int top = 0;
                    for (int i = 0; i < _controls.Count; i++)
                    {
                        top += _padding;
                        _controls[i].Left = _padding;
                        _controls[i].Top = top;
                        top += _controls[i].Height;
                    }
                    break;
                case Orientation.Horizontal:
                    int left = 0;
                    for (int i = 0; i < _controls.Count; i++)
                    {
                        left += _padding;
                        _controls[i].Left = left;
                        _controls[i].Top = _padding;
                        left += _controls[i].Height;
                    }
                    break;
                default:
                    break;
            }
            base.Update();
        }
    }
}
