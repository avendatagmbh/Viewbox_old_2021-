using System.ComponentModel;
using ViewValidatorLogic.Structures.InitialSetup;
using ViewValidatorLogic.Structures.Results;

namespace ViewValidator.Models.Result {
    public class ResultsModel :INotifyPropertyChanged {
        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string property) {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
        #endregion events

        #region Constructor
        public ResultsModel(){
//            SelectedTableResult = -1;
            CurrentTableResult = null;
            //AvailableTableResults = new ObservableCollection<string>();
        }
        #endregion

        #region Properties
        public ValidationResults ValidationResults { get; set; }

        #region CurrentTableResult
        private TableValidationResult _currentTableResult;
        public TableValidationResult CurrentTableResult
        {
          get { return _currentTableResult; }
          set {
              if (_currentTableResult != value) {
                  _currentTableResult = value;
                  ResultTableDetailsModel.Result = value;
                  OnPropertyChanged("CurrentTableResult");
                  OnPropertyChanged("ResultsCalculated");
              }
          }
        }
        #endregion

        #region ResultTableDetailsModel
        ResultTableDetailsModel _resultTableDetailsModel = new ResultTableDetailsModel(null, 0);

        public ResultTableDetailsModel ResultTableDetailsModel {
            get { return _resultTableDetailsModel; }
            set { 
                _resultTableDetailsModel = value; 
            }
        }
        #endregion

        public bool ResultsCalculated { 
            get { 
                return CurrentTableResult != null && (
                CurrentTableResult.ResultValidationTable.RowCount != 0 || CurrentTableResult.ResultViewTable.RowCount != 0); 
            } 
        }
        #endregion

        #region Methods
        public void SetTableMappingResult(TableMapping tableMapping) {
            if (ValidationResults == null || tableMapping == null) return;
            CurrentTableResult = ValidationResults[tableMapping];
            //for (int i = 0; i < ValidationResults.Setup.TableMappings.Count; ++i) {
            //    if (ValidationResults.Setup.TableMappings[i] == tableMapping) {
            //        CurrentTableResult = ValidationResults[i];
            //        break;
            //    }
            //}
        }

        #endregion

        internal void NewResults(ViewValidatorLogic.Structures.Results.ValidationResults validationResults) {
            this.ValidationResults = validationResults;
        }
    }
}

