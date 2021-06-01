// -----------------------------------------------------------
// Created by Benjamin Held - 15.08.2011 11:41:08
// Copyright AvenDATA 2011
// -----------------------------------------------------------

using System.Collections.Generic;
using ViewValidatorLogic.Interfaces;
using ViewValidatorLogic.Structures.Logic;

namespace ViewValidatorLogic.Structures.Results {
    public class RowDifference : IRow{
        public Row[] Rows { get; set; }
        public Row RowValidation { get; set; }
        public Row RowView { get; set; }
        public List<int> ColumnDifferences { get; set; }

        public RowDifference(Row row1, Row row2) {
            RowValidation = row1;
            RowView = row2;

            Rows = new Row[2] { RowValidation, RowView };
            this.ColumnDifferences = new List<int>();
            row1.GetDifferentCols(row2, ColumnDifferences);

            foreach (var differentCol in ColumnDifferences) {
                RowValidation[differentCol].RowEntryType = RowEntryType.Mismatch;
                RowView[differentCol].RowEntryType = RowEntryType.Mismatch;
            }
        }

        public IRowEntry this[int index] {
            get {
                if (index % 2 == 0)
                    return RowValidation[index/2];
                return RowView[(index - 1)/2];
            }
        }

        public int ColumnCount {
            get { return 2*RowValidation.ColumnCount; }
        }

        public string HeaderOfColumn(int index) {
            if (index % 2 == 0)
                return RowValidation.HeaderOfColumn(index/2);
            return RowView.HeaderOfColumn((index-1)/2);
        }
    }
}
