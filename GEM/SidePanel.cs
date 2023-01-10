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

        public SidePanel(Texture2D texture, SpriteFont spriteFont, CustomControl parent) : base(texture, spriteFont, parent)
        {
            Left = 0;
            Top = 0;
            Width = 50;
            Height = 200;
            BackColorIdle = new Color(0,0,0,0.9f);
            BackColorHover = BackColorIdle;
            ClickEnabled = false;

            Controls.Add(new SidePanelButton(texture, spriteFont, this) { Left = 0, Top = 0 , Caption = "ON",  BackColorHover = Color.Green });
            Controls.Add(new SidePanelButton(texture, spriteFont, this) { Left = 0, Top = 50, Caption = "OFF", BackColorHover = Color.Red });

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
            foreach (CustomControl control in Controls)
            {
                control.CustomState = CustomState.Expanded;
            }
            CustomState = CustomState.Expanded;
            Controls[0].Caption = "Power ON";
            Controls[1].Caption = "Power OFF";
            base.customHoverAction();
        }

        internal override void customUnHoverAction()
        {
            foreach (CustomControl control in Controls)
            {
                control.CustomState = CustomState.Collapsed;
            }
            CustomState = CustomState.Collapsed;
            Controls[0].Caption = "ON";
            Controls[1].Caption = "OFF";
            base.customUnHoverAction();
        }
    }
}
