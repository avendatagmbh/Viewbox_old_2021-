using Business.Interfaces;
using DbAccess.Interaction;
using Utils;

namespace Business.Structures {
    internal class TransferTableProgressWrapper : NotifyPropertyChangedBase, ITransferTableProgressWrapper {
        private ITransferTableProgress _progress;

        public ITransferTableProgress Progress {
            get { return _progress; }
            set {
                _progress = value;
                OnPropertyChanged("Progress");
            }
        }
    }
}