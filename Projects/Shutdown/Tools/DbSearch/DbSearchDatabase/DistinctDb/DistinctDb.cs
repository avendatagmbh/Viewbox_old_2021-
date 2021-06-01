using System.Collections.Generic;
using DbAccess;
using DbAccess.Structures;
using System;

namespace DbSearchDatabase.DistinctDb {

    public class DistinctDb {

        public DistinctDb(DbConfig dbConfig) {
            DbConfig config = (DbConfig) dbConfig.Clone();
            config.DbName = dbConfig.DbName + "_idx";
            using (var conn = ConnectionManager.CreateConnection(config)) {
                _dbName = conn.DbConfig.DbName;
                conn.Open();
                conn.SetHighTimeout();
                Tables = new Dictionary<string, DistinctTableInfo>(StringComparer.InvariantCultureIgnoreCase);


                foreach (DistinctTableInfo ti in DistinctTableInfo.OpenTableInfos(conn)) {
                    Tables[ti.Name] = ti;
                }
            }
        }

        //Only load table info for one table
        public DistinctDb(DbConfig dbConfig, string tableName) {
            DbConfig config = (DbConfig) dbConfig.Clone();
            config.DbName = dbConfig.DbName + "_idx";
            _dbName = config.DbName;
            using(var conn = ConnectionManager.CreateConnection(config)){
                conn.Open();
                conn.SetHighTimeout();
                Tables = new Dictionary<string, DistinctTableInfo>(StringComparer.InvariantCultureIgnoreCase);
                Tables[tableName] = DistinctTableInfo.OpenTableInfo(conn, tableName);
            }
        }
        private readonly string _dbName;
        /// <summary>
        /// Gets or sets the tables.
        /// </summary>
        /// <value>The tables.</value>
        public Dictionary<string, DistinctTableInfo> Tables { get; private set; }

        /// <summary>
        /// Gets the index table.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="columm">The columm.</param>
        /// <returns></returns>
        public DistinctIndexTable GetIndexTable(string table, string columm) {
            if (!Tables.ContainsKey(table) || !Tables[table].Columns.ContainsKey(columm)) return null;
            return new DistinctIndexTable(_dbName, Tables[table].Columns[columm]);
        }

    }
}
