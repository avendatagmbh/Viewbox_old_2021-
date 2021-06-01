using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;
using Config.Interfaces.DbStructure;
using DbAccess.Structures;
using Base.Localisation;
using Logging.DbStructure;

namespace Logging {
    public class LoggingDb {
        internal static ConnectionManager ConnectionManager { get; set; }

        public LoggingDb(ConnectionManager connectionManager) {
            ConnectionManager = connectionManager;

            using (IDatabase conn = ConnectionManager.GetConnection()) {
                // create tables
                CreateTable<Table>(conn);
                CreateTable<Column>(conn);
                CreateTable<Performance>(conn);
            }
        }

        #region CreateTable
        private static void CreateTable<T>(IDatabase conn) {
            try {
                conn.DbMapping.CreateTableIfNotExists<T>();
            } catch (Exception ex) {
                throw new Exception(
                    string.Format(
                        ExceptionMessages.CreateTableFailed,
                        conn.DbMapping.GetTableName<T>(),
                        ex.Message),
                    ex);
            }
        }
        #endregion

        public Interfaces.DbStructure.ITable CreateLogEntity(ITable entity, IProfile profile) {
            Interfaces.DbStructure.ITable result = new Table() {
                TableId = entity.Id,
                ProfileId = profile.Id,
                InputConfig = profile.InputConfig.Config.GetXmlRepresentation(),
                OutputConfig = profile.OutputConfig.Config.GetXmlRepresentation(),
                Timestamp = System.DateTime.Now,
                Filter = entity.Filter,
                State = ExportStates.InProgress
            };
            foreach (var col in entity.Columns) {
                if(!col.DoExport) continue;
                result.AddColumn(new Column() {ColumnId = col.Id, TableDbMapping = (Table) result});
            }
            return result;
        }

        public Interfaces.DbStructure.ITable CreateLogEntity(IFile entity) { throw new NotImplementedException(); }

        public void Save(Interfaces.DbStructure.ITable logEntity) {
            using (IDatabase conn = ConnectionManager.GetConnection()) {
                try {
                    conn.BeginTransaction();
                    conn.DbMapping.Save(logEntity);
                    conn.CommitTransaction();
                } catch (Exception) {
                    conn.RollbackTransaction();
                    throw;
                }
            }
        }

        public void Save(Interfaces.DbStructure.IPerformance performance) {
            using (IDatabase conn = ConnectionManager.GetConnection()) {
                try {
                    conn.BeginTransaction();
                    conn.DbMapping.Save(performance);
                    conn.CommitTransaction();
                } catch (Exception) {
                    conn.RollbackTransaction();
                    throw;
                }
            }
        }

        public Interfaces.DbStructure.IPerformance CreatePerformanceLogEntity() {
            return new Performance();
        }

        public List<Interfaces.DbStructure.ITable> GetLogTables(long id) {
            List<Interfaces.DbStructure.ITable> result = new List<Interfaces.DbStructure.ITable>();
            using (IDatabase conn = ConnectionManager.GetConnection()) {
                foreach (var table in conn.DbMapping.Load<DbStructure.Table>("table_id = " + id)) {
                    result.Add(table);
                }
            }
            return result;
        }
    }
}

