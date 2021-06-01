// --------------------------------------------------------------------------------
// author: Lajos Szoke
// since: 2011-12-12
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utils
{
    /// <summary>
    /// Class for storing disabled Notification events
    /// </summary>
    public class NotificationElement
    {
        #region Properties

        public static int GenId;

        public INotificationObject NotificationObject;
        public object Parameter;
        public NotificationType NotificationType;
        public int Id;

        #endregion Properties

        #region Constructor
        public NotificationElement(INotificationObject notificationObject, object parameter, NotificationType notificationType)
        {
            NotificationObject = notificationObject;
            Parameter = parameter;
            NotificationType = notificationType;
            Id = GenId;
            GenId++;
        }
        #endregion Constructor

        #region Equals
        /// <summary>
        /// For filtering out the duplicated events in the NotificationHandling.AddDisabledEvent
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return 
                obj != null 
                && ((NotificationElement)obj).NotificationObject == NotificationObject
                && ((NotificationElement)obj).Parameter == Parameter
                && ((NotificationElement)obj).NotificationType == NotificationType;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion Equals
    }
}
