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

            Controls.Add(new SidePanelButton(texture, spriteFont, this) { Left = 0, Top = 0 , BackColorHover = Color.Green });
            Controls.Add(new SidePanelButton(texture, spriteFont, this) { Left = 0, Top = 50, BackColorHover = Color.Red });

            Controls[0].ClickAction = emulator.GameboyOn;
            Controls[1].ClickAction = emulator.GameboyOff;

            Controls[0].Controls.Add(new ButtonCaption(texture, spriteFont, Controls[0]) { CollapsedCaption = "ON",  ExpandedCaption = "Power ON" });
            Controls[1].Controls.Add(new ButtonCaption(texture, spriteFont, Controls[1]) { CollapsedCaption = "OFF", ExpandedCaption = "Power OFF" });
        }

        public List<CustomAction> ActionList { get; set; }

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
