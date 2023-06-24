using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Text;

namespace GEM.Menu
{
    public enum NotificationType
    {
        Success,
        Information,
        Alert
    }

    internal class Notification : BaseControl
    {
        #region Fields
        double _seconds;
        MenuButton _button;
        public static int HEIGHT = 80;
        #endregion

        #region Constructors
        public Notification(BaseControl parentControl, string message, NotificationType type, double seconds) : base(parentControl)
        {
            _seconds = seconds;
            _button = new MenuButton(this, null, message, MenuType.StandAlone);
            _button.Label.Margin = HEIGHT + 15;
            _button.Label.HorizontalAlign = Align.Left;
            _button.Width = _button.Label.Width + _button.Label.Margin.Left + 15;
            _button.Height = HEIGHT;
            _button.Left = -_button.Width;
            _button.Top = -HEIGHT;
            _button.Enabled = false;
            EmbeddedControls.Add(_button);

            string pic;
            switch (type)
            {
                case NotificationType.Success:
                    pic = "success";
                    break;
                case NotificationType.Information:
                    pic = "info";
                    break;
                case NotificationType.Alert:
                    pic = "quit"; 
                    break;
                default:
                    pic = "info";
                    break;
            }

            Image img = _button.AddImage(pic);
            img.Margin = 15;
            img.HorizontalAlign = Align.Left;
            img.VerticalAlign = Align.Center;
            img.ForeColor = _button.ForeColor[State.Disabled];
        }
        #endregion

        #region Properties
        #endregion

        #region Methods
        public override void Update(GameTime gameTime)
        {
            _seconds -= gameTime.ElapsedGameTime.TotalSeconds;
            if (_seconds <= 0)
            {
                ((NotificationPanel)ParentControl).Pop(this);
            }

            base.Update(gameTime);
        }
        #endregion
    }
}
