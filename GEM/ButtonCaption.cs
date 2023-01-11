using Microsoft.Xna;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEM
{
    internal class ButtonCaption : CustomControl
    {
        public ButtonCaption(Texture2D texture, SpriteFont spriteFont, CustomControl parent) : base(texture, spriteFont, parent)
        {
            Width = 50;
            Height = 50;

            HoverEnabled= false;
            ClickEnabled= false;

            BackColorIdle = Color.Transparent;

        }

        public override void Update()
        {
            switch (CustomState)
            {
                case CustomState.Collapsed:
                    HorizontalTextAlign = Align.Center;
                    Left = 0;
                    break;
                case CustomState.Expanded:
                    HorizontalTextAlign = Align.Left;
                    Left = 60;
                    break;
                default:
                    break;
            }
            base.Update();
        }
    }
}
