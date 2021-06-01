using System;
using System.Collections.Generic;
using System.Linq;
using DbSearchDatabase.Interfaces;
using Utils;

namespace DbSearchLogic.Structures.TableRelated {
    public class Row {
        #region Row
        public Row(IDbRow dbRow, Query query) {
            DbRow = dbRow;
            Query = query;
            int columnCount = dbRow.RowEntries.Count();
            RowEntries = new List<RowEntry>(columnCount);
            for (int i = 0; i < columnCount; ++i) {
                RowEntry rowEntry = new RowEntry(dbRow.RowEntries[i], Query.Columns[i].RuleSet);
                RowEntries.Add(rowEntry);
                rowEntry.PropertyChanged += Row_PropertyChanged;
            }
        }

        ~Row() {
            foreach(var rowEntry in RowEntries)
                rowEntry.PropertyChanged -= Row_PropertyChanged;
        }
        #endregion

        #region Properties
        public IDbRow DbRow { get; private set; }
        public Query Query { get; set; }
        public List<RowEntry> RowEntries { get; set; }

        #endregion

        #region Events
        public EventHandler<ColumnValueChangedEventArgs> ColumnValueChanged;
        private void OnColumnValueChanged(int column){if(ColumnValueChanged != null) ColumnValueChanged(this, new ColumnValueChangedEventArgs(column));}

        public class ColumnValueChangedEventArgs : EventArgs {
            public ColumnValueChangedEventArgs(int column) {
                Column = column;
            }

            public int Column { get; set; }
        }

        #endregion

        #region EventHandler
        //Fire column value changed event whenever a rowentry's display string changed
        void Row_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if(e.PropertyName == "DisplayString" || e.PropertyName == "RuleDisplayString") {
                for (int i = 0; i < RowEntries.Count(); ++i) {
                    if (RowEntries[i] == sender) {
                        OnColumnValueChanged(i);
                        break;
                    }
                }
            }
        }
        #endregion EventHandler

        public void AddColumns(int count) {
            int indexStart = RowEntries.Count;
            
            for (int i = 0; i < count; ++i) {
                RowEntry rowEntry = new RowEntry(DbRow.RowEntries[indexStart + i], Query.Columns[indexStart + i].RuleSet);
                RowEntries.Add(rowEntry);
                rowEntry.PropertyChanged += Row_PropertyChanged;
            }
        }

        public void DeleteColumn(int index) {
            RowEntries.RemoveAt(index);
        }
    }
}