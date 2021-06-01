// -----------------------------------------------------------
// Created by Benjamin Held - 15.08.2011 11:41:11
// Copyright AvenDATA 2011
// -----------------------------------------------------------

using System.Collections.Generic;
using ViewValidatorLogic.Structures.InitialSetup;
using ViewValidatorLogic.Structures.Logic;

namespace ViewValidatorLogic.Structures.Results {
    public class ResultPerTable {
        public List<Row> MissingRows { get; set; }
        public int MissingRowsCount { get; set; }
        public int RowCount { get; set; }


        public ResultPerTable() {
            this.MissingRows = new List<Row>();
            this.MissingRowsCount = 0;
            this.RowCount = 0;
        }
    }

    public class TableValidationResult {
        //Combines ResultValidationTable and ResultViewTable in one array
        public ResultPerTable[] ResultPerTable { get; private set; }
        public ResultPerTable ResultValidationTable { get; private set; }
        public ResultPerTable ResultViewTable { get; private set; }
        public List<RowDifference> RowDifferences { get; private set; }
        //Saves number of row differences for each column mapping
        public List<int> RowDifferencePerColumn { get; private set; }
        //This is saved due to the fact that RowDifferences are only added up to a limit
        public int RowDifferenceCount { get; private set; }
        public int RowsEqual { get; private set; }
        public TableMapping TableMapping { get; private set; }
        private int ErrorLimit { get; set; }
        
        public TableValidationResult(TableMapping tableMapping, int errorLimit) {
            this.ResultValidationTable = new ResultPerTable();
            this.ResultViewTable = new ResultPerTable();
            this.ResultPerTable = new ResultPerTable[2] {ResultValidationTable, ResultViewTable};
            this.RowDifferencePerColumn = new List<int>(tableMapping.ColumnMappings.Count);
            this.RowDifferencePerColumn.AddRange(new int[tableMapping.ColumnMappings.Count]);
            this.ErrorLimit = errorLimit;
            

            this.RowDifferences = new List<RowDifference>();
            this.TableMapping = tableMapping;
            
        }

        public void AddRowMissing(int where, Row row) {
            if (ErrorLimit == 0 || ResultPerTable[where].MissingRows.Count < ErrorLimit) ResultPerTable[where].MissingRows.Add(row.Clone());
            ResultPerTable[where].MissingRowsCount++;
            RowAdded(where==1, where==0);
        }

        public void AddRowDifference(Row row1, Row row2) {
            RowDifference rowDifference = new RowDifference(row1.Clone(), row2.Clone());

            //Only add until limit is reached
            if (ErrorLimit == 0 || RowDifferences.Count < ErrorLimit) {
                RowDifferences.Add(rowDifference);
            }
            //Increase count of column differences for each column where the rows differ
            foreach (var column in rowDifference.ColumnDifferences)
                RowDifferencePerColumn[column]++;

            RowDifferenceCount++;

            RowAdded(true, true);
        }

        public void AddRowsEqual() {
            this.RowsEqual++;
            RowAdded(true, true);
        }

        private void RowAdded(bool validationTable, bool viewTable){
            if (validationTable) ResultPerTable[0].RowCount++;
            if (viewTable) ResultPerTable[1].RowCount++;
        }
    }
}
