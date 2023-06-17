using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

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
            foreach (Notification notification in _notifications)
            {
                notification.Draw(spriteBatch);
            }
        }
        public void Push(string message, NotificationType type = NotificationType.Information, double seconds = 3.0d)
        {
            Notification notification = new Notification(this, message, type, seconds);
            _notifications.Add(notification);
        }
        public void Pop(Notification notification) 
        {
            _delete.Add(notification);
        }

        #endregion
    }
}
