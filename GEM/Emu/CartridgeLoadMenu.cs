using GEM.Ctrl;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEM.Emu
{
    internal class CartridgeLoadMenu : Submenu
    {
        public CartridgeLoadMenu(BaseControl parent, Emulator emulator) : base(parent, emulator)
        {
            _rootButton.Width = 200;
            _rootButton.Height = 40;
            _rootButtonMinHeight = 40;

            _rootButton.BackColorIdle = Color.Transparent;
            _rootButton.AddControl(new Label(_rootButton, emulator, "Open ROM file") { Width = 200, Height = 40 });

            AddControl(new Button(this, emulator) { Width = 200, Height = 40, BackColorIdle = Color.Transparent, Caption = "Test 1", ClickAction = _emulator.DoNothing });
            AddControl(new Button(this, emulator) { Width = 200, Height = 40, BackColorIdle = Color.Transparent, Caption = "Test 2", ClickAction = _emulator.DoNothing });
            AddControl(new Button(this, emulator) { Width = 200, Height = 40, BackColorIdle = Color.Transparent, Caption = "Test 3", ClickAction = _emulator.DoNothing });
            AddControl(new Button(this, emulator) { Width = 200, Height = 40, BackColorIdle = Color.Transparent, Caption = "Test 4", ClickAction = _emulator.DoNothing });
        }
    }
}
