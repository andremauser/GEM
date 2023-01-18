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
            BackColorIdle = Color.Black;
            BackColorHover = Color.DarkOrchid;
            BackColorPress = Color.DarkSeaGreen;

            TextColorIdle = Color.White;
            TextColorHover = Color.White;
            TextColorPress = Color.White;
        }

    }
}
