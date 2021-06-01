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
    public class NotificationHandling
    {
        #region Properties
        /// <summary>
        /// Counter for the callings of DisableNotificationEvents. If more than one times called the DisableNotificationEvents, it only gets enabled at the last time
        /// </summary>
        private static readonly Dictionary<NotificationType, int> DisabledNotificationCount = new Dictionary<NotificationType, int>()
                {
                    {NotificationType.PropertyChanged, 0},
                    {NotificationType.PropertyChanging, 0}
                };

        /// <summary>
        /// List for storing the disabled and not fired events
        /// </summary>
        private static readonly List<NotificationElement> DisabledEvents = new List<NotificationElement>();

        #endregion Properties

        #region Methods

        #region DisableNotificationEvents
        /// <summary>
        /// It disables the notificationTypes.
        /// </summary>
        /// <param name="notificationTypes">You can choose the NotificationTypes to disable</param>
        public static void DisableNotificationEvents(NotificationType[] notificationTypes = null)
        {
            if (notificationTypes == null)
                notificationTypes = new[]
                    {
                        NotificationType.PropertyChanging,
                        NotificationType.PropertyChanged
                    };

            foreach (var notificationType in notificationTypes)
            {
                DisabledNotificationCount[notificationType]++;
            }
        }
        #endregion DisableNotificationEvents

        #region EnableNotificationEvents
        /// <summary>
        /// It enables the notificationTypes, and fires the disabled events.
        /// </summary>
        /// <param name="notificationTypes">You can choose the NotificationTypes to enable</param>
        public static void EnableNotificationEvents(NotificationType[] notificationTypes = null)
        {
            if (notificationTypes == null)
                notificationTypes = new[]
                                        {
                                            NotificationType.PropertyChanging,
                                            NotificationType.PropertyChanged
                                        };

            var needNotification = new List<NotificationType>();
            foreach (var notificationType in notificationTypes)
            {
                if (DisabledNotificationCount[notificationType] > 0)
                    DisabledNotificationCount[notificationType]--;
                if (DisabledNotificationCount[notificationType] != 0) continue;
                if (!needNotification.Contains(notificationType))
                    needNotification.Add(notificationType);
            }
            if (needNotification.Count == 0)
                return;
            var index = 0;
            foreach (var notificationElement in DisabledEvents.Where(w => needNotification.Any(n => n == w.NotificationType)).OrderBy(w => w.Id))
            {
                if (index % 100 == 0)
                {
                    System.Threading.Thread.Sleep(10);
                    index = 0;
                }
                index++;
                notificationElement.NotificationObject.DoNotification(notificationElement.NotificationType,
                                                                      notificationElement.Parameter);
            }
            DisabledEvents.RemoveAll(w => needNotification.Any(n => n == w.NotificationType));
            if (DisabledEvents.Count == 0)
                NotificationElement.GenId = 0;
        }

        #endregion Disable/Enable

        #region IsNotificationEnabled
        /// <summary>
        /// Determine if the NotificationType enabled or not
        /// </summary>
        /// <param name="notificationType"></param>
        /// <returns></returns>
        public static bool IsNotificationEnabled(NotificationType notificationType)
        {
            return DisabledNotificationCount[notificationType] == 0;
        }
        #endregion IsNotificationEnabled

        #region AddDisabledEvent
        /// <summary>
        /// If a NotificationType is disabled, you should call this method to store the disabled events. It filters out the duplicated events.
        /// </summary>
        /// <param name="notificationObject"></param>
        /// <param name="parameter"></param>
        /// <param name="notificationType"></param>
        public static void AddDisabledEvent(INotificationObject notificationObject, object parameter, NotificationType notificationType)
        {
            var newElement = new NotificationElement(notificationObject, parameter, notificationType);
            if (DisabledEvents.Contains(newElement))
                return;
            DisabledEvents.Add(newElement);
        }
        #endregion AddDisabledEvent

        #endregion Methods
    }
}
