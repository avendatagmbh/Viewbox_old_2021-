using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Text;
using DbAccess;
using DbAccess.Structures;

namespace DbSearchDatabase.DistinctDb {

    public class DistinctIndexTable {

        private const string COLNAME_ID = "id";
        private const string COLNAME_VALUE = "value";
        private const string COLNAME_ROWIDS = "rowIds";
        private const string COLNAME_HAS_ROWNUMBER_TABLE = "hasRowNoTable";

        /// <summary>
        /// Initializes a new instance of the <see cref="DistinctIndexTable"/> class.
        /// </summary>
        /// <param name="db">The db.</param>
        /// <param name="assignedColumn">The assigned column.</param>
        internal DistinctIndexTable(string distinctDbName, DistinctColumnInfo assignedColumn) {
            this.DistinctDbName = distinctDbName;
            this.AssignedColumn = assignedColumn;
        }

        private string DistinctDbName { get; set; }

        /// <summary>
        /// Gets or sets the assigned column.
        /// </summary>
        /// <value>The assigned column.</value>
        private DistinctColumnInfo AssignedColumn { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has a row number table.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has a row number table; otherwise, <c>false</c>.
        /// </value>
        private bool HasRowNumberTable { get; set; }

        /// <summary>
        /// Creates the index table.
        /// </summary>
        private void CreateIndexTable(IDatabase db, string sourceDbName) {
            try {
                string type = string.Empty;

                using (var oReader = db.ExecuteReader("explain " + db.Enquote(sourceDbName, this.AssignedColumn.TableInfo.Name))) {
                    while (oReader.Read()) {
                        if (oReader.GetString(0).ToUpper().Equals(this.AssignedColumn.Name.ToUpper())) {
                            type = oReader.GetString(1);
                            break;
                        }
                    }
                    oReader.Close();
                }

                db.DropTableIfExists(this.AssignedColumn.IndexTableName);

                string sql =
                        "CREATE TABLE IF NOT EXISTS " + db.Enquote(this.AssignedColumn.IndexTableName) + " (" +
                        db.Enquote(COLNAME_ID) + " INT UNSIGNED AUTO_INCREMENT" +
                        "," + db.Enquote(COLNAME_VALUE) + " " + type +
                        "," + db.Enquote(COLNAME_HAS_ROWNUMBER_TABLE) + " TINYINT(1) UNSIGNED NOT NULL" +
                        "," + db.Enquote(COLNAME_ROWIDS) + " LONGBLOB NOT NULL" +
                        ",PRIMARY KEY(" + db.Enquote("id") + ")";

                if (type.ToUpper().Contains("TEXT")) {
                        sql += ",INDEX(" + db.Enquote(COLNAME_VALUE) + "(100))";
                    
                } else if (type.ToUpper().Contains("VARCHAR")) {
                    int len = int.Parse(type.Substring(type.IndexOf('(') + 1, type.Length - type.IndexOf('(') - 2));
                    sql += ",INDEX(" + db.Enquote(COLNAME_VALUE) + (len > 100 ? "(100)" : "") + ")";
                
                } else {
                    sql += ",INDEX(" + db.Enquote(COLNAME_VALUE) + ")";                
                }

                sql += ") engine=MyISAM DEFAULT CHARSET=latin1 COLLATE=latin1_general_cs";

                db.ExecuteNonQuery(sql);

            } catch (Exception ex) {
                throw ex;
            }
        }

        /// <summary>
        /// Creates the row number table.
        /// </summary>
        private void CreateRowNumberTable(IDatabase db) {
            string sql =
                    "CREATE TABLE IF NOT EXISTS " + db.Enquote(this.AssignedColumn.RowNumberTableName) + " (" +
                        db.Enquote("id") + " INT UNSIGNED NOT NULL AUTO_INCREMENT," +
                        db.Enquote(COLNAME_ROWIDS) + " LONGBLOB NOT NULL," +
                    "PRIMARY KEY(" + db.Enquote("id") + ")" +
                    ") engine=MyISAM";

            db.ExecuteNonQuery(sql);

            this.HasRowNumberTable = true;
        }

        public void Create(IDatabase db, string sourceDbName) {
            CreateIndexTable(db, sourceDbName);

            // Drop rownumbers table, if exist. 
            // This table will be recreated, if more than MAX_ROWS_PER_STREAM row_ids must be saved. 
            // Otherwhise the ids will be saved within the index table.
            db.DropTableIfExists(AssignedColumn.RowNumberTableName);

            this.HasRowNumberTable = false;
        }

        /// <summary>
        /// Inserts the row number.
        /// </summary>
        /// <param name="columnId">The column id.</param>
        /// <param name="tableId">The table id.</param>
        /// <param name="memStream">The mem stream.</param>
        /// <returns></returns>
        public UInt32 InsertRowNumber(IDatabase db, MemoryStream memStream) {

            if (!this.HasRowNumberTable) CreateRowNumberTable(db);

            DbColumnValues fieldValues = new DbColumnValues();
            fieldValues[COLNAME_ROWIDS] = memStream.ToArray();

            db.InsertInto(AssignedColumn.RowNumberTableName, fieldValues);

            // reset memstream
            memStream.Position = 0;
            memStream.SetLength(0);

            return (UInt32)(Int64)db.ExecuteScalar(
                "SELECT LAST_INSERT_ID() " +
                "FROM " + db.Enquote(AssignedColumn.RowNumberTableName.ToString()));
        }

        public void InsertValue(IDatabase db, MemoryStream memStream, object value, List<UInt32> rowNumberIds) {
            // save row no id´s to db
            DbColumnValues fieldValues = new DbColumnValues();
            if (value.GetType().Name == "String") {
                fieldValues[COLNAME_VALUE] = Encoding.GetEncoding("latin1").GetBytes((string)value);
            } else {
                fieldValues[COLNAME_VALUE] = value;
            }

            if (rowNumberIds.Count == 0) {
                // write row-numbers into index table                
                fieldValues[COLNAME_ROWIDS] = memStream.ToArray();

                // reset memstream and row-counter
                memStream.Position = 0;
                memStream.SetLength(0);

                fieldValues[COLNAME_HAS_ROWNUMBER_TABLE] = 0;
            
            } else {
                // write row-numbers into separate row-number table
                rowNumberIds.Add(InsertRowNumber(db, memStream));

                using (MemoryStream memStreamRowNumberIds = new MemoryStream()) {
                    foreach (UInt32 rowId in rowNumberIds) {
                        memStreamRowNumberIds.Write(BitConverter.GetBytes(rowId), 0, 4);
                    }

                    fieldValues[COLNAME_ROWIDS] = memStreamRowNumberIds.ToArray();
                    memStreamRowNumberIds.Close();

                    fieldValues[COLNAME_HAS_ROWNUMBER_TABLE] = 1;
                }
            }

            db.InsertInto(AssignedColumn.IndexTableName, fieldValues);
        }

        private IDataReader mReader;
        
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public object Value { get; private set; }
        
        /// <summary>
        /// Gets or sets the id value (needed to receive the assigned row numbers).
        /// </summary>
        /// <value>The id value.</value>
        public object IdValue { get; private set; }

        public void OpenReader(IDatabase db) {
            string sql =
                "SELECT " +
                    db.Enquote("Id") + ", " +
                    db.Enquote(COLNAME_VALUE) + " " +
                "FROM " + db.Enquote(this.AssignedColumn.IndexTableName);
            mReader = db.ExecuteReader(sql);
        }

        public bool GetNextValue() {
            if (mReader != null) {
                bool result = mReader.Read();

                this.Value = mReader.GetValue(0);
                this.IdValue = mReader.GetValue(1);
                               
                return result;
            } else {
                return false;
            }
        }

        public void CloseReader() {
            if (mReader != null) {
                mReader.Close();
                mReader = null;
            }
        }

        public List<UInt32> GetRowNumbersById(IDatabase db, int idValue) {

            IDataReader reader = null;
            try {
                string sql =
                        "SELECT " +
                            db.Enquote(COLNAME_HAS_ROWNUMBER_TABLE) + "," +
                            db.Enquote(COLNAME_ROWIDS) + "," +
                            "LENGTH(" + db.Enquote(COLNAME_ROWIDS) + ") " +
                        "FROM " + db.Enquote(DistinctDbName, AssignedColumn.IndexTableName) + " " +
                        "WHERE " + db.Enquote(COLNAME_ID) + " = " + idValue;

                List<UInt32> rowNumbers = new List<uint>();

                reader = db.ExecuteReader(sql);

                //if (!reader.HasRows) return rowNumbers;

                if (!reader.Read())
                    return rowNumbers;

                int length = Convert.ToInt32(reader[2]);
                Byte[] tmp = new Byte[length];
                reader.GetBytes(1, 0, tmp, 0, (int)length);

                int pos = 0;
                while (pos < length) {
                    rowNumbers.Add(BitConverter.ToUInt32(tmp, pos));
                    pos += 4;
                }

                if (reader.GetBoolean(0)) {
                    reader.Close();

                    StringBuilder sb = new StringBuilder();
                    foreach (UInt32 id in rowNumbers) {
                        sb.Append(id).Append(",");
                    }

                    rowNumbers.Clear();

                    reader = db.ExecuteReader(
                        "SELECT " + db.Enquote(COLNAME_ROWIDS) + ",LENGTH(" + db.Enquote(COLNAME_ROWIDS) + ") FROM " +
                        db.Enquote(DistinctDbName, AssignedColumn.RowNumberTableName) +
                        " WHERE id IN (" + sb.Remove(sb.Length - 1, 1).ToString() + ")");

                    while (reader.Read()) {
                        length = Convert.ToInt32(reader[1]);
                        tmp = new Byte[length];
                        reader.GetBytes(0, 0, tmp, 0, (int)length);
                        pos = 0;

                        while (pos < length) {
                            rowNumbers.Add(BitConverter.ToUInt32(tmp, pos));
                            pos += 4;
                        }

                    }
                }

                rowNumbers.Sort();
                return rowNumbers;

            } catch (Exception ex) {
                throw ex;
            } finally {
                if (reader != null) reader.Close();
            }
        }

        public List<UInt32> GetRowNumbersExakt(IDatabase db, string value) {

            IDataReader reader = null;
            try {
                string sql =
                        "SELECT " +
                            db.Enquote(COLNAME_HAS_ROWNUMBER_TABLE) + "," +
                            db.Enquote(COLNAME_ROWIDS) + "," +
                            "LENGTH(" + db.Enquote(COLNAME_ROWIDS) + ") " +
                        "FROM " + db.Enquote(DistinctDbName, AssignedColumn.IndexTableName) + " " +
                        "WHERE " + db.Enquote(COLNAME_VALUE) + " = " + db.GetSqlString(value);

                List<UInt32> rowNumbers = new List<uint>();

                reader = db.ExecuteReader(sql);

                //if (!reader.HasRows) return rowNumbers;

                if (!reader.Read())
                    return rowNumbers;

                int length = Convert.ToInt32(reader[2]);
                Byte[] tmp = new Byte[length];
                reader.GetBytes(1, 0, tmp, 0, (int)length);

                int pos = 0;
                while (pos < length) {
                    rowNumbers.Add(BitConverter.ToUInt32(tmp, pos));
                    pos += 4;
                }

                if (reader.GetBoolean(0)) {
                    reader.Close();

                    StringBuilder sb = new StringBuilder();
                    foreach (UInt32 id in rowNumbers) {
                        sb.Append(id).Append(",");
                    }

                    rowNumbers.Clear();

                    reader = db.ExecuteReader(
                        "SELECT " + db.Enquote(COLNAME_ROWIDS) + ",LENGTH(" + db.Enquote(COLNAME_ROWIDS) + ") FROM " + 
                        db.Enquote(DistinctDbName, AssignedColumn.RowNumberTableName) +
                        " WHERE id IN (" + sb.Remove(sb.Length - 1, 1).ToString() + ")");

                    while (reader.Read()) {
                        length = Convert.ToInt32(reader[1]);
                        tmp = new Byte[length];
                        reader.GetBytes(0, 0, tmp, 0, (int)length);
                        pos = 0;

                        while (pos < length) {
                            rowNumbers.Add(BitConverter.ToUInt32(tmp, pos));
                            pos += 4;
                        }

                        //if (rowNumbers.Count > 3000000) break;
                    }
                }

                rowNumbers.Sort();
                return rowNumbers;
            } catch (Exception ex) {
                throw ex;
            } finally {
                if (reader != null) reader.Close();
            }
        }

        public List<UInt32> GetRowNumbers(IDatabase db, string value) {

            IDataReader reader = null;
            try {
                string sql =
                        "SELECT " +
                            db.Enquote(COLNAME_HAS_ROWNUMBER_TABLE) + "," +
                            db.Enquote(COLNAME_ROWIDS) + "," +
                            "LENGTH(" + db.Enquote(COLNAME_ROWIDS) + ") " +
                        "FROM " + db.Enquote(DistinctDbName, AssignedColumn.IndexTableName) + " " +
                        "WHERE LCASE(" + db.Enquote(COLNAME_VALUE) + ") LIKE " + db.GetSqlString("%" + value.ToLower() + "%");

                List<UInt32> rowNumbers = new List<uint>();
                reader = db.ExecuteReader(sql);

                //if (!reader.HasRows) return rowNumbers;

                if (!reader.Read())
                    return rowNumbers;

                int length = Convert.ToInt32(reader[2]);
                Byte[] tmp = new Byte[length];
                reader.GetBytes(1, 0, tmp, 0, (int)length);

                int pos = 0;
                while (pos < length) {
                    rowNumbers.Add(BitConverter.ToUInt32(tmp, pos));
                    pos += 4;
                }

                if (reader.GetBoolean(0)) {
                    reader.Close();

                    StringBuilder sb = new StringBuilder();
                    foreach (UInt32 id in rowNumbers) {
                        sb.Append(id).Append(",");
                    }

                    rowNumbers.Clear();

                    reader = db.ExecuteReader(
                        "SELECT " + db.Enquote(COLNAME_ROWIDS) + ",LENGTH(" + db.Enquote(COLNAME_ROWIDS) + ") FROM " +
                        db.Enquote(DistinctDbName, AssignedColumn.RowNumberTableName) +
                        " WHERE id IN (" + sb.Remove(sb.Length - 1, 1).ToString() + ")");

                    while (reader.Read()) {
                        length = Convert.ToInt32(reader[1]);
                        tmp = new Byte[length];
                        reader.GetBytes(0, 0, tmp, 0, (int)length);

                        pos = 0;
                        while (pos < length) {
                            rowNumbers.Add(BitConverter.ToUInt32(tmp, pos));
                            pos += 4;
                        }

                        //if (rowNumbers.Count > 3000000) break;
                    }
                }

                rowNumbers.Sort();
                return rowNumbers;
            } catch (Exception ex) {
                throw ex;
            } finally {
                if (reader != null) reader.Close();
            }
        }

        public long Count(IDatabase db) {
            return db.CountTable(DistinctDbName, AssignedColumn.IndexTableName);
        }

    }    
}
