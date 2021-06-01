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
    class RowWiseReader : IDbReader {
        #region Constructor
        public RowWiseReader(IDatabase conn, int which, TableMapping tableMapping) {
            _tableMapping = tableMapping;
            _conn = conn;
            _which = which;
        }

        #endregion

        #region Properties
        private TableMapping _tableMapping;
        private int _which;
        private IDatabase _conn;

        private IDataReader Reader { get; set; }
        #endregion
        #region Methods
        private List<string> ListToEnquotedList(List<string> list, IDatabase conn) {
            List<string> result = new List<string>();
            foreach (var entry in list)
                result.Add(conn.Enquote(entry));
            return result;
        }
        

        public bool Read() {
            return Reader.Read();
        }

        public object this[int colIndex] {
            get { return Reader[colIndex]; }
        }

        public bool IsDBNull(int index) {
            return Reader.IsDBNull(index);
        }

        public void Close() {
            if(Reader != null && !Reader.IsClosed)
                Reader.Close();
        }

        ColumnMapping FindColumnMappingFromKeyEntryMapping(ColumnMapping keyEntryMapping) {
            foreach (var columnMapping in _tableMapping.ColumnMappings)
                if (columnMapping.Source == keyEntryMapping.Source && columnMapping.Destination == keyEntryMapping.Destination)
                    return columnMapping;
            return null;
        }

        public void DoPrecomputations(Row row)
        {
            List<string> keyEntryColumns = new List<string>();
            foreach (var keyMapping in _tableMapping.KeyEntryMappings)
            {
                ColumnMapping columnMapping = FindColumnMappingFromKeyEntryMapping(keyMapping);
                if (columnMapping == null)
                    throw new InvalidOperationException("Konnte keine Spaltenzuordnung finden zwischen " + keyMapping.Source + " und " +
                                        keyMapping.Destination + ", es existiert aber Sortierkriterium mit diesen Spalten, bitte überprüfen Sie das.");

                if (columnMapping.GetColumn(_which).Rules.SortRules.Count > 0)
                {
                    keyEntryColumns.Add(
                        columnMapping.GetColumn(_which).Rules.SortRules[0].GetSqlSortStatement(columnMapping.GetColumnName(_which),
                                                                                           _conn, _which));
                }
                else keyEntryColumns.Add(_conn.Enquote(keyMapping.GetColumnName(_which)));
            }

            string sql;
            string m_filter = _tableMapping.Tables[_which].Filter.GetFilterSQL();
            string m_TempTable = _tableMapping.Tables[_which].Name + ((!string.IsNullOrWhiteSpace(m_filter)) ? "_temp" : string.Empty);
            string m_IfExistCommand;
            string m_DropTableCommand;
            object m_Result;

            if (_which == 0) //Access
            {
                if (!string.IsNullOrWhiteSpace(m_filter))
                {
                    /*//Check is temp table exists.
                    m_IfExistCommand = "SELECT COUNT(*) as table_Found FROM MsysObjects WHERE [Type]=1 AND [Name] = '" + m_TempTable + "'";
                    m_Result = _conn.ExecuteScalar(m_IfExistCommand);

                    //If exists delete it.
                    if ((int)m_Result > 0)
                    {
                        m_DropTableCommand = "DROP TABLE " + m_TempTable + ";";
                        _conn.ExecuteNonQuery(m_DropTableCommand);
                    }*/

                    DeleteTempTables();

                    //Create the new temp table.
                    sql = "SELECT " + _tableMapping.GetColumns(_conn, _which) +
                    " INTO " + m_TempTable +
                    " FROM " + _tableMapping.Tables[_which].Name + " " + m_filter;

                    _conn.ExecuteNonQuery(sql);
                }

                sql = "SELECT " + _tableMapping.GetColumns(_conn, _which) + " FROM " + m_TempTable;

                if (keyEntryColumns != null && keyEntryColumns.Count > 0)
                    sql += " ORDER BY " + string.Join(",", keyEntryColumns);

            }
            else //MySQL
            {
                if (!string.IsNullOrWhiteSpace(m_filter))
                {
                    //If exists delete it.
                    m_DropTableCommand = "DROP TABLE IF EXISTS " + m_TempTable;
                    _conn.ExecuteNonQuery(m_DropTableCommand);

                    sql = "CREATE TABLE " + m_TempTable + " LIKE " + _tableMapping.Tables[_which].Name;
                    _conn.ExecuteNonQuery(sql);

                    sql = "INSERT " + m_TempTable + " SELECT * FROM " + _tableMapping.Tables[_which].Name + " " + m_filter;
                    _conn.ExecuteNonQuery(sql);
                }

                sql = "SELECT " + _tableMapping.GetColumns(_conn, _which) + " FROM " + m_TempTable;

                if (keyEntryColumns != null && keyEntryColumns.Count > 0)
                    sql += " ORDER BY " + string.Join(",", keyEntryColumns);

                MySqlCommand cmd = new MySqlCommand("set net_write_timeout=99999; set net_read_timeout=99999", (MySqlConnection)_conn.Connection); // Setting tiimeout on mysqlServer
                cmd.ExecuteNonQuery();
            }

            Reader = _conn.ExecuteReader(sql);
        }

        /// <summary>
        /// Deletes temp tables created when filtering thw original tables. Closes the Reader as it is required to be able to delete.
        /// </summary>
        public void DeleteTempTables()
        {
            if (_which == 0)
            {
                if (Reader != null && !Reader.IsClosed)
                    Reader.Close();

                try
                {
                    //Because it is difficult to set the security options in Access every time, we do not check
                    //whether the table exists or not.
                    _conn.ExecuteNonQuery("DROP TABLE " + _tableMapping.Tables[_which].Name + "_temp");
                }
                catch
                {
                    //Do nothing, table did not exist.
                }
            }
            else
            {
                if (Reader != null && !Reader.IsClosed)
                    Reader.Close();

                try
                {
                    _conn.ExecuteNonQuery("DROP TABLE IF EXISTS " + _tableMapping.Tables[0].Name + "_temp");
                }
                catch
                {
                    //Do nothing, table did not exist.
                }
            }
        }

        #endregion Methods

        public void Dispose() {
            Close();
        }
    }
}
