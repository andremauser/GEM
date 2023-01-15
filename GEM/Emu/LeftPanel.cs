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
        public LeftPanel(Texture2D texture, SpriteFont spriteFont, BaseControl parent, CustomAction[] actions) : base(texture, spriteFont, parent, Orientation.Vertical, 0, actions)
        {
            _left = 0;
            Top = 0;
            Width = 50;
            Height = 200;
            _backColorIdle = new Color(0, 0, 0, 0.9f);

            _controls.Add(new CartridgeMenu(texture, spriteFont, this, actions, Direction.Right));
            _controls.Add(new CartridgeMenu(texture, spriteFont, this, actions, Direction.Right));

            /*
            old_SidePanelButton btnOn = new old_SidePanelButton(texture, spriteFont, this, actions);
            btnOn.Left = 0;
            btnOn.Top = 0;
            btnOn.BackColorHover = Color.Green;
            btnOn.ClickAction = actions[0];
            old_ButtonCaption capOn = new old_ButtonCaption(texture, spriteFont, btnOn, actions);
            capOn.CollapsedCaption = "ON";
            capOn.ExpandedCaption = "Power ON";
            Controls.Add(btnOn);
            btnOn.Controls.Add(capOn);

            old_SidePanelButton btnOff = new old_SidePanelButton(texture, spriteFont, this, actions);
            btnOff.Left = 0;
            btnOff.Top = 50;
            btnOff.BackColorHover = Color.Red;
            btnOff.ClickAction = actions[1];
            old_ButtonCaption capOff = new old_ButtonCaption(texture, spriteFont, btnOff, actions);
            capOff.CollapsedCaption = "OFF";
            capOff.ExpandedCaption = "Power OFF";
            Controls.Add(btnOff);
            btnOff.Controls.Add(capOff);
            */
        }
    }
}
