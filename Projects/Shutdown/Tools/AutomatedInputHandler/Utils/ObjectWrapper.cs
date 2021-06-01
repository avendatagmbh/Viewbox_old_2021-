// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-07-31
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.ComponentModel;

namespace Utils {

    /// <summary>
    /// Wrapper class - used if a specific object is needed in many controls to provide 
    /// property changed events without the necessity to update each control manually.
    /// </summary>
    /// <typeparam name="T">Type of the wrapped object</typeparam>
    public class ObjectWrapper<T> : NotifyPropertyChangedBase
        where T : class, INotifyPropertyChanged {

        private T _value;

        public T Value {
            get { return _value; }
            set {
                if (_value == value) return;
                OnPropertyChanging("Value");
                if (_value != null) _value.PropertyChanged -= ValuePropertyChanged;
                if (value != null) value.PropertyChanged += ValuePropertyChanged;
                _value = value;
                OnPropertyChanged("Value");
            }
        }

        private void ValuePropertyChanged(object sender, PropertyChangedEventArgs e) { OnPropertyChanged("Value." + e.PropertyName); }
        }
}
