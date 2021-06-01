// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-03-21
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using Taxonomy;

namespace eBalanceKit.Windows.Reconciliation {
    public class DragDropData : Utils.NotifyPropertyChangedBase {
        public IElement Item { get; set; }

        #region Message
        private string _message;

        public string Message {
            get { return _message; }
            set {
                if (_message == value) return;
                _message = value;
                OnPropertyChanged("Message");
            }
        }
        #endregion // Message

        #region AllowDrop
        private bool _allowDrop;

        public bool AllowDrop {
            get { return _allowDrop; }
            set {
                if (_allowDrop == value) return;
                _allowDrop = value;
                OnPropertyChanged("AllowDrop");
            }
        }
        #endregion // AllowDrop

        #region ShowAddSign
        private bool _showAddSign;

        public bool ShowAddSign {
            get { return _showAddSign; }
            set {
                if (_showAddSign == value) return;
                _showAddSign = value;
                OnPropertyChanged("ShowAddSign");
            }
        }
        #endregion // ShowAddSign

        #region ShowReplacePositionSign
        private bool _showReplacePositionSign;

        public bool ShowReplacePositionSign {
            get { return _showReplacePositionSign; }
            set {
                if (_showReplacePositionSign == value) return;
                _showReplacePositionSign = value;
                OnPropertyChanged("ShowReplacePositionSign");
            }
        }
        #endregion // ShowReplacePositionSign

    }
}