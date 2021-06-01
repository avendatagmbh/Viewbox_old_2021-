using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utils
{
    /// <summary>
    /// Interface for handling Notification objects
    /// </summary>
    public interface INotificationObject
    {
        void DoNotification(NotificationType notificationType, object parameter);
    }
}
