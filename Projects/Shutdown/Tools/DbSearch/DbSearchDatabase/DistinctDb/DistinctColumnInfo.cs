using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DbAccess;
using DbAccess.Structures;

namespace DbSearchDatabase.DistinctDb {
    [DbTable("columns")]
    public class DistinctColumnInfo {

        private const string TABLENAME = "columns";

        private const string COLNAME_ID = "cid";
        private const string COLNAME_TABLE_ID = "tid";
        private const string COLNAME_NAME = "name";
        private const string COLNAME_SIZE = "size";

        public DistinctColumnInfo() {
        }

        public DistinctColumnInfo(DistinctTableInfo tableInfo, string columnName) {
            this.Id = Id;
            this.TableInfo = tableInfo;
            this.Name = columnName;
        }

        public string IndexTableName { get { return "idx_" + this.Id.ToString(); } }


        public string RowNumberTableName {
            get { return "rowno_" + this.Id.ToString(); }
        }
       
        [DbColumn("cid"), DbPrimaryKey]
        public int Id { get; set; }
        [DbColumn("tid",AllowDbNull = false)]
        public int TableId { get; set; }
        [DbColumn("name", Length = 256)]
        public string Name { get; set; }
        [DbColumn("size")]
        public long Size { get; set; }
        [DbColumn("started")]
        public DateTime Started { get; set; }
        [DbColumn("finished")]
        public DateTime Finished { get; set; }

        public DistinctTableInfo TableInfo { get; internal set; }
        public DbColumnInfo DbColumnInfo { get; set; }



        private long _count = -1;

        internal void Save(IDatabase db) {
            if (TableInfo != null) TableId = TableInfo.Id;
            try {
                db.ExecuteNonQuery("LOCK TABLES columns WRITE");
                db.DbMapping.Save(this);
                
            } finally {
                db.ExecuteNonQuery("UNLOCK TABLES");
            }
            //if (this.Id > 0) {
            //    string filter = db.Enquote(COLNAME_TABLE_ID) + " = " + this.TableInfo.Id + " AND " + db.Enquote(COLNAME_ID) + " = " + this.Id;
                
            //    DbColumnValues fieldValues = new DbColumnValues();
            //    fieldValues[COLNAME_NAME] = this.Name;
            //    fieldValues[COLNAME_SIZE] = this.Size;
            //    db.Update(TABLENAME, fieldValues, filter);
            
            //} else {
            //    DbColumnValues fieldValues = new DbColumnValues();
            //    fieldValues[COLNAME_TABLE_ID] = this.TableInfo.Id;
            //    fieldValues[COLNAME_NAME] = this.Name;
            //    fieldValues[COLNAME_SIZE] = this.Size;
            //    db.InsertInto(TABLENAME, fieldValues);   

            //    // Receive last insert id.
            //    this.Id = (UInt32)db.ExecuteScalar(
            //        "SELECT " + db.Enquote(COLNAME_ID) + " " +
            //        "FROM " + db.Enquote(TABLENAME) + " " +
            //        "WHERE " + db.Enquote(COLNAME_TABLE_ID) + "=" + this.TableInfo.Id + " " +
            //        "AND " + db.Enquote(COLNAME_NAME) + "='" + this.Name + "'");
            //}

        }

        internal static void CreateTable(IDatabase db) {
            db.DbMapping.CreateTableIfNotExists<DistinctColumnInfo>();
            //string sSql =
            //        "CREATE TABLE IF NOT EXISTS " + TABLENAME + " (" +
            //            db.Enquote(COLNAME_ID) + " INT UNSIGNED NOT NULL AUTO_INCREMENT," +
            //            db.Enquote(COLNAME_TABLE_ID) + " INT UNSIGNED NOT NULL," +
            //            db.Enquote(COLNAME_NAME) + " VARCHAR(64) NOT NULL," +
            //            db.Enquote(COLNAME_SIZE) + " BIGINT UNSIGNED NOT NULL DEFAULT 0," +
            //        "PRIMARY KEY(" + db.Enquote(COLNAME_ID) + ")," +
            //        "UNIQUE KEY(" + db.Enquote(COLNAME_TABLE_ID) + "," + db.Enquote(COLNAME_NAME) + ")," +
            //        "FOREIGN KEY(" + db.Enquote(COLNAME_TABLE_ID) + ") REFERENCES " +
            //        db.Enquote(DistinctTableInfo.TABLENAME) + "(" + db.Enquote(COLNAME_TABLE_ID) + ")" +
            //        ") engine=MyISAM";

            //db.ExecuteNonQuery(sSql);
        }

        internal static void OpenColumnInfos(IDatabase db, DistinctTableInfo ti) {
            string sql =
                "SELECT " +
                    db.Enquote(COLNAME_ID) + ", " +
                    db.Enquote(COLNAME_NAME) + ", " +
                    db.Enquote(COLNAME_SIZE) + " " +
                "FROM " + db.Enquote(TABLENAME) + " " +
                "WHERE " + db.Enquote(COLNAME_TABLE_ID) + " = " + ti.Id;

            IDataReader reader = null;
            try {
                ti.Columns.Clear();
                reader = db.ExecuteReader(sql);
                while (reader.Read()) {
                    DistinctColumnInfo ci = new DistinctColumnInfo();
                    ci.Id = Convert.ToInt32(reader.GetValue(0));
                    ci.Name = reader.GetValue(1).ToString();
                    ci.Size = Convert.ToInt64(reader.GetValue(2));
                    ci.TableInfo = ti;
                    ti.Columns.Add(ci.Name, ci);
                }
            } finally {
                if (reader != null) reader.Close();
            }
        }

        public long Count(IDatabase conn) {
            if (_count == -1) {
                _count = conn.CountTable(IndexTableName);
            }
            return _count;
        }

        public void GetRowIdsForRowNoTable(List<int> result, IDatabase conn, List<int> rowIdTables ) {
            using (
                IDataReader rowIdReader =
                    conn.ExecuteReader("SELECT rowIds FROM " + RowNumberTableName + " WHERE id in (" + string.Join(",", rowIdTables) + ")")) {
                while (rowIdReader.Read()) {
                    ReaderBufferToRowNumbers(rowIdReader, 0, result);
                }
            }
        }

        public List<int> GetRowIdsToDistinctIds(List<uint> distinctIds, IDatabase conn) {
            if(distinctIds.Count == 0) return new List<int>();
            List<int> result = new List<int>();
            using (IDataReader reader = conn.ExecuteReader("SELECT rowIds,hasRowNoTable FROM " + IndexTableName + 
                " WHERE id in (" + string.Join(",",distinctIds) + ")")) {
                while (reader.Read()) {
                    //Check if hasRowNoTable is false
                    if(reader.GetInt32(1) == 0) ReaderBufferToRowNumbers(reader, 0, result);
                    else {
                        //A special table has been used to store the row ids as they did not fit into a blob
                        List<int> idsInRowNoTable = new List<int>();
                        ReaderBufferToRowNumbers(reader, 0, idsInRowNoTable);
                        using (IDatabase newConn = ConnectionManager.CreateConnection(conn.DbConfig)) {
                            newConn.Open();
                            GetRowIdsForRowNoTable(result, newConn, idsInRowNoTable);
                        }
                    }
                }
            }
            return result;
        }

        //Converts the byte stream into a list of row numbers, given a reader and the column ordinal containing the bytes
        public void ReaderBufferToRowNumbers(IDataReader reader, int columnOrdinal, List<int> rowNumbers) {
            const int size = 1024;
            byte[] buffer = new byte[size];
            long offset = 0;
            long readBytes;
            long blobSize = reader.GetBytes(columnOrdinal, 0, null, 0, 0);

            do {
                readBytes = reader.GetBytes(columnOrdinal, offset, buffer, 0, size);
                offset += readBytes;
                for (int i = 0; i < readBytes; i += 4)
                    rowNumbers.Add(BitConverter.ToInt32(buffer, i));
            } while (readBytes == size && offset < blobSize);
        }

        public Dictionary<string,List<int>> GetValueToRows(int howMany, IDatabase conn) {
            Dictionary<string,List<int>> result = new Dictionary<string, List<int>>();
            using (IDataReader reader = conn.ExecuteReader("SELECT rowIds,value FROM " + IndexTableName + " WHERE hasRowNoTable=0 ORDER BY length(rowIds)" + " LIMIT " + howMany)) {
                while (reader.Read()) {
                    //Read back byte buffer and convert to row numbers
                    List<int> rowNumbers = new List<int>();
                    ReaderBufferToRowNumbers(reader, 0, rowNumbers);
                    result[reader.GetString(1)] = rowNumbers;
                }
            }
            return result;
        }
    }
}
