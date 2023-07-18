using System;

namespace GEM.Menu
{
    internal class SwitchControl : BaseControl
    {
        #region Fields
        Image _switchImage;
        #endregion

        #region Constructors
        public SwitchControl(BaseControl parent) : base(parent)
        {
            _switchImage = new Image(this, "switch", 2);
            ((MenuButton)parent).Image = _switchImage; // for updating color

            // add to controls
            EmbeddedControls.Add(_switchImage);
            HorizontalAlign = Align.Right;
            VerticalAlign = Align.Center;
            Margin = 5;
        }
        #endregion

        #region Properties
        public override int Width
        {
            get
            {
                return _switchImage.Width;
            }
        }
        public override int Height
        {
            get
            {
                return _switchImage.Height;
            }
        }
        #endregion

        #region Methods
        public void UpdateSwitch(bool state)
        {
            _switchImage.ImageIndex = Convert.ToInt32(state);
        }
        #endregion
    }
}
