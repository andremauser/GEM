using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEM.Ctrl
{
    internal class Button : BaseControl
    {
        public Button(Texture2D texture, SpriteFont spriteFont, BaseControl parent, CustomAction[] actions) : base(texture, spriteFont, parent, actions)
        {
            _backColorIdle = Color.Transparent;
            _backColorHover = Color.Magenta;
            _backColorPress = Color.White;
            _textColorIdle = Color.White;
            _textColorHover = Color.White;
            _textColorPress = Color.Black;
        }

        public override void Update()
        {
            switch (_customState)
            {
                case CustomState.Collapsed:
                    _clickEnabled= true;
                    _hoverEnabled= true;
                    break;
                case CustomState.Expanded:
                    _clickEnabled= false;
                    _hoverEnabled= false;
                    break;
                default:
                    break;
            }
            base.Update();
        }
    }
}
