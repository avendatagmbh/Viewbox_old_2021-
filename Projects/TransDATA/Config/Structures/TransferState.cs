using Utils;

namespace Config.Structures {
    public class TransferState : NotifyPropertyChangedBase{
        #region Constructor
        public TransferState() {
        }
        #endregion Constructor

        #region Properties

        #region TransferStates
        private TransferStates _state;
        public TransferStates State {
            get { return _state; }
            set {
                if (_state != value) {
                    _state = value;
                    OnPropertyChanged("State");
                }
            }
        }
        #endregion TransferStates

        #region Message
        private string _message;

        public string Message {
            get { return _message; }
            set {
                if (_message != value) {
                    _message = value;
                    OnPropertyChanged("Message");
                }
            }
        }
        #endregion Message
        #endregion Properties

        #region Methods
        #endregion Methods
    }
}
