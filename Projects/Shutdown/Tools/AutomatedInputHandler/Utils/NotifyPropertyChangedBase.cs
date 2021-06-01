using System.ComponentModel;

namespace Utils {
    public class NotifyPropertyChangedBase : INotifyPropertyChanged, INotifyPropertyChanging, INotificationObject {
        private PropertyChangedEventHandler _propertyChanged;

        private PropertyChangingEventHandler _propertyChanging;

        #region INotificationObject Members
        /// <summary>
        /// After calling the EnableNotificationEvents, it calls this method to fire the stored events
        /// </summary>
        /// <param name="notificationType"></param>
        /// <param name="parameter"></param>
        public void DoNotification(NotificationType notificationType, object parameter) {
            switch (notificationType) {
                case NotificationType.PropertyChanged:
                    OnPropertyChanged(parameter == null ? "" : parameter.ToString());
                    break;
                case NotificationType.PropertyChanging:
                    OnPropertyChanging(parameter == null ? "" : parameter.ToString());
                    break;
            }
        }
        #endregion

        #region INotifyPropertyChanged Members
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged { add { _propertyChanged += value; } remove { _propertyChanged -= value; } }
        #endregion

        #region INotifyPropertyChanging Members
        /// <summary>
        /// Occurs when a property value is changing.
        /// </summary>
        public event PropertyChangingEventHandler PropertyChanging { add { _propertyChanging += value; } remove { _propertyChanging -= value; } }
        #endregion

        /// <summary>
        /// Clears all event handler.
        /// </summary>
        public void ClearAllEventHandler() {
            _propertyChanged = null;
            _propertyChanging = null;
        }

        /// <summary>
        /// If the OnPropertyChanged notificationtype is disabled, it only stores the event
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected virtual void OnPropertyChanged(string propertyName) {
            if (_propertyChanged == null) {
                return;
            }
            if (NotificationHandling.IsNotificationEnabled(NotificationType.PropertyChanged)) {
                _propertyChanged(this, new PropertyChangedEventArgs(propertyName));
            } else {
                NotificationHandling.AddDisabledEvent(this, propertyName, NotificationType.PropertyChanged);
            }
        }

        /// <summary>
        /// If the OnPropertyChanging notificationtype is disabled, it only stores the event
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected virtual void OnPropertyChanging(string propertyName) {
            if (_propertyChanging == null) {
                return;
            }
            if (NotificationHandling.IsNotificationEnabled(NotificationType.PropertyChanging)) {
                _propertyChanging(this, new PropertyChangingEventArgs(propertyName));
            } else {
                NotificationHandling.AddDisabledEvent(this, propertyName, NotificationType.PropertyChanging);
            }
        }
    }
}