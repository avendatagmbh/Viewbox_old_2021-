using ViewValidatorLogic.Structures.Results;

namespace ViewValidator.Models.Result {
    public class ResultOverviewModel {
        #region Constructor
        public ResultOverviewModel(ValidationResults results) {
            this.Results = results;

            _testedViews = 0;
            foreach (var result in results.TableValidationResults.Values) {
                if (result == null || !result.TableMapping.Used) continue;

                _testedViews++;
                if (ViewIsCorrect(result))
                    _correctViews++;
            }

            
        }

        #endregion

        #region Properties
        public ValidationResults Results { get; private set; }

        #region TestedViews
        private int _testedViews;

        public int TestedViews {
            get { return _testedViews; }
            set { _testedViews = value; }
        }
        #endregion

        #region CorrectViews
        private int _correctViews;

        public int CorrectViews {
            get { return _correctViews; }
            set { _correctViews = value; }
        }
        #endregion

        #endregion

        #region Methods
        private bool ViewIsCorrect(TableValidationResult result) {
            return result.RowDifferenceCount == 0 && result.ResultValidationTable.MissingRowsCount == 0 && result.ResultViewTable.MissingRowsCount == 0;
        }
        #endregion
    }
}
