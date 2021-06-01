using System.ComponentModel;
using ViewValidatorLogic.Structures.Results;

namespace ViewValidator.Models.Result {
    public class ResultTableDetailsModel : INotifyPropertyChanged {
        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string property) {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
        #endregion events


        public ResultTableDetailsModel(TableValidationResult result, int id) {
            this.Result = result;
        }

        #region Properties

        #region Result
        private TableValidationResult _result;

        public TableValidationResult Result {
            get { return _result; }
            set {
                if (_result != value) {
                    _result = value;
                    if (_result != null) {
                        _correctColumns = 0;
                        foreach (var rowDifferencesPerColumn in _result.RowDifferencePerColumn)
                            if (rowDifferencesPerColumn == 0) _correctColumns++;
                    }
                    OnPropertyChanged("");
                }
            }
        }
        #endregion

        public int CorrectRows { get { return Result == null ? 0 : Result.RowsEqual; } }
        public int MissingRowsView { get { return Result == null ? 0 : Result.ResultViewTable.MissingRowsCount; } }
        public int MissingRowsValidation { get { return Result == null ? 0 : Result.ResultValidationTable.MissingRowsCount; } }
        public int DifferentRows { get { return Result == null ? 0 : Result.RowDifferenceCount; } }

        private bool _HasValidationFinished;
        public bool HasValidationFinished
        {
            get
            {
                return _HasValidationFinished;
            }
            set
            {
                if (_HasValidationFinished != value)
                {
                    _HasValidationFinished = value;
                    OnPropertyChanged("HasValidationFinished");
                }
            }
        }


        #region CorrectColumns
        private int _correctColumns;

        public int CorrectColumns {
            get { return _correctColumns; }
            set { _correctColumns = value; }
        }
        #endregion
        #endregion
    }
}
