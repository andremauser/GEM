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
        public Label(BaseControl parent, Emulator emulator) : base(parent, emulator)
        {
            _backColorIdle = Color.Transparent;
            _backColorHover= Color.Transparent;
            _backColorPress= Color.Transparent;
            _textColorIdle = Color.White;
            _textColorHover = Color.White;
            _textColorPress = Color.White;
            _clickEnabled = false;
            _hoverEnabled= false;
        }
    }
}
