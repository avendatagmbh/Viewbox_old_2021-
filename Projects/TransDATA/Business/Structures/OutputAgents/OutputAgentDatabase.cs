// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-11
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AV.Log;
using Business.Interfaces;
using Business.Structures.DateReaders;
using Business.Structures.MetadataAgents;
using Config.Enums;
using Config.Interfaces.DbStructure;
using Config.Structures;
using DbAccess;
using Config.Interfaces.Config;
using DbAccess.DbSpecific.ADO;
using DbAccess.DbSpecific.SQLServer;
using GenericOdbc;
using System.Collections.Generic;
using DbAccess.Structures;
using System.Threading;
using Ionic.Zip;
using Logging;
using AvdCommon.Logging;
using log4net;
using Database = DbAccess.DbSpecific.SQLServer.Database;

namespace Business.Structures.OutputAgents {
    internal class OutputAgentDatabase : OutputAgentBase {
        internal ILog _log = LogHelper.GetLogger();

        //private ConnectionManager ConnManager { get; set; }
        private DbConfig DbConfig { get; set; }
        private IDataReader TableDataReader { get; set; }
        private Queue<List<DbColumnValues>> Queue { get; set; }
        private Thread InsertThread { get; set; }
        private bool ReadingFinished { get; set; }
        private int InsertPackageSize { get; set; }
        private TransferTableProgress Progress { get; set; }

        public OutputAgentDatabase(IProfile profile, IInputAgent inputAgent, IOutputConfig config)
            : base(profile, inputAgent, config) {
            IDatabaseOutputConfig outputConfig = (IDatabaseOutputConfig) profile.OutputConfig.Config;
            DbConfig = new DbConfig(outputConfig.DbType) {ConnectionString = outputConfig.ConnectionString};
            //ConnManager = connManager;
        }

        public override void InitTransfer() {
            _log.ContextLog( LogLevelEnum.Debug,"");

            DbConfig configWithoutDbName = (DbConfig) DbConfig.Clone();
            configWithoutDbName.DbName = "";
            using (var conn = ConnectionManager.CreateConnection(configWithoutDbName)) {
                _log.ContextLog( LogLevelEnum.Debug,"conn.Open");
                Stopwatch sw = new Stopwatch();
                sw.Start();
                conn.Open();
                sw.Stop();

                if (sw.Elapsed.TotalSeconds < 30) {
                    _log.ContextLog(LogLevelEnum.Debug, "conn.Open Elapsed time: {0}",
                                                     sw.Elapsed.ToString());
                } else {
                    _log.ContextLog(LogLevelEnum.Warn, "conn.Open Elapsed time: {0}",
                                                     sw.Elapsed.ToString());
                }


                _log.ContextLog(LogLevelEnum.Debug, "conn.IsOpen: {0}", conn.IsOpen ? "yes" : "no");

                sw.Reset();
                sw.Start();
                conn.CreateDatabaseIfNotExists(DbConfig.DbName);
                sw.Stop();

                _log.ContextLog(sw.Elapsed.TotalSeconds < 30 ? LogLevelEnum.Debug : LogLevelEnum.Warn,
                                                 "conn.CreateDatabaseIfNotExists DbName: {0} Elapsed time: {1}",
                                                 DbConfig.DbName,
                                                 sw.Elapsed.ToString());
            }
        }

        public override void ProcessEntity(ITransferEntity entity, TransferTableProgress progress, LoggingDb loggingDb,
                                           bool useAdo = false) {
            Stopwatch sw = new Stopwatch();

            _log.ContextLog(LogLevelEnum.Debug,
                                             "");

            _log.ContextLog(LogLevelEnum.Info,
                                             "entity.Id: {0} entity.Name: {1}",
                                             entity.Id, entity.Name);


            long datasetCount = 0;
            Logging.Interfaces.DbStructure.ITable logEntity = loggingDb.CreateLogEntity((ITable) entity, Profile);
            logEntity.Timestamp = progress.StartTime;

            _log.ContextLog( LogLevelEnum.Debug,
                "logEntity.Timestamp:",
                logEntity.Timestamp.ToUniversalTime());

            int baanCompanyIdLength = 0;
            bool cfgIsBaanCsv = false;
            string baanCompanyIdField = null;
            bool isCsv = false;

            if (Profile.InputConfig.Type == InputConfigTypes.Csv) {
                ICsvInputConfig cfg = Profile.InputConfig.Config as ICsvInputConfig;
                cfgIsBaanCsv = cfg.IsBaanCsv;
                baanCompanyIdLength = cfg.BaanCompanyIdLength;
                baanCompanyIdField = cfg.BaanCompanyIdField;
                isCsv = true;
            }


            try {
                logEntity.Count = InputAgent.GetCount(entity);

                using (var conn = GetOpenConnection()) {
                    IDatabaseOutputConfig config = (IDatabaseOutputConfig) Config.Config;
                    Queue = new Queue<List<DbColumnValues>>(config.QueuedInsertPackages);
                    ReadingFinished = false;

                    var insertSize = config.CountInsertLines;
                    InsertPackageSize = config.QueuedInsertPackages;


                    string database = string.Empty;
                    if (!string.IsNullOrEmpty(entity.Schema)) database = entity.Schema;
                    else if (Profile.InputConfig.Type == InputConfigTypes.Csv) {
                        var cfg = Profile.InputConfig.Config as ICsvInputConfig;
                        database =
                            new DirectoryInfo(new FileInfo((entity as ITable).FileNames[0]).DirectoryName).Name;
                    }


                    _log.ContextLog( LogLevelEnum.Debug,
                        "database: {0}",
                        database);


                    bool isBaanTable = false;
                    if (cfgIsBaanCsv) {
                        string i = (entity as ITable).FileNames.FirstOrDefault();

                        if (MetadataAgentCsv.BaanGetBaseFilename(i, baanCompanyIdLength) != i)
                            isBaanTable = true;
                    }
                    _log.ContextLog( LogLevelEnum.Debug,
                        "isBaanTable: {0}",
                        isBaanTable);


                    sw.Reset();
                    sw.Start();
                    string tableName = entity.DestinationName ?? entity.Name;
                    if (config.UseDatabaseTablePrefix) tableName = database + "__" + tableName;

                    // use only for debug information
                    string finalTablename = tableName;

                    if (!string.IsNullOrEmpty(database) && config.UseImportDatabases) {
                        finalTablename = tableName.ToLower().Replace(".csv", string.Empty);
                        conn.DropTableIfExists(database, finalTablename);
                    } else {
                        finalTablename = tableName.ToLower().Replace(".csv", string.Empty).Replace("-", "_");
                        conn.DropTableIfExists(finalTablename);
                    }
                    sw.Stop();

                    _log.ContextLog( LogLevelEnum.Debug,
                        "conn.DropTableIfExists Table: {0} Time: {1}",
                        finalTablename, sw.Elapsed.ToString());


                    var columnInfos = new List<DbColumnInfo>();
                    foreach (var column in entity.Columns) {
                        if (!column.DoExport) continue;

                        var columnName = column.Name.Replace("\"", string.Empty);


                        while (NameAlreadyExists(columnInfos, columnName)) {
                            columnName += "__DUPLICATE";
                        }
                        column.Name = columnName;
                        DbColumnInfo _columnInfo = new DbColumnInfo() {
                            Name = columnName,

                            //TODO: it is a hack while we dont't read any type information for CSV files
                            Type = isCsv ? DbColumnTypes.DbLongText:column.DbType,
                            
                            //AllowDBNull = true,
                            //MaxLength = column.MaxLength,
                            //AutoIncrement = column.AutoIncrement,
                            //NumericScale = column.NumericScale,
                            //Comment = column.Comment,
                            //DefaultValue = column.DefaultValue,
                            //IsIdentity = column.IsIdentity,
                            //IsPrimaryKey = column.IsPrimaryKey,
                            //IsUnsigned = column.IsUnsigned,
                            //OrdinalPosition = column.OrdinalPosition

                            //TODO: implement a proper solution for the MySql unsigned type conversion to MSSQL
                            // The following MySql unsigned types should be converted to the following MSSQL type
                            // MySql                MSSQL
                            // unsigned smallint    int
                            // unsigned mediumint   int
                            // unsigned int         bigint
                            // unsigned bigint      numeric(18,0)

                            AllowDBNull = true,
                            AutoIncrement = column.AutoIncrement,
                            Comment = column.Comment,
                            DefaultValue = column.DefaultValue,
                            IsIdentity = column.IsIdentity,
                            IsPrimaryKey = column.IsPrimaryKey,
                            OrdinalPosition = column.OrdinalPosition,
                            IsUnsigned = column.IsUnsigned,
                            OriginalType = column.TypeName,
                        };
                        //TODO: implement a proper solution for the MySql unsigned type conversion to MSSQL
                        if (!(config.IsMsSql && _columnInfo.IsUnsigned)){
                            _columnInfo.MaxLength = column.MaxLength;
                            _columnInfo.NumericScale = column.NumericScale;
                        } else {
                            _columnInfo.MaxLength = column.MaxLength;
                            _columnInfo.NumericScale = column.NumericScale;
                            switch (column.DbType) {
                                case DbColumnTypes.DbInt:
                                    _columnInfo.Type = DbColumnTypes.DbBigInt;
                                    break;
                                case DbColumnTypes.DbBigInt:
                                    _columnInfo.Type = DbColumnTypes.DbNumeric;
                                    _columnInfo.MaxLength = 0;
                                    _columnInfo.NumericScale = 0;
                                    break;
                                default:
                                    break;
                            }
                        }
                        columnInfos.Add(_columnInfo);
                    }


                    // If BAAN csv Add CompanyID column to table
                    if (cfgIsBaanCsv && isBaanTable)
                        columnInfos.Add(new DbColumnInfo
                                        {Name = baanCompanyIdField, Type = DbColumnTypes.DbLongText, AllowDBNull = true});


                    sw.Reset();
                    sw.Start();
                    string _tableName = "";
                    var engine = config.UseCompressDatabase ? "ARCHIVE" : string.Empty;
                    if (!string.IsNullOrEmpty(database) && config.UseImportDatabases) {
                        try {
                            conn.CreateDatabaseIfNotExists(database);

                            _tableName = tableName.ToLower().Replace(".csv", string.Empty).Replace("-", "_");
                            conn.CreateTable(database,
                                             _tableName,
                                             columnInfos, engine);
                        } catch (Exception ex) {
                            _log.ContextLog( LogLevelEnum.Error, 
                                "conn.CreateTable Table: {0} Error #1: {1}",
                                _tableName, ex.Message);
                            throw;
                        }
                    } else {
                        try {
                            _tableName = tableName.ToLower().Replace(".csv", string.Empty).Replace("-", "_");
                            conn.CreateTable(_tableName, columnInfos, engine);
                        } catch (Exception ex) {
                            _log.ContextLog( LogLevelEnum.Error, 
                                "conn.CreateTable Table: {0} Error #2: {1}",
                                _tableName, ex.Message);

                            throw;
                        }
                    }
                    sw.Stop();

                    _log.ContextLog( LogLevelEnum.Debug,
                        "conn.CreateTable Table: {0} Time: {1}",
                        _tableName, sw.Elapsed.ToString());


                    // do bulk-insert if source is csv
                    if (Profile.InputConfig.Type == InputConfigTypes.Csv) {
                        _log.ContextLog( LogLevelEnum.Debug,
                            " do bulk-insert, source is csv");

                        var cfg = Profile.InputConfig.Config as ICsvInputConfig;
                        var file = entity as ITable;
                        for (int i = 0; i < file.FileNames.Count; i++) {
                            var removeFile = string.Empty;
                            var fileName = file.FileNames[i];

                            _log.ContextLog( LogLevelEnum.Debug,
                                "Filename: {0}",
                                fileName);


                            if (fileName.ToLower().EndsWith(".zip")) {
                                sw.Reset();
                                sw.Start();
                                var fi = new FileInfo(fileName);
                                ZipFile zip = new ZipFile(fileName);
                                //zip.ExtractAll(new FileInfo(fileName).Directory.FullName);
                                zip.ExtractAll(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
                                //fileName = fileName.Substring(0, fileName.Length - 4);
                                fileName = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\" +
                                           fi.Name.Substring(0, fi.Name.Length - 4);
                                ;
                                removeFile = fileName;
                                sw.Stop();
                                _log.ContextLog( LogLevelEnum.Debug,
                                    "Unzip to: {0} Time: {1}",
                                    fileName, sw.Elapsed.ToString());
                            }

                            //ReplaceSingleDoublequotes(fileName);

                            string CompanyIDvalue = null;

                            if (cfgIsBaanCsv && isBaanTable) {
                                // determine CompanyID 
                                int tmp;
                                string tblname = fileName.ToLower().Replace(".csv", string.Empty).Replace("-", "_");
                                string compID = tblname.Substring(tblname.Length - cfg.BaanCompanyIdLength);
                                if (int.TryParse(compID, out tmp))
                                    CompanyIDvalue = compID;

                                _log.ContextLog( LogLevelEnum.Debug,
                                    "CompanyIDvalue: {0}",
                                    CompanyIDvalue);
                            }

                            // determine column names
                            sw.Reset();
                            sw.Start();
                            List<string> csvcolumns= new List<string>();
                            if (cfgIsBaanCsv && isBaanTable)
                            {
                                // if CSV is a BAAN table, the column names maybe different in each file
                                csvcolumns.AddRange(DbImport.CsvImport.CsvImporterBase.GetFieldNamesFromCsv(fileName, (ICsvInputConfig) Profile.InputConfig.Config).Select(col => ("`" + col + "`")));
                                // TODO: this shoud be used if entity conteanis the proper info for every single csv
                                //foreach (IColumn col in entity.Columns)
                                //    csvcolumns.Add(("`" + col.Name + "`"));

                            }
                            else {
                                csvcolumns.AddRange(columnInfos.Select(ci => ("`" + ci.Name + "`")));
                            }

                            sw.Stop();
                            _log.ContextLog( LogLevelEnum.Debug,
                                "determine column names. Time: {0}",
                                sw.Elapsed.ToString());


                            _log.ContextLog( LogLevelEnum.Debug,
                                "load csv data start");

                            sw.Reset();
                            sw.Start();

                            CsvInformation _info = new CsvInformation() {
                                Database =
                                    !string.IsNullOrEmpty(database) && config.UseImportDatabases
                                        ? database
                                        : string.Empty,
                                TableName = tableName.ToLower().Replace(".csv", string.Empty).Replace("-", "_"),
                                FileName = fileName.Replace("\\", "/"),
                                Encoding = cfg.FileEncoding ,
                                FieldSeperator = cfg.FieldSeperator,
                                LineSeperator = cfg.LineEndSeperator,
                                OptionallyEnclosedBy = cfg.OptionallyEnclosedBy,
                                IgnoreLines =
                                    (cfg.IsBaanCsv ||  (i==0 && cfg.HeadlineInFirstLine) || cfg.HeadlineInEachFileFirstLine)
                                        ? cfg.IgnoreLines + 1
                                        : cfg.IgnoreLines,
                                CsvColumns = cfg.IsBaanCsv ? csvcolumns:null,
                                CompanyIDfield = cfg.IsBaanCsv ?baanCompanyIdField:null,
                                CompanyIDValue = cfg.IsBaanCsv ?CompanyIDvalue:null
                            };

                            conn.LoadCSVData(_info);
                            sw.Stop();

                            _log.ContextLog( LogLevelEnum.Info,
                                "load csv data done. Time: {0}",
                                sw.Elapsed.ToString());

                            progress.DataSetsProcessed +=
                                (long) ((double) logEntity.Count/(double) (file.FileNames.Count - i));


                            progress.FilesProcessed += 1;

                            _log.ContextLog( LogLevelEnum.Info,
                                "DataSetsProcessed: {0} FilesProcessed: {1}",
                                progress.DataSetsProcessed, progress.FilesProcessed);

                            if (!string.IsNullOrEmpty(removeFile)) File.Delete(removeFile);
                        }

                        sw.Reset();
                        sw.Start();
                        if (!string.IsNullOrEmpty(database) && config.UseImportDatabases) {
                            datasetCount = conn.CountTable(database,
                                                           tableName.ToLower().Replace(".csv", string.Empty).Replace(
                                                               "-", "_"));
                        } else {
                            datasetCount =
                                conn.CountTable(tableName.ToLower().Replace(".csv", string.Empty).Replace("-", "_"));
                        }
                        sw.Stop();
                        _log.ContextLog( LogLevelEnum.Info,
                            "conn.CountTable: {0} Time: {1}",
                            datasetCount,
                            sw.Elapsed.ToString());
                    } else {
                        _log.ContextLog( LogLevelEnum.Debug,
                            "normal insert, not csv");


                        Progress = progress;

                        List<DbColumnValues> lValues = new List<DbColumnValues>();
                        datasetCount = 0;
                        var _config = ((IDatabaseOutputConfig) Profile.OutputConfig.Config);

                        if (_config.IsMsSql) {
                            using (var r = (TableDataReader) InputAgent.GetDataReader(entity)) {
                                TableDataReader = r;
                                r.Load();

                                Database _sqlConn = (Database) conn;

                                var _holder = new BulkDataHolder() {
                                    BatchSize = _config.BatchSize,
                                    Progress = new SqlRowsCopiedEventHandler(OnSqlRowsCopied),
                                    Reader = r.Reader,
                                    TableName = tableName,
                                    TableSchema = entity.Schema,
                                    ColumnInfos = columnInfos,
                                };
                                datasetCount = _sqlConn.SqlCopy(_holder);
                            }
                        } else {
                            using (var r = (TableDataReader) InputAgent.GetDataReader(entity)) {
                                TableDataReader = r;
                                r.Load();

                                while (r.Read()) {
                                    object[] data = r.GetData();

                                    int pos = 0;
                                    DbColumnValues values = new DbColumnValues();
                                    foreach (var column in columnInfos) {
                                        values[column.Name] = data[pos++];
                                    }
                                    lValues.Add(values);

                                    // //add values to queue for db-insert if limit reached / passed
                                    if (lValues.Count >= insertSize)
                                        AddToQueue(lValues);

                                    conn.InsertInto(tableName, values);
                                    datasetCount++;
                                    if (datasetCount%100 == 0) {
                                        progress.DataSetsProcessed += datasetCount;
                                    }
                                }
                            }
                        }
                        ReadingFinished = true;
                    }
                }
            } catch (Exception ex) {
                logEntity.Error = ex.Message;
                logEntity.State = ExportStates.Error;
                _log.ContextLog( LogLevelEnum.Info,
                    "entity.TransferState.State:TransferedError");
                entity.TransferState.State = TransferStates.TransferedError;
                entity.TransferState.Message = ex.Message;
                _log.ContextLog( LogLevelEnum.Error,  
                    "entity.TransferState.Message #1: {0}",
                    entity.TransferState.Message);
                throw;
            } finally {
                logEntity.CountDest = datasetCount;
                logEntity.Duration = DateTime.Now - progress.StartTime;
                loggingDb.Save(logEntity);

                if (string.IsNullOrEmpty(logEntity.Error)) {
                    if (logEntity.Count != logEntity.CountDest || entity.Count != logEntity.CountDest) {
                        _log.ContextLog( LogLevelEnum.Info,
                            "entity.TransferState.State:TransferedCountDifference");

                        //if (logEntity.Count != logEntity.CountDest) {
                        entity.TransferState.State = TransferStates.TransferedCountDifference;
                        entity.TransferState.Message = string.Format(
                            "Anzahl der Zeilen weicht ab. Die Quelle hat {0} Einträge, die Datenbank hat {1} Einträge.",
                            logEntity.Count, logEntity.CountDest);

                        _log.ContextLog( LogLevelEnum.Info,
                            "entity.TransferState.Message #2: {0}",
                            entity.TransferState.Message);
                    } else {
                        _log.ContextLog( LogLevelEnum.Info,
                            "entity.TransferState.State:TransferStates.TransferedOk");

                        entity.TransferState.State = TransferStates.TransferedOk;
                        entity.TransferState.Message = "Tabelle wurde korrekt übertragen";

                        _log.ContextLog( LogLevelEnum.Info,
                            "entity.TransferState.Message #3: {0}",
                            entity.TransferState.Message);
                    }
                    //entity.TransferState.State = logEntity.Count != logEntity.CountDest ? TransferStates.TransferedCountDifference : TransferStates.TransferedOk;
                }

                _log.ContextLog( LogLevelEnum.Debug,"entity.Save");

                entity.Save();
            }
        }


        private void OnSqlRowsCopied(object sender, SqlRowsCopiedEventArgs e) { Progress.DataSetsProcessed = e.RowsCopied; }

        private bool NameAlreadyExists(List<DbColumnInfo> columnInfos, string columnName) {
            foreach (var dbColumnInfo in columnInfos) {
                if (dbColumnInfo.Name.ToLower().Equals(columnName.ToLower())) return true;
            }
            return false;
        }

        public override void CompleteTransfer() {
            _log.ContextLog( LogLevelEnum.Debug,"");

            // nothing to do
        }

        public override void Cancel() {
            _log.ContextLog( LogLevelEnum.Debug,"");

            base.Cancel();
            if (TableDataReader != null) TableDataReader.Cancel();
        }

        //private  string GetValidTableName(string name) { return name; }

        private void AddToQueue(List<DbColumnValues> lValues) {
            _log.ContextLog( LogLevelEnum.Debug,
                "");

            if (lValues == null || lValues.Count == 0) return;

            bool isQueued = false;
            while (!isQueued) {
                lock (Queue) {
                    if (Queue.Count < InsertPackageSize) {
                        Queue.Enqueue(lValues.ToList());
                        lValues.Clear();
                        isQueued = true;
                    }
                }
                if (!isQueued) Thread.Sleep(1000);
            }
        }

        private void AddQueueToDatabase(string tabName) {
            _log.ContextLog( LogLevelEnum.Debug,
                " tabName: {0}", tabName);

            string tableName = tabName;
            using (var conn = GetOpenConnection()) {
                while (!ReadingFinished) {
                    List<DbColumnValues> lValues = null;
                    lock (Queue) {
                        if (Queue.Count > 0) lValues = Queue.Dequeue();
                    }
                    if (lValues == null) {
                        Thread.Sleep(1000);
                    } else {
                        conn.InsertInto(tableName, lValues);
                    }
                }
                while (Queue.Count > 0) conn.InsertInto(tableName, Queue.Dequeue());
            }
        }

        private IDatabase GetOpenConnection() {
            _log.ContextLog( LogLevelEnum.Debug,
                "");


            IDatabase conn = ConnectionManager.CreateConnection(DbConfig);
            if (!conn.IsOpen) {
                conn.Open();
            }

            _log.ContextLog( LogLevelEnum.Debug,
                "conn.IsOpen: {0} ConnectionString: {1}",
                conn.IsOpen, DbConfig.ConnectionString);
            return conn;
        }

        public override bool CheckDataAccess() {
            bool ret = false;
            try {
                DbConfig configWithoutDbName = (DbConfig) DbConfig.Clone();
                configWithoutDbName.DbName = "";
                using (var conn = ConnectionManager.CreateConnection(configWithoutDbName)) {
                    if (!conn.IsOpen)
                        conn.Open();

                    ret = conn.IsOpen;
                }
            } catch (Exception) {
                ret = false;
            }

            _log.ContextLog( LogLevelEnum.Debug,
                "conn.IsOpen: {0} ConnectionString: {1}",
                ret, DbConfig.ConnectionString);
            return ret;
        }

        public override string GetDescription() { return "Datenbank"; }

        public void ReplaceSingleDoublequotes(string file) {
            string PATH = @"C:\Users\mag\AppData\Roaming";
            Regex regex = new Regex("(?<=.)(?<!>)\"(?!<)");

            FileInfo fi = new FileInfo(file);
            File.Move(file, PATH + @"\_" + fi.Name);


            var reader = new StreamReader(PATH + @"\_" + fi.Name, Encoding.UTF8);
            var writer = new StreamWriter(file, false, Encoding.UTF8);

            var lineCounter = 0;
            while (!reader.EndOfStream) {
                var line = reader.ReadLine();

                if (line.StartsWith("\"155869") || line.StartsWith("\"1201304")) {
                }
                lineCounter++;
                //if (string.IsNullOrEmpty(line)) continue;
                //writer.Write(line[0]);

                //line = line.Substring(1);

                //var result = regex.Replace(line, "\"\"");
                var result = regex.Replace(line, "\"\"");
                writer.WriteLine(result);
            }

            reader.Close();
            writer.Close();

            File.Delete(PATH + @"\_" + fi.Name);
        }
    }
}