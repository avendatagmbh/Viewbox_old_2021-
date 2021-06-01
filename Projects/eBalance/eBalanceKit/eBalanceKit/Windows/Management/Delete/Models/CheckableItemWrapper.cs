// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-06-22
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using Utils;

namespace eBalanceKit.Windows.Management.Delete.Models {
    public class CheckableItemWrapper<T> : NotifyPropertyChangedBase {
        public CheckableItemWrapper(T item) { Item = item; }
        public T Item { get; set; }

        #region IsChecked
        private bool _isChecked;

        public bool IsChecked {
            get { return _isChecked; }
            set {
                if (_isChecked == value) return;
                _isChecked = value;
                OnPropertyChanged("IsChecked");
            }
        }
        #endregion // IsChecked
    }
}