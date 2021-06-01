// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-09-09
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Xml;
using DbAccess;
using DbAccess.Structures;
using DbSearchDatabase.Config;
using DbSearchDatabase.Interfaces;
using DbSearchDatabase.Results;
using DbSearchDatabase.Structures;
using AV.Log;
using log4net;

namespace DbSearchDatabase.TableRelated {
    [DbTable("queries", ForceInnoDb = true)]
    public class DbQuery : DatabaseObjectBase<int>, IDbQuery {

        internal static ILog _log = LogHelper.GetLogger();

        public DbQuery() {
            Init(null, string.Empty);
        }

        public DbQuery(IDbProfile dbProfile, string name) {
            Init(dbProfile, name);
        }

        #region Properties
        [DbColumn("profile_id", IsInverseMapping = true)]
        public DbProfile DbProfile { get; set; }

        [DbColumn("name", Length = 256)]
        public string Name { get; private set; }

        [DbColumn("count")]
        public long Count { get; set; }

        //Stores for example the user mappings
        [DbColumn("additional_infos", Length = 65536)]
        public string AdditionalInfos { get; set; }

        [DbColumn("created_by")]
        public int CreatedBy { get; set; }

        public List<IDbColumn> Columns { get; set; }
        public List<IDbRow> Rows { get; set; }

        public string TableName { get { return "query_" + Id; } }

        public DbConfigSearchParams DbConfigSearchParams { get; private set; }
        List<DbColumn> _columnsToDelete = new List<DbColumn>();
        #endregion

        #region Methods
        public void Init(IDbProfile dbProfile, string name) {
            DbProfile = (DbProfile)dbProfile;
            Name = name;
            Rows = new List<IDbRow>();
            Columns = new List<IDbColumn>();
            DbConfigSearchParams = new DbConfigSearchParams();
        }

        private string GetColumns(IDatabase conn) {
            if (Columns.Count == 0)
                return "";
            StringBuilder result = new StringBuilder();
            foreach (var columnInfo in Columns) {
                result.Append(conn.Enquote(columnInfo.OriginalColumnName)).Append(",");
            }
            //Delete last comma
            result = result.Remove(result.Length - 1, 1);
            return result.ToString();
        }

        #region Load
        public bool Load() {
            using (IDatabase conn = DbProfile.GetOpenConnection()) {
                List<DbColumn> columnsList = conn.DbMapping.Load<DbColumn>("query_id=" + Id);
                if (columnsList == null || columnsList.Count == 0) return false;
                Columns.Clear();
                Rows.Clear();

                List<DbColumnInfo> columnInfos = conn.GetColumnInfos(TableName);
                if (columnInfos == null) {
                    _log.Log(LogLevelEnum.Error, "Die Tabelle " + TableName + " wurde nicht gefunden, Abfrage: " + Name, true);
                    columnInfos = new List<DbColumnInfo>();
                }

                foreach (var column in columnsList) {
                    column.DbQuery = this;

                    //Find columninformation
                    foreach (var columnInfo in columnInfos) {
                        if (columnInfo.Name == column.Name) {
                            column.DbColumnInfo = columnInfo;
                            break;
                        }
                    }

                    Columns.Add(column);
                }
                DbColumn.LoadRowEntries(columnsList, Rows, this);
            }
            return true;
        }


        #endregion Load

        #region LoadData
        public void LoadData(int limit, List<int> rowNumbers) {
            //if (_loaded) return;
            using (IDatabase conn = DbProfile.GetOpenConnection()) {
                
                List<DbColumnInfo> columnInfos = conn.GetColumnInfos(TableName);
                if (columnInfos == null) {
                    _log.Log(LogLevelEnum.Error, "Die Tabelle " + TableName + " wurde nicht gefunden, Abfrage: " + Name, true);
                    return;
                }
                bool hasRowNumbers = false;
                foreach (var columnInfo in columnInfos) {
                    if (columnInfo.Name == "_row_no_") {
                        hasRowNumbers = true;
                        continue;
                    }
                    Columns.Add(new DbColumn(columnInfo, this));
                }


                if (Columns.Count != 0) {
                    string sql;
                    string fetchRowNoSql = hasRowNumbers ? ",_row_no_" : string.Empty;
                    if (rowNumbers != null && rowNumbers.Count > 0) {
                        sql = "SELECT " + GetColumns(conn) + fetchRowNoSql + " FROM " + conn.Enquote(TableName) + " WHERE _row_no_ in (" + string.Join(",", rowNumbers) + ")";
                    } else {
                        sql = "SELECT " + GetColumns(conn) + fetchRowNoSql + " FROM " + conn.Enquote(TableName) + " WHERE _row_no_<=" + limit;
                    }
                    using (IDataReader reader = conn.ExecuteReader(sql)) {
                        Rows.Clear();
                        while (reader.Read()) {
                            DbRow row = new DbRow(this, hasRowNumbers ? reader.GetInt64(Columns.Count) : 0);
                            for (int i = 0; i < Columns.Count; ++i) {
                                row.RowEntries[i].Value = reader[i];
                            }

                            Rows.Add(row);
                        }

                    }
                }
            }
            //_loaded = true;
        }


        #endregion LoadData

        public void RemoveRow(IDbRow dbRow) {
            Rows.Remove(dbRow);
        }

        public void AddRow(IDbRow dbRow) {
            Rows.Add(dbRow);
        }

        public void DeleteColumns() {
            using (IDatabase conn = DbProfile.GetOpenConnection()) {
                foreach (var column in Columns)
                    conn.DbMapping.Delete(column);
            }
            Columns.Clear();
        }


        #region EnquotedColumns
        private string EnquotedColumns(IEnumerable<DbColumnInfo> columns, IDatabase conn ) {
            StringBuilder columnsString = new StringBuilder();
            foreach (var column in columns)
                columnsString.Append(conn.Enquote(column.Name)).Append(",");
            columnsString.Remove(columnsString.Length - 1, 1);
            return columnsString.ToString();
        }
        #endregion EnquotedColumns

        #region CopyData
        public void CopyData(IDatabase queryConn, IDatabase profileConn) {
            List<DbColumnInfo> columns = queryConn.GetColumnInfos(Name);
            _log.Log(LogLevelEnum.Info, "Importiere Tabelle \"" + Name + "\"", true);

            //Create table sql statement
            StringBuilder sql = new StringBuilder("CREATE TABLE ");
            sql.Append(profileConn.Enquote(TableName)).Append(" (");
            //Add row number as additional column
            string rowColumnName = "_row_no_";
            bool hasRowNumber = false;
            foreach (var column in columns)
                if (column.Name == rowColumnName)
                    hasRowNumber = true;

            if (!hasRowNumber) {
                sql.Append(rowColumnName).Append(" BIGINT UNSIGNED,");
            }

            foreach (var column in columns) {
                sql.Append(profileConn.DbMapping.GetColumnDefinitionFromColumnInfo(column)).Append(",");
            }
            //Remove last comma
            sql.Remove(sql.Length - 1, 1);
            sql.Append(");");

            string sqlString = sql.ToString();
            profileConn.DropTableIfExists(TableName);
            profileConn.ExecuteNonQuery(sqlString);

            //Copy data
            using (IDataReader reader = queryConn.ExecuteReader("SELECT " + EnquotedColumns(columns, queryConn) + " FROM " + queryConn.Enquote(Name))) {
                List<DbColumnValues> rows = new List<DbColumnValues>();
                long rowNo = 1;
                while (reader.Read()) {
                    DbColumnValues currentRow = new DbColumnValues();
                    if(!hasRowNumber) currentRow[rowColumnName] = rowNo;

                    foreach (var column in columns)
                        currentRow[column.Name] = reader[column.Name];

                    rows.Add(currentRow);

                    if (rows.Count > 5000) {
                        profileConn.InsertInto(TableName, rows);
                        rows.Clear();
                    }
                    rowNo++;
                }
                if(rows.Count > 0)
                    profileConn.InsertInto(TableName, rows);

                //Update count
                Count = rowNo - 1;
                profileConn.DbMapping.Save(this);
            }
            //Create index for rowNo column
            profileConn.CreateIndex(TableName, TableName + rowColumnName, new List<string>() {rowColumnName});
            _log.Log(LogLevelEnum.Info, "Tabelle \"" + Name + "\" importiert", true);
        }
        #endregion CopyData

        #region Delete
        public void Delete(IDatabase profileConn, IDatabase distinctConn) {
            profileConn.DropTableIfExists(TableName);
            profileConn.DbMapping.Delete(this);
            profileConn.ExecuteNonQuery("DELETE FROM " + profileConn.Enquote("query_columns") + " WHERE query_id=" + Id);
            //Drop all results for this query
            profileConn.ExecuteNonQuery("DELETE FROM " + profileConn.Enquote("results_search_values") + " WHERE query_id=" + Id);
            profileConn.ExecuteNonQuery("DELETE FROM " + profileConn.Enquote("results_column_hit_infos") + " WHERE query_id=" + Id);
            profileConn.ExecuteNonQuery("DELETE FROM " + profileConn.Enquote("results") + " WHERE query_id=" + Id);

            //Delete from distinct db
            int queryId;
            object result = distinctConn.ExecuteScalar("SELECT tid FROM tables WHERE name=" + profileConn.GetSqlString(TableName));
            if(result == null || !Int32.TryParse(result.ToString(), out queryId)) {
                _log.Log(LogLevelEnum.Warn, "Konnte die Destinct Tabelle des Queries " + Name + " nicht finden.", true);
                return;
            }

            //Find column ids
            List<string> columnIds = distinctConn.GetColumnStringValues("SELECT cid FROM columns WHERE tid=" + queryId);

            //Drop tables
            foreach (var columnId in columnIds) {
                distinctConn.DropTableIfExists("idx_" + columnId);
            }
            //Drop from columns
            distinctConn.ExecuteNonQuery("DELETE FROM columns WHERE tid=" + queryId);
            //Drop from tables
            distinctConn.ExecuteNonQuery("DELETE FROM tables WHERE tid=" + queryId);

            

        }
        #endregion Delete

        #region Save
        public void Save() {
            using (IDatabase conn = DbProfile.GetOpenConnection()) {
                int index = 0;
                foreach (var column in Columns) {
                    ((DbColumn) column).Save(conn, index++);
                }
                //Delete columns which are marked for removal
                foreach(var columnToDelete in _columnsToDelete)
                    conn.DbMapping.Delete(columnToDelete);
                _columnsToDelete.Clear();

                conn.DbMapping.Save(this);
            }
        }
        #endregion Save

        #region Xml
        public void ToXml(XmlWriter xmlWriter) {
            xmlWriter.WriteStartElement("DbQuery");
            xmlWriter.WriteAttributeString("Id", Id.ToString());
            xmlWriter.WriteAttributeString("Name", Name);
            xmlWriter.WriteAttributeString("Count", Count.ToString());

            DbConfigSearchParams.ToXml(xmlWriter);
            //The columns also save the rows implicitly
            xmlWriter.WriteStartElement("Columns");
            xmlWriter.WriteAttributeString("Count", Columns.Count.ToString());
            for (int i = 0; i < Columns.Count; ++i)
                ((DbColumn)Columns[i]).ToXml(xmlWriter, i);
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("AddInfos");
            xmlWriter.WriteAttributeString("Content", AdditionalInfos);
            xmlWriter.WriteEndElement();

            xmlWriter.WriteEndElement();
        }

        public static DbQuery FromXml(XmlReader reader, IDbProfile profile) {
            if (reader.Read() && reader.Name == "DbQuery") {
                DbQuery result = new DbQuery(profile, reader.GetAttribute("Name"));
                result.Id = Convert.ToInt32(reader.GetAttribute("Id"));
                result.Count = Convert.ToInt64(reader.GetAttribute("Count"));
                int currentCol = 0;
                if(reader.Read()) result.DbConfigSearchParams.FromXml(reader);
                if (reader.Name == "Columns" || (reader.Read() && reader.Name == "Columns")) {
                    int columnCount = Convert.ToInt32(reader.GetAttribute("Count"));
                    while (reader.Read() && !(reader.NodeType == XmlNodeType.EndElement && reader.Name == "Columns")) {
                        if (reader.Name == "Column" && reader.NodeType == XmlNodeType.Element) {
                            result.Columns.Add(DbColumn.FromXml(reader, result.Rows, currentCol, result, columnCount));
                            ++currentCol;
                        }
                    }
                }
                if (reader.Read() && reader.Name == "AddInfos")
                    result.AdditionalInfos = reader.GetAttribute("Content");
                return result;
            }
            return null;
        }
        #endregion Xml

        public void DeleteResult(IDbResultSet result) {
            try {
                using (IDatabase conn = DbProfile.GetOpenConnection()) {
                    conn.ExecuteNonQuery("DELETE FROM " + conn.Enquote("results_column_hit_infos") + " WHERE result_id=" +
                                         result.Id);
                    conn.ExecuteNonQuery("DELETE FROM " + conn.Enquote("results_search_values") + " WHERE result_id=" +
                                         result.Id);
                    conn.DbMapping.Delete(result);
                }
            } catch (Exception ex) {
                _log.Log(LogLevelEnum.Error, "Konnte das Ergebnis vom " + result.Timestamp.ToString() + " nicht löschen, Fehler: " + ex.Message, true);
            }
        }

        public void AddColumn(IDbColumn dbColumn) {
            Columns.Add(dbColumn);
            foreach(DbRow row in Rows)
                row.AddColumn(dbColumn);
        }

        public void DeleteColumn(int index) {
            //The column needs to be stored so that on a save request, the column can be deleted from the database
            _columnsToDelete.Add((DbColumn) Columns[index]);
            Columns.RemoveAt(index);
            foreach (DbRow row in Rows)
                row.DeleteColumn(index);
        }

        [DbTable("query_")]
        class EmptyTable {
            [DbColumn("_row_no_"),DbPrimaryKey]
            public long RowId { get; set; }
        }

        public static DbQuery CreateEmptyDbQuery(IDbProfile dbProfile, string name) {
            DbQuery result = new DbQuery(dbProfile, name);
            result.Save();
            using (IDatabase conn = ((DbProfile)dbProfile).GetOpenConnection()) {
                conn.DbMapping.SetTableName<EmptyTable>("query_" + result.Id);
                conn.DbMapping.CreateTableIfNotExists<EmptyTable>();
            }
            return result;
        }
        #endregion Methods

    }
}
