using GEM.Emu;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEM.Ctrl
{
    internal class Label : BaseControl
    {
        public Label(BaseControl parent, Emulator emulator, string caption) : base(parent, emulator)
        {
            BackColorIdle = Color.Transparent;
            BackColorHover= Color.Transparent;
            BackColorPress= Color.Transparent;
            TextColorIdle = Color.White;
            TextColorHover = Color.White;
            TextColorPress = Color.White;
            _clickEnabled = false;
            _hoverEnabled= false;
            Caption = caption;
        }
    }
}
