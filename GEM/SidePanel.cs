using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEM
{

    internal class SidePanel : CustomControl
    {

        public SidePanel(Texture2D texture, SpriteFont spriteFont, CustomControl parent, Emulator emulator) : base(texture, spriteFont, parent)
        {
            Left = 0;
            Top = 0;
            Width = 50;
            Height = 200;
            BackColorIdle = new Color(0,0,0,0.9f);
            BackColorHover = BackColorIdle;
            ClickEnabled = false;

            SidePanelButton btnOn = new SidePanelButton(texture, spriteFont, this);
                btnOn.Left = 0;
                btnOn.Top = 0;
                btnOn.BackColorHover = Color.Green;
                btnOn.ClickAction = emulator.GameboyOn;
            ButtonCaption capOn = new ButtonCaption(texture, spriteFont, btnOn);
                capOn.CollapsedCaption = "ON";
                capOn.ExpandedCaption = "Power ON";
            Controls.Add(btnOn);
            btnOn.Controls.Add(capOn);

            SidePanelButton btnOff = new SidePanelButton(texture, spriteFont, this);
                btnOff.Left = 0;
                btnOff.Top = 50;
                btnOff.BackColorHover = Color.Red;
                btnOff.ClickAction = emulator.GameboyOff;
            ButtonCaption capOff = new ButtonCaption(texture, spriteFont, btnOff);
                capOff.CollapsedCaption = "OFF";
                capOff.ExpandedCaption = "Power OFF";
            Controls.Add(btnOff);
            btnOff.Controls.Add(capOff);

        }

        public override void Update()
        {
            switch (CustomState)
            {
                case CustomState.Collapsed:
                    Width = 50;
                    break;
                case CustomState.Expanded:
                    Width = 200;
                    break;
                default:
                    break;
            }
            base.Update();
        }

        internal override void customHoverAction()
        {
            CustomState = CustomState.Expanded;
            base.customHoverAction();
        }

        internal override void customUnHoverAction()
        {
            CustomState = CustomState.Collapsed;
            base.customUnHoverAction();
        }
    }
}
