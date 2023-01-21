using GEM.Emu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEM.Ctrl
{
    internal class MenuButton : Button
    {
        public Panel MenuPanel;

        public MenuButton(BaseControl parent, Emulator emulator) : base(parent, emulator)
        {
            MenuPanel = new Panel(this, emulator, Orientation.Vertical);
            _controls.Add(MenuPanel);
            MenuPanel.Left = Width;
            MenuPanel.Visible = false;
        }

        public override int Width 
        { 
            get => base.Width;
            set
            {
                base.Width = value;
                if(MenuPanel != null)
                    MenuPanel.Left = Width;
            }
        }

        internal override void onClick()
        {
            MenuPanel.Visible = !MenuPanel.Visible;
            base.onClick();
        }
    }
}
