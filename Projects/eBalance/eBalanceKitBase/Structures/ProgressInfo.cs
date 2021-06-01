using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows;

namespace eBalanceKitBase.Structures {
    /// <summary>
    /// Info object, which is used to contain some status info that could be displayed in progress bars.
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <since>2011-05-24</since>
    public class ProgressInfo : INotifyPropertyChanged {

        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
        #endregion

        #region properties

        #region Caption
        public string Caption {
            get { return _caption; }
            set {
                _caption = value;
                OnPropertyChanged("Caption");
            }
        }
        private string _caption;
        #endregion

        #region Minimum
        public double Minimum {
            get { return _minimum; }
            set {
                _minimum = value;
                OnPropertyChanged("Minimum");
            }
        }
        private double _minimum;
        #endregion

        #region Maximum
        public double Maximum {
            get { return _maximum; }
            set {
                _maximum = value;
                OnPropertyChanged("Maximum");
            }
        }
        private double _maximum = 100;
        #endregion

        #region Value
        public double Value {
            get { return _value; }
            set {
                _value = value;
                OnPropertyChanged("Value");
            }
        }
        private double _value;
        #endregion

        #region IsIndeterminate
        public bool IsIndeterminate {
            get { return _isIndeterminate; }
            set {
                _isIndeterminate = value;
                OnPropertyChanged("IsIndeterminate");
            }
        }
        private bool _isIndeterminate;
        #endregion

        public Window Parent { get; set; }

        #endregion properties

        public void CloseParent() {
            Parent.Dispatcher.Invoke(
                System.Windows.Threading.DispatcherPriority.Background,
                new Action(delegate {
                Parent.Close();
            }));
        }
    }
}
