using System.ComponentModel;
using AvdCommon.DataGridHelper;
using AvdCommon.DataGridHelper.Interfaces;
using ScreenshotAnalyzer.Controls.Results;

namespace ScreenshotAnalyzer.Models.Results {
    public class ColumnHeaderModel : INotifyPropertyChanged {
        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
        #endregion

        public ColumnHeaderModel(IDataColumn column) {
            Column = column;
        }

        public IDataColumn Column { get; set; }

        private bool _isInEditMode;
        public bool IsInEditMode {
            get { return _isInEditMode; }
            set {
                if (_isInEditMode != value) {
                    _isInEditMode = value;
                    OnPropertyChanged("IsInEditMode");
                }
            }
        }


        public void EditHeader(bool value) {
            IsInEditMode = value;
        }
    }
}
