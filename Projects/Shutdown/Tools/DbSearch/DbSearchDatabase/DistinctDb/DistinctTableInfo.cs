using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DbAccess;
using DbAccess.Structures;

namespace DbSearchDatabase.DistinctDb {
    [DbTable("tables")]
    public class DistinctTableInfo {

        public const string TABLENAME = "tables";

        private const string COLNAME_ID = "tid";
        private const string COLNAME_NAME = "name";
        private const string COLNAME_COUNT = "count";
        private const string COLNAME_TIMESTAMP = "timestamp";

        public DistinctTableInfo() {
            this.Columns = new Dictionary<string, DistinctColumnInfo>(StringComparer.OrdinalIgnoreCase);
        }

        public DistinctTableInfo(string tableName) {
            this.Id = 0;
            this.Name = tableName;
            this.Columns = new Dictionary<string, DistinctColumnInfo>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        [DbColumn("tid"), DbPrimaryKey]
        public int Id { get; private set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        [DbColumn("name", Length = 256)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the count.
        /// </summary>
        /// <value>The count.</value>
        [DbColumn("count")]
        public long Count { get; set; }

        [DbColumn("started")]
        public DateTime Started { get; set; }
        //For compatibility reasons, this column is called timestamp
        [DbColumn("timestamp")]
        public DateTime Finished { get; set; }

        public Dictionary<string, DistinctColumnInfo> Columns { get; private set; }

        public void Save(IDatabase db) {
            db.DbMapping.Save(this);
            //DbColumnValues fieldValues = new DbColumnValues();
            //fieldValues[COLNAME_NAME] = this.Name;
            //fieldValues[COLNAME_COUNT] = this.Count;
            //fieldValues[COLNAME_TIMESTAMP] = System.DateTime.Now;
            //db.InsertInto(TABLENAME, fieldValues);

            //// Receive last insert id.
            //this.Id = (uint)db.ExecuteScalar(
            //    "SELECT " + db.Enquote(COLNAME_ID) + " " +
            //    "FROM " + db.Enquote(TABLENAME) + " " +
            //    "WHERE " + db.Enquote(COLNAME_NAME) + "='" + this.Name + "'");

            //foreach (DistinctColumnInfo ci in this.Columns.Values) ci.Save(db);
        }

        public static DistinctTableInfo OpenTableInfo(IDatabase db, string tableName) {

            DistinctTableInfo ti = null;
            
            string sql =
                "SELECT " +
                    db.Enquote(COLNAME_ID) + ", " +
                    db.Enquote(COLNAME_NAME) + ", " +
                    db.Enquote(COLNAME_COUNT) + " " +
                "FROM " + db.Enquote(TABLENAME) + " " +
                "WHERE LOWER(" + db.Enquote(COLNAME_NAME) + ")=" + db.GetSqlString(tableName.ToLower());

            IDataReader reader = null;
            try {
                reader = db.ExecuteReader(sql);

                //if (!reader.HasRows) return null;

                if (!reader.Read())
                    return null;

                ti = new DistinctTableInfo();
                ti.Id = (int)reader.GetValue(0);
                ti.Name = reader.GetValue(1).ToString();
                ti.Count = (long)reader.GetValue(2);
                
                reader.Close();

                DistinctColumnInfo.OpenColumnInfos(db, ti);

            } catch (Exception ex) {
                throw ex;
            } finally {
                if (reader != null) reader.Close();
            }

            return ti;
        }

        /// <summary>
        /// Opens the table infos.
        /// </summary>
        /// <param name="db">The db.</param>
        /// <returns></returns>
        public static List<DistinctTableInfo> OpenTableInfos(IDatabase db) {
            List<DistinctTableInfo> result = new List<DistinctTableInfo>();

            string sql =
                "SELECT " +
                    db.Enquote(COLNAME_ID) + ", " +
                    db.Enquote(COLNAME_NAME) + ", " +
                    db.Enquote(COLNAME_COUNT) + " " +
                "FROM " + db.Enquote(TABLENAME);

            IDataReader reader = null;
            try {
                reader = db.ExecuteReader(sql);
                while (reader.Read()) {
                    DistinctTableInfo ti = new DistinctTableInfo();
                    ti.Id = reader.GetInt32(0);
                    ti.Name = reader.GetString(1);
                    ti.Count = reader.GetInt64(2);
                    result.Add(ti);
                }
                reader.Close();

                // load assigned columnn infos
                foreach (DistinctTableInfo ti in result) {
                    DistinctColumnInfo.OpenColumnInfos(db, ti);
                }
            } catch (Exception ex) {
                throw ex;
            } finally {
                if (reader != null) reader.Close();
            }

            return result;
        }

        /// <summary>
        /// Creates the table.
        /// </summary>
        /// <param name="db">The db.</param>
        public static void CreateTable(IDatabase db) {
            db.DbMapping.CreateTableIfNotExists<DistinctTableInfo>();
            //string sSql =
            //            "CREATE TABLE IF NOT EXISTS " + db.Enquote(TABLENAME) + " (" +
            //                db.Enquote(COLNAME_ID) + " INT UNSIGNED NOT NULL AUTO_INCREMENT," +
            //                db.Enquote(COLNAME_NAME) + " VARCHAR(64) NOT NULL," +
            //                db.Enquote(COLNAME_COUNT) + " INT UNSIGNED NOT NULL," +
            //                db.Enquote(COLNAME_TIMESTAMP) + " DATETIME," +
            //            "PRIMARY KEY(" + db.Enquote(COLNAME_ID) + ")," +
            //            "UNIQUE KEY(" + db.Enquote(COLNAME_NAME) + ")" +
            //            ") engine=MyISAM";

            //db.ExecuteNonQuery(sSql);
        }
    }
}
