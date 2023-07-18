using Microsoft.Xna.Framework;

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
        public static int HEIGHT = 60;
        #endregion

        #region Constructors
        public Notification(BaseControl parentControl, string message, Style style, NotificationType type, double seconds) : base(parentControl)
        {
            _seconds = seconds;
            _button = new MenuButton(this, null, message, style, MenuType.StandAlone);
            _button.Label.HorizontalAlign = Align.Right;
            _button.Label.Margin = 60;
            _button.Width = _button.Label.Width + _button.Label.Margin.Left + 20;
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
            img.Height = 40;
            img.Width = 40;
            img.Margin = 10;
            img.HorizontalAlign = Align.Right;
            img.VerticalAlign = Align.Center;
            img.ForeColor = _button.Style.ForeColor(State.Disabled);
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
