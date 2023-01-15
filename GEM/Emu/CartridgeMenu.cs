using GEM.Ctrl;
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
        public CartridgeMenu(Texture2D texture, SpriteFont spriteFont, BaseControl parent, CustomAction[] actions, Direction direction) : base(texture, spriteFont, parent, actions, direction)
        {
            _root = new Label(texture, spriteFont, this, actions);
            _root.Caption = "GAME";
            
            _root.Width= 50;
            _root.Height= 50;

            _controls.Add(new Button(texture, spriteFont, this, actions) { Width = 200, Height = 50, Caption = "ON", ClickAction = actions[0] });
            _controls.Add(new Button(texture, spriteFont, this, actions) { Width = 200, Height = 50, Caption = "OFF", ClickAction = actions[1] });
        }

    }
}
