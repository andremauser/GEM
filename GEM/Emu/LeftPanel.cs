using GEM.Ctrl;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEM.Emu
{
    internal class LeftPanel : Panel
    {
        public LeftPanel(BaseControl parent, Emulator emulator) : base(parent, Orientation.Vertical, 0, emulator)
        {
            _left = 0;
            Top = 0;
            Width = 60;
            Height = 200;
            _backColorIdle = new Color(0, 0, 0, 0.8f);

            _controls.Add(new CartridgeMenu(this, emulator, Direction.Right));
            _controls.Add(new CartridgeMenu(this, emulator, Direction.Right));
            _controls.Add(new CartridgeMenu(this, emulator, Direction.Right));
            _controls.Add(new CartridgeMenu(this, emulator, Direction.Right));

        }
    }
}
