using System.Collections.Generic;
using System.Collections.ObjectModel;
using ViewValidator.Structures;
using ViewValidatorLogic.Structures.Results;

namespace ViewValidator.Models.Result {
    class RowsDifferentColumnWiseModel {
        #region Constructor
        public RowsDifferentColumnWiseModel(TableValidationResult result) {
            this.TableValidationResult = result;
            if (result != null) {
                //Initialize ColumnMappings
                


                ColumnMappings = new List<ColumnMappingHelper>(result.RowDifferencePerColumn.Count);
                for (int i = 0; i < result.RowDifferencePerColumn.Count; ++i) {
                    int ShownErrors = 0;
                    //Not all errors can be shown as only a fixed amount of Rowdifferences is saved and it is pure luck that all errors of a column are in this collection
                    foreach (var rowDifference in TableValidationResult.RowDifferences)
                        if (rowDifference.ColumnDifferences.Contains(i))
                            ShownErrors++;

                    ColumnMappings.Add(new ColumnMappingHelper() {
                        Source = result.TableMapping.ColumnMappings[i].Source,
                        Destination = result.TableMapping.ColumnMappings[i].Destination,
                        Errors = result.RowDifferencePerColumn[i],
                        ShownErrors = ShownErrors
                    });
                }
                RowDifferences = new ObservableCollection<RowDifference>();
            }
        }
        #endregion

        #region Properties
        public TableValidationResult TableValidationResult { get; private set; }
        public List<ColumnMappingHelper> ColumnMappings { get; private set; }
        public ObservableCollection<RowDifference> RowDifferences { get; private set; }
        #endregion

        #region Methods
        public void SelectedColumn(int columnIndex) {
            if (TableValidationResult == null) return;
            RowDifferences.Clear();
            foreach (var rowDifference in TableValidationResult.RowDifferences)
                if (rowDifference.ColumnDifferences.Contains(columnIndex))
                    RowDifferences.Add(rowDifference);
        }
        #endregion
    }
}
