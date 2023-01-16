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
    internal class CartridgeMenu : Submenu
    {
        public CartridgeMenu(BaseControl parent, Emulator emulator, Direction direction) : base(parent, emulator, direction)
        {
            _root = new Image(this, emulator, "cartridge");
            _controls.Add(_root);
            
            _root.Width= 60;
            _root.Height= 60;
            _root.Padding = 5;

            _controls.Add(new Label(this, emulator) { Width = 200, Height = 40 });
            _controls.Add(new Button(this, emulator) { Width = 200, Height = 40, Caption = "Open ROM file", ClickAction = _emulator.Nothing });
            _controls.Add(new Button(this, emulator) { Width = 200, Height = 40, Caption = "Eject cartridge", ClickAction = _emulator.EjectCartridge });
        }

        public override void Update()
        {
            _controls[1].Caption = _emulator.CartridgeTitle;
            base.Update();
        }

    }
}
