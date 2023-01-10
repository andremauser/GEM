using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GEM
{
    internal class SidePanelButton : CustomControl
    {
        public SidePanelButton(Texture2D texture, SpriteFont spriteFont, CustomControl parent) : base(texture, spriteFont, parent)
        {
            Width = 50;
            Height = 50;
            BackColorIdle = Color.Transparent;
            BackColorHover = Color.DarkMagenta;
            BackColorPress = Color.White;

            TextColorIdle = Color.Gray;
            TextColorHover = Color.White;
            TextColorPress = Color.Black;

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

    }
}
