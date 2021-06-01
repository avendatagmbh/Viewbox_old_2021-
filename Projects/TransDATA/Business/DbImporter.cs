// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-10-06
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Business.Enums;
using Business.Structures;
using Config.DbStructure;
using Config.Interfaces.Config;
using Config.Interfaces.DbStructure;
using Utils;
using Business.Interfaces;
using DbAccess.Structures;
using DbAccess;
using Logging;
using Business.Structures.DateReaders;
using GenericOdbc;

namespace Business {
    internal class DbImporter : IDbImporter {
        internal DbImporter(IProfile profile) {
            Profile = profile;
            ImportProgress = new TransferProgress();
            UICulture = System.Threading.Thread.CurrentThread.CurrentUICulture;

            DbConfig = new DbConfig("MySQL") {
                Hostname = "localhost",
                Username = "root",
                Password = "avendata"
            };
        }


        #region events
        public event EventHandler Finished;
        public void OnFinished() { if (Finished != null) Finished(this, new EventArgs()); }
        #endregion

        private ConnectionManager ConnectionManager { get; set; }
        private DbConfig DbConfig { get; set; }
        private CultureInfo UICulture { get; set; }
        private TransferProgress ImportProgress { get; set; }
        private bool IsCancelled { get; set; }
        private IProfile Profile { get; set; }
        private List<ITable> Tables { get; set; }
        private List<IDataReader> TableDataReader { get; set; }

        ITransferProgress IDbImporter.ImportProgress { get { return ImportProgress; } }

        public void Start() { Task.Factory.StartNew(Export); }

        public void Cancel() {
            IsCancelled = true;
            lock (TableDataReader) {
                foreach (IDataReader tableDataReader in TableDataReader)
                    tableDataReader.Cancel();
            }
        }

        private static string GetCsvFileName(string tableName) {
            var invalidChars = new[] {
                (char) 34, // "
                (char) 42, // *
                (char) 47, // /
                (char) 58, // :
                (char) 60, // <
                (char) 62, // >
                (char) 63, // ?
                (char) 92, // \
                (char) 124 // | 
            };

            string tmp = invalidChars.Aggregate(tableName,
                                                (current, invalidChar) =>
                                                current.Replace(invalidChar.ToString(), "%" + ((int)invalidChar) + "%"));
            for (int i = 0; i <= 31; i++)
                tmp = tmp.Replace(((char)i).ToString(), "%" + i + "%");
            return tmp + ".csv";
        }

        private void AddTableDataReader(IDataReader reader) {
            lock (TableDataReader) {
                TableDataReader.Add(reader);
            }
        }

        private void RemoveTableDataReader(IDataReader reader) {
            lock (TableDataReader) {
                TableDataReader.Remove(reader);
            }
        }

        /// <summary>
        /// Exports all selected tables.
        /// </summary>
        private void Export() {
            new LoggingDb(Config.ConfigDb.ConnectionManager);

            System.Threading.Thread.CurrentThread.CurrentUICulture = UICulture;

            try {
                using (var conn = DbAccess.ConnectionManager.CreateConnection(DbConfig)) {
                    conn.Open();
                    using (var sourceDbConnection = ConnectionManager.CreateConnection("GenericODBC", ((IDatabaseInputConfig)Profile.InputConfig).ConnectionString))
                    {
                        conn.CreateDatabaseIfNotExists("test" + "_transfer");
                    }
                }

                DbConfig.DbName = "test" + "_transfer";
                ConnectionManager = new DbAccess.ConnectionManager(DbConfig, 10);

                // init table list
                Tables = new List<ITable>();
                foreach (ITable table in Profile.Tables.Where(table => table.DoExport)) {
                    if (IsCancelled) break;
                    Tables.Add(table);
                }

                ImportProgress.EntitiesTotal = Tables.Count;
                ImportProgress.Step = TransferProgressSteps.ExportingTables;

                if (IsCancelled) return;

                TableDataReader = new List<IDataReader>();

                // init export tasks
                int taskCount = Math.Min(Environment.ProcessorCount, 1);
                var tasks = new Task[taskCount];
                for (int i = 0; i < taskCount; i++) {
                    tasks[i] = Task.Factory.StartNew(ExportTable, TaskCreationOptions.LongRunning);
                }

                // wait until all export tasks are finished or cancelled
                Task.WaitAll(tasks);

                if (!IsCancelled) {
                    ImportProgress.Step = TransferProgressSteps.GeneratingIndexXml;
                }
            } catch (Exception ex) {
                ImportProgress.AddErrorMessage(ex.Message);
            } finally {
                if (!IsCancelled) OnFinished();
            }
        }

        #region export
        /// <summary>
        /// Starts a new table exports until no more tables remain.
        /// </summary>
        private void ExportTable() {

            System.Threading.Thread.CurrentThread.CurrentUICulture = UICulture;

            ITable table;
            while (!IsCancelled && GetNextTable(out table)) {
                try {
                    ExportTable(table);
                } catch (Exception ex) {
                    ImportProgress.AddErrorMessage(ex.Message);
                }
            }
        }

        /// <summary>
        /// Gets the next table from table list.
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        private bool GetNextTable(out ITable table) {
            lock (Tables) {
                if (Tables.Count > 0) {
                    table = Tables.First();
                    Tables.RemoveAt(0);
                } else {
                    table = null;
                }
            }
            return table != null;
        }

        /// <summary>
        /// Exports the specified table.
        /// </summary>
        /// <param name="table"></param>
        private void ExportTable(ITable table) {
            try {
                using (var conn = ConnectionManager.GetConnection())
                {

                    conn.DropTableIfExists(table.Name);
                    var columnInfos = new List<DbColumnInfo>();
                    columnInfos.Add(new DbColumnInfo { Name = "_row_no_", Type = DbColumnTypes.DbInt, IsUnsigned = true, AutoIncrement = true, IsPrimaryKey = true });
                    foreach (ITableColumn column in table.Columns)
                    {
                        if ((column.Name == "_row_no_") || (!column.DoExport)) continue;
                        columnInfos.Add(new DbColumnInfo { Name = column.Name, Type = DbColumnTypes.DbLongText });
                    }
                    conn.CreateTable(table.Name, columnInfos);

                    var tableProgress = ImportProgress.AddProcessedEntity(table) as TransferTableProgress;
                    int datasetCount = 0;

                    //using (ITableDataReader r = SourceDbManager.GetTableDataReader(Profile.ConnectionString, table)) {
                    //using (ITableDataReader r = SourceDbManager.GetTableDataReader(((IDatabaseConfig)Profile.InputConfig).ConnectionString, table))
                    using (var connInput = ConnectionManager.CreateConnection("GenericODBC",
                    ((IDatabaseInputConfig)Profile.InputConfig.Config).ConnectionString,
                    DbTemplateManager.GetTemplate(((IDatabaseInputConfig)Profile.InputConfig.Config).DbTemplateName)))
                    {
                        using (IDataReader r = new TableDataReader(connInput, table))
                        {
                            if (IsCancelled) return;

                            AddTableDataReader(r);
                            r.Load();

                            while (!IsCancelled && r.Read())
                            {
                                object[] data = r.GetData();
                                int pos = 1;
                                DbColumnValues values = new DbColumnValues();
                                foreach (ITableColumn column in table.Columns)
                                {
                                    if ((column.Name == "_row_no_") || (!column.DoExport)) continue;
                                    values[column.Name] = data[pos++];
                                }

                                conn.InsertInto(table.Name, values);
                                datasetCount++;
                                if (datasetCount % 100 == 0)
                                {
                                    tableProgress.DataSetsProcessed += datasetCount;
                                    datasetCount = 0;
                                }
                            }

                            RemoveTableDataReader(r);
                        }
                    }
                }

                ImportProgress.RemoveProcessedEntity(table);
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
        #endregion export
    }
}