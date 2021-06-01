using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace AvdWpfControls.Converters {
    public class EmptyListToVisibilityConverter : IValueConverter {

        public EmptyListToVisibilityConverter() {
            EmptyIsVisible = false;
            _isEmpty = Visibility.Collapsed;
            _isNotEmpty = Visibility.Visible;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {

            bool configuredEmptyIsVisible = EmptyIsVisible;
            Visibility result = Visibility.Collapsed;

            if (parameter != null) {
                EmptyIsVisible = System.Convert.ToBoolean(parameter);
            }

            if (value == null) {
                result = _isEmpty;
            } else if (value is ICollection) {
                ICollection list = value as ICollection;
                result = list.Count == 0 ? _isEmpty : _isNotEmpty;
            } else if (value is IEnumerable<object>) {
                IEnumerable<object> enumerable = value as IEnumerable<object>;
                result = enumerable.Any() ? _isNotEmpty : _isEmpty;
            } else {
                int count;
                if (Int32.TryParse(value.ToString(), out count))
                    result = count == 0 ? _isEmpty : _isNotEmpty;
            }

            // maybe we configured something by passing as parameter so we have to set back the origin value
            EmptyIsVisible = configuredEmptyIsVisible;
            
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {

            throw new NotImplementedException();

        }


        public bool EmptyIsVisible {
            get { return _emptyIsVisible; }
            set {
                if (value == _emptyIsVisible) {
                    return;
                }
                _emptyIsVisible = value;
                if (_emptyIsVisible) {
                    _isEmpty = Visibility.Visible;
                    _isNotEmpty = Visibility.Collapsed;
                } else {
                    _isEmpty = Visibility.Collapsed;
                    _isNotEmpty = Visibility.Visible;
                }
            }
        }

        private bool _emptyIsVisible;

        private Visibility _isEmpty;
        private Visibility _isNotEmpty;
    }
}
