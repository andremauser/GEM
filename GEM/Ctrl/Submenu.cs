using GEM.Emu;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEM.Ctrl
{
    internal class Submenu : BaseControl
    {
        /// <summary>
        /// base class for submenu opening to the right - entries one above the other
        /// </summary>

        #region Fields

        protected Button _rootButton;
        protected int _rootButtonMinHeight;

        #endregion

        #region Constructors

        public Submenu(BaseControl parent, Emulator emulator) : base(parent, emulator)
        {
            _rootButton = new Button(this, emulator);
            _controls.Add(_rootButton);

            BackColorIdle = Color.Transparent;
            BackColorHover = new Color(0f, 0f, 0f, 0.8f);
            BackColorPress = new Color(0f, 0f, 0f, 0.8f);

            _rootButtonMinHeight = 60;

            _rootButton.Height = _rootButtonMinHeight;
            _rootButton.Width = _rootButtonMinHeight;
        }

        #endregion

        #region Properties

        public override int Width
        {
            get
            {
                int maxControlWidth = 0;
                for (int i = 1; i < _controls.Count; i++)
                {
                    if (_controls[i].Visible)
                        maxControlWidth = Math.Max(_controls[i].Width, maxControlWidth);
                }
                int width = _rootButton.Width + maxControlWidth;

                return width;
            }
        }

        public override int Height
        {
            get
            {
                int totalHeight = 0;
                for (int i = 1; i < _controls.Count; i++)
                {
                    if (_controls[i].Visible)
                        totalHeight += _controls[i].Height;
                }
                int height = Math.Max(_rootButton.Height, totalHeight);

                return height;
            }
        }

        #endregion

        #region Methods

        public override void Update()
        {
            _rootButton.Height = _rootButtonMinHeight;

            // arrange embedded controls
            int top = 0;
            for (int i = 1; i < _controls.Count; i++)
            {
                if (_controls[i].Visible)
                {
                    _controls[i].Left = _rootButton.Width;
                    _controls[i].Top = top;
                    top += _controls[i].Height;
                }
            }

            _rootButton.Height = this.Height;

            base.Update();
        }

        internal override void onClick()
        {
            if (_customState == CustomState.Collapsed)
            {
                for (int i = 1; i < _controls.Count; i++)
                {
                    _controls[i].Visible = true;
                }
                _rootButton.BackColorIdle = Color.DarkOrchid;
                CustomState = CustomState.Expanded;
            }
            else
            {
                for (int i = 1; i < _controls.Count; i++)
                {
                    _controls[i].Visible = false;
                }
                _rootButton.BackColorIdle = Color.Transparent;
                CustomState = CustomState.Collapsed;
            }
            base.onClick();
        }

        internal override void onHoverOut()
        {
            for (int i = 1; i < _controls.Count; i++)
            {
                _controls[i].Visible = false;
            }
            _rootButton.BackColorIdle = Color.Transparent;
            CustomState = CustomState.Collapsed;
            base.onHoverOut();
        }

        #endregion

    }
}
