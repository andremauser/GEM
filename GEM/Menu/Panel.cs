using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEM.Menu
{
    internal class Panel : BaseControl
    {
        #region Fields
        #endregion

        #region Constructors
        public Panel(BaseControl parent) : base(parent)
        {
        }
        #endregion

        #region Properties
        #endregion

        #region Methods
        public override void Update()
        {
            base.Update();
            
            // order controls vertically
            int top = 0;
            for (int i = 0; i < _controls.Count; i++)
            {
                _controls[i].Top = top;
                top += _controls[i].Height;

                _controls[i].Left = 0;
            }
        }
        #endregion
    }
}
