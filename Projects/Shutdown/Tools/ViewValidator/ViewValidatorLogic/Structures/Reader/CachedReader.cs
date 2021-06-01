using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using DbAccess;
using MySql.Data.MySqlClient;
using ViewValidatorLogic.Structures.InitialSetup;
using ViewValidatorLogic.Structures.Logic;

namespace ViewValidatorLogic.Structures.Reader {
    //Reads complete table
    class CachedReader : IDbReader {
        #region Constructor
        public CachedReader(IDatabase conn, int which, TableMapping tableMapping) {
            List<string> keyEntryColumns = new List<string>();
            foreach (var keyMapping in tableMapping.KeyEntryMappings)
                keyEntryColumns.Add(keyMapping.GetColumnName(which));

            string sql;
            if (which == 0) {
                sql = "SELECT " + tableMapping.GetColumns(conn, which) + " FROM " +
                      conn.Enquote(tableMapping.Tables[which].Name)
                      + " " + tableMapping.Tables[which].Filter.GetFilterSQL();

            } else {
                sql = "SELECT " + tableMapping.GetColumns(conn, which) + " FROM " +
                      conn.Enquote(tableMapping.Tables[which].Name)
                      + " " + tableMapping.Tables[which].Filter.GetFilterSQL();
            }
            conn.SetHighTimeout();
            _reader = conn.ExecuteReader(sql);
        }

        #endregion

        #region Properties
        private IDataReader _reader;
        private bool _doingPrecomputations = true;
        private List<Row> _rows;
        private int _rowIndex = -1;
        #endregion Properties

        #region Methods
        public void Dispose() {
            Close();
        }

        public bool Read() {
            ++_rowIndex;
            return _rowIndex < _rows.Count;
        }

        public object this[int colIndex] {
            get {
                if (_doingPrecomputations) return _reader[colIndex];
                return _rows[_rowIndex][colIndex];
            }
        }

        public bool IsDBNull(int index) {
            if (_doingPrecomputations) return _reader.IsDBNull(index);
            return _rows[_rowIndex][index] == null;
        }

        public void Close() {
            
        }

        public void DoPrecomputations(Row row) {
            //Read all rows
            _rows = new List<Row>();
            _doingPrecomputations = true;
            while (_reader.Read()) {
                Row newRow = (Row) row.Clone();
                newRow.ReadRow(this);
                _rows.Add(newRow);
            }
            _reader.Close();

            //Sort rows
            _rows.Sort((r1, r2) => r1.CompareKeyEntries(r2));

            _doingPrecomputations = false;
        }

        #endregion Methods
    }
}
