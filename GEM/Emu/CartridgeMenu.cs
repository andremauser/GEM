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
        BaseControl _subCartridgeTitle;
        CartridgeLoadMenu _subCartridgeOpen;
        BaseControl _subCartridgeEject;

        public CartridgeMenu(BaseControl parent, Emulator emulator) : base(parent, emulator)
        {
            _rootButton.AddControl(new Image(_rootButton, emulator, "cartridge") { Width = 60, Height = 60, Padding = 5 }) ;
            
            _subCartridgeTitle = new Label(this, emulator, "") { Width = 200, Height = 40, };
            _controls.Add(_subCartridgeTitle);

            _subCartridgeOpen = new CartridgeLoadMenu(this, emulator) ;
            _controls.Add(_subCartridgeOpen);

            _subCartridgeEject = new Button(this, emulator) { Width = 200, Height = 40, BackColorIdle = Color.Transparent, BackColorHover = Color.Firebrick, Caption = "Eject cartridge", ClickAction = _emulator.EjectCartridge };
            _controls.Add(_subCartridgeEject);

        }

        public override void Update()
        {
            _subCartridgeTitle.Caption = _emulator.CartridgeTitle;
            base.Update();
        }

    }
}
