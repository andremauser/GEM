using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace GEM.Menu
{
    internal class NotificationPanel : BaseControl
    {
        #region Fields
        List<Notification> _notifications = new List<Notification>();
        List<Notification> _delete = new List<Notification>();
        #endregion

        #region Constructors
        public NotificationPanel(BaseControl parentControl) : base(parentControl)
        {

        }
        #endregion

        #region Properties
        #endregion

        #region Methods
        public override void Update(GameTime gameTime)
        {
            int top = 0;
            foreach (Notification notification in _notifications)
            {
                notification.Update(gameTime);
                notification.Top = top;
                top -= Notification.HEIGHT;
            }
            foreach (Notification delete in _delete)
            {
                _notifications.Remove(delete);
            }
            _delete.Clear();
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            foreach (Notification notification in _notifications)
            {
                notification.Draw(spriteBatch);
            }
        }
        public Notification Push(string message, Style style, NotificationType type = NotificationType.Information, double seconds = 3.0d, string id = "")
        {
            Notification returnValue = null;
            bool alreadyThere = false;
            foreach (Notification n in _notifications)
            {
                if (id == n.ID && id != "")
                {
                    alreadyThere = true;
                    returnValue = n;
                }
            }
            if (!alreadyThere)
            {
                Notification notification = new Notification(this, message, style, type, seconds, id);
                _notifications.Add(notification);
                returnValue = notification;
            }
            return returnValue;
        }
        public void Pop(Notification notification) 
        {
            _delete.Add(notification);
        }
        public void CloseID(string id)
        {
            foreach (Notification notification in _notifications)
            {
                if (notification.ID == id)
                {
                    notification.TimerEnabled = true;
                    notification.Timer = Notification.MOVETIME;
                }
            }
        }
        public bool ExistsID(string id)
        {
            bool exists = false;
            foreach (Notification notification in _notifications)
            {
                if (notification.ID == id)
                {
                    exists = true;
                }
            }
            return exists;
        }

        #endregion
    }
}
