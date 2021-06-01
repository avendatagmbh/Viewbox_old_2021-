// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-09-12
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess.Structures;
using DbSearchDatabase.Interfaces;

namespace DbSearchDatabase.TableRelated {
    internal class DbRow : IDbRow {
        #region Constructor
        public DbRow(DbQuery query, long rowNumber, int columnCount) {
            Init(query, rowNumber, columnCount);
            for (int i = 0; i < columnCount; ++i)
                RowEntries[i] = new DbRowEntry(null, this);
        }

        private void Init(DbQuery query, long rowNumber, int columnCount) {
            Query = query;
            RowNumber = rowNumber;
            RowEntries = new List<IDbRowEntry>(new DbRowEntry[columnCount]);
        }

        public DbRow(DbQuery query, long rowNumber) {
            Init(query, rowNumber, query.Columns.Count);

            for (int i = 0; i < Query.Columns.Count; ++i)
                RowEntries[i] = new DbRowEntry(Query.Columns[i], this);
        }
        #endregion

        #region Properties
        public List<IDbRowEntry> RowEntries { get; set; }
        private DbQuery Query { get; set; }
        //private List<DbColumn> ColumnInfos { get; set; }
        internal long RowNumber { get; set; }
        #endregion

        #region Methods
        public void AddColumn(IDbColumn dbColumn) {
            RowEntries.Add(new DbRowEntry(dbColumn, this));
        }

        public void DeleteColumn(int index) {
            RowEntries.RemoveAt(index);
        }
        #endregion Methods

    }
}
