using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEM
{
    internal abstract class Control
    {
        public int Left { get; set; }
        public int Top { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public bool Visible { get; set; }
        public bool Enabled { get; set; }

        protected Control()
        {
            Left = 0;
            Top = 0;
            Width = 100;
            Height = 20;

            Visible = true;
        }

        public abstract void Update();

        public abstract void Draw(SpriteBatch spriteBatch);

    }
}
