using Microsoft.Xna.Framework;
using System;
using System.Threading;

namespace GEM.Menu
{
    public enum NotificationType
    {
        Success,
        Information,
        Alert,
        Action
    }

    internal class Notification : BaseControl
    {
        #region Fields
        public double Timer;
        MenuButton _button;
        public static int HEIGHT = 60;
        public static float MOVETIME = 0.25f;
        #endregion

        #region Constructors
        public Notification(BaseControl parentControl, string message, Style style, NotificationType type, double seconds, string id) : base(parentControl)
        {
            Timer = seconds;
            ID = id;
            TimerEnabled = true;
            _button = new MenuButton(this, null, message, style, MenuType.StandAlone);
            _button.Label.HorizontalAlign = Align.Right;
            _button.Label.Margin = 60;
            int roundValue = 20;
            int roundWidth = _button.Label.Width + _button.Label.Margin.Left + 20;
            roundWidth -= roundWidth % roundValue;
            roundWidth += roundValue;
            _button.Width = roundWidth;
            _button.Height = HEIGHT;
            _button.OffsetX = -_button.Width;
            _button.OffsetY = -HEIGHT;
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
                case NotificationType.Action:
                    pic = "action";
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

            Label counter = _button.AddLabel("");
            counter.HorizontalAlign = Align.Left;
            counter.VerticalAlign = Align.Top;
            counter.OffsetX = 5;
            counter.OnDraw += (o, e) => {
                ((Label)o).Caption = string.Format("{0}", Math.Ceiling(Timer).ToString());
                ((Label)o).ForeColor = Color.DarkGray;
            };
            counter.Visible = false; // Hide Countdown

            OffsetX = _button.Width;
            MoveTo(0, 0, MOVETIME);
        }
        #endregion

        #region Properties
        public string ID { get; set; }
        public bool TimerEnabled { get; set; }
        #endregion

        #region Methods
        public override void Update(GameTime gameTime)
        {
            if (TimerEnabled)
            {
                Timer -= gameTime.ElapsedGameTime.TotalSeconds;
            }
            if (Timer < MOVETIME && !_isMoving)
            {
                MoveTo(_button.Width, 0, MOVETIME);
            }
            if (Timer <= 0)
            {
                ((NotificationPanel)ParentControl).Pop(this);
            }

            base.Update(gameTime);
        }
        #endregion
    }
}
