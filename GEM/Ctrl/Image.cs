using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEM.Ctrl
{
    internal class Image : BaseControl
    {
        public Image(Texture2D texture, SpriteFont spriteFont, BaseControl parent, CustomAction[] actions) : base(texture, spriteFont, parent, actions)
        {
        }
    }
}
