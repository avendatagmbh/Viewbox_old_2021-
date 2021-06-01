using System.ComponentModel;

namespace Utils
{
    public class NotifyPropertyChangedBase : INotifyPropertyChanged, INotifyPropertyChanging
    {
        private PropertyChangedEventHandler _propertyChanged;

        private PropertyChangingEventHandler _propertyChanging;

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged
        {
            add { _propertyChanged += value; }
            remove { _propertyChanged -= value; }
        }

        #endregion

        #region INotifyPropertyChanging Members

        public event PropertyChangingEventHandler PropertyChanging
        {
            add { _propertyChanging += value; }
            remove { _propertyChanging -= value; }
        }

        #endregion

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (_propertyChanged != null) _propertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanging(string propertyName)
        {
            if (_propertyChanging != null) _propertyChanging(this, new PropertyChangingEventArgs(propertyName));
        }

        public void ClearAllEventHandler()
        {
            _propertyChanged = null;
            _propertyChanging = null;
        }
    }
}