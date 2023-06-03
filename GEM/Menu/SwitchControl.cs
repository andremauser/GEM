using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEM.Menu
{
    internal class SwitchControl : BaseControl
    {
        #region Fields
        Image _switchImage;
        #endregion

        #region Constructors
        public SwitchControl(BaseControl parent, String caption) : base(parent)
        {
            _switchImage = new Image(this, "switch", 2);
            Caption = new Label(this, caption);

            if (caption != "" && parent is MenuButton)
            {
                ((MenuButton)parent).Label.Caption = "";
                ((MenuButton)parent).Label = Caption;
            }
            ((MenuButton)parent).Image = _switchImage;

            // add to controls
            Controls.Add(_switchImage);
            Controls.Add(Caption);

            // size
            Width = parent.Width;
            Height = parent.Height;

            // positions
            Caption.Left = 15;
            _switchImage.Left = Width - _switchImage.Width - 5;
        }
        #endregion

        #region Properties
        Label Caption { get; set; }
        public override int Width 
        { 
            get => base.Width;
            set
            {
                base.Width = value;
                _switchImage.Left = Width - _switchImage.Width;
            }
        }
        #endregion

        #region Methods
        public void SetSwitch(bool state)
        {
            _switchImage.ImageIndex = Convert.ToInt32(state);
        }
        #endregion
    }
}
