using GEM.Emu;
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

        public Panel(BaseControl parent, Emulator emulator, Orientation orientation) : base(parent, emulator)
        {
            _orientation = orientation;
            _hoverEnabled = false;
            _clickEnabled = false;
            BackColorIdle = Color.Transparent;
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
                        top += Padding;
                        _controls[i].Left = Padding;
                        _controls[i].Top = top;
                        top += _controls[i].Height;
                    }
                    break;
                case Orientation.Horizontal:
                    int left = 0;
                    for (int i = 0; i < _controls.Count; i++)
                    {
                        left += Padding;
                        _controls[i].Left = left;
                        _controls[i].Top = Padding;
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
