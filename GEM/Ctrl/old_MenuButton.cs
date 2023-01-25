using GEM.Emu;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEM.Ctrl
{
    internal class old_MenuButton : old_Button
    {
        public old_Panel MenuPanel;

        public old_MenuButton(old_BaseControl parent, Emulator emulator, int width, int height, string caption="") : base(parent, emulator, width, height)
        {
            MenuPanel = new old_Panel(this, emulator, Orientation.Vertical);
            _controls.Add(MenuPanel);
            MenuPanel.Left = Width;
            MenuPanel.Visible = false;
            Caption = caption;
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
