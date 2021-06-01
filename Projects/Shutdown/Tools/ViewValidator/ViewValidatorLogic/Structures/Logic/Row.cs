// -----------------------------------------------------------
// Created by Benjamin Held - 15.08.2011 11:40:57
// Copyright AvenDATA 2011
// -----------------------------------------------------------

using System.Collections.Generic;
using System.Data.Common;
using ViewValidatorLogic.Interfaces;
using ViewValidatorLogic.Structures.InitialSetup;
using System.Collections.ObjectModel;
using ViewValidatorLogic.Structures.Reader;

namespace ViewValidatorLogic.Structures.Logic {
    public class Row : IRow{
        #region Properties
        RowEntry[] Entries { get; set; }
        private Table Table { get; set; }
        List<ColumnInfoHelper> ColumnInfos { get; set; }
        public int ColumnCount { get { return ColumnInfos.Count; } }
        public List<int> KeyEntries { get; private set; }
        private ObservableCollection<ColumnMapping> ColumnMappings { get; set; }
        private int Which { get; set; }
        public IRowEntry this[int index] {
            get { return Entries[index]; }
        }
        #endregion

        #region Constructor
        public Row(Table table, List<ColumnInfoHelper> columnInfos, List<int> keyEntries, ObservableCollection<ColumnMapping> columnMappings, int which) {
            this.Which = which;
            this.Table = table;
            this.ColumnInfos = columnInfos;
            this.KeyEntries = keyEntries;
            this.ColumnMappings = columnMappings;

            Entries = new RowEntry[ColumnCount];
            for (int i = 0; i < ColumnCount; ++i) {
                Entries[i] = new RowEntry(columnMappings[i].GetColumn(which).Rules) {
                    RowEntryType = KeyEntries.Contains(i) ? RowEntryType.KeyEntry : RowEntryType.Normal
                    };
            }
            //Entries[i] = new RowEntry(Rules[i].GetRules(i));
        }
        #endregion

        #region Methods
        public Row Clone() {
            Row result = new Row(Table, ColumnInfos, KeyEntries, ColumnMappings, Which);
            for (int i = 0; i < ColumnCount; ++i)
                result.Entries[i] = Entries[i].Clone();
            return result;
        }

        public void ReadRow(IDbReader reader) {
            for (int i = 0; i < ColumnCount; ++i) {
                if (reader.IsDBNull(i)) {
                    Entries[i].Value = null;
                } else {
                    Entries[i].Value = reader[i];
                }
                Entries[i].ColumnType = ColumnInfos[i].Type;
            }
        }

        internal int CompareKeyEntries(Row row) {
            for (int i = 0; i < KeyEntries.Count; ++i) {
                int result = Entries[KeyEntries[i]].CompareTo(row.Entries[row.KeyEntries[i]]);
                if (result != 0) return result;
            }
            return 0;
        }

        //Returns true if both rows are equal, otherwise returns false
        internal bool IsEqualTo(Row row) {
            for (int i = 0; i < ColumnCount; ++i)
                if (!Entries[i].IsEqualTo(row.Entries[i])) {
                    return false;
                }
            return true;
        }
        internal void GetDifferentCols(Row row, List<int> differentCols) {
            for (int i = 0; i < ColumnCount; ++i)
                if (!Entries[i].IsEqualTo(row.Entries[i]))
                    differentCols.Add(i);

        }

        public override string ToString() {
            string result = "";
            foreach (var entry in Entries)
                result += entry + ", ";
            return result;
        }

        public string HeaderOfColumn(int index) {
            return ColumnInfos[index].Name;
        }
        #endregion

    }
}
