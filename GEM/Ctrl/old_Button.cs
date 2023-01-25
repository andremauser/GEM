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
    internal class old_Button : old_BaseControl
    {
        public old_Button(old_BaseControl parent, Emulator emulator, int width, int height, string caption = "") : base(parent, emulator)
        {
            BackColorIdle = Color.Black;
            BackColorHover = Color.DarkOrchid;
            BackColorPress = Color.DarkSeaGreen;

            TextColorIdle = Color.White;
            TextColorHover = Color.White;
            TextColorPress = Color.White;

            Width = width; 
            Height = height;
            Caption = caption;
        }

    }
}
