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
    internal class Button : BaseControl
    {
        public Button(BaseControl parent, Emulator emulator) : base(parent, emulator)
        {
            _backColorIdle = Color.Black;
            _backColorHover = Color.DarkMagenta;
            _backColorPress = Color.White;
            _textColorIdle = Color.White;
            _textColorHover = Color.White;
            _textColorPress = Color.DarkMagenta;
        }

    }
}
