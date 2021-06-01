using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AV.Log;
using Business.Interfaces;
using Config.DbStructure;
using Config.Interfaces.Config;
using Config.Interfaces.DbStructure;
using DbAccess;
using DbAccess.Structures;
using GenericOdbc;
using System.IO;
using log4net;

namespace Business.Structures.MetadataAgents {
    internal class MetadataAgentDatabase : IMetadataAgent
    {
        private ILog _log = LogHelper.GetLogger();

        #region Constructor

        internal MetadataAgentDatabase(IDatabaseInputConfig config, int taskCount)
        {
            InputConfig = config;
            this.taskCount = taskCount;
        }

        #endregion Constructor

        #region Properties

        private int taskCount;
        private IDatabaseInputConfig InputConfig { get; set; }

        #endregion Properties

        #region Methods

        public void AddTables(IInputAgent importAgent, List<DbTableInfo> tables,
                              IImportDbStructureProgress importProgress) {
            if (string.IsNullOrEmpty(InputConfig.TableWhitelist)) {

                var dbList = new List<string>();
                if (!string.IsNullOrEmpty(InputConfig.DatabaseWhitelist)) {
                    using (var reader = new StreamReader(InputConfig.DatabaseWhitelist)) {
                        while (!reader.EndOfStream) {
                            dbList.Add(reader.ReadLine().ToLower());
                        }
                    }
                }

                using (var conn = ConnectionManager.CreateConnection(InputConfig.UseAdo ? "ADO" : "GenericODBC",
                                                                     InputConfig.ConnectionString,
                                                                     DbTemplateManager.GetTemplate(
                                                                         InputConfig.
                                                                             DbTemplateName))) {
                    conn.Open();
                    var tableInfos = importAgent.GetTableInfos(conn, false, false);

                    int count = 0;
                    foreach (var tableInfo in tableInfos) {
                        count++;

                        if (InputConfig.ProcessTables &&
                            (string.IsNullOrEmpty(tableInfo.Item1.Type) ||
                             tableInfo.Item1.Type.Equals("TABLE", StringComparison.InvariantCultureIgnoreCase) ||
                             tableInfo.Item1.Type.Equals("PASS-THROUGH", StringComparison.InvariantCultureIgnoreCase))) {
                            if (string.IsNullOrEmpty(InputConfig.DatabaseWhitelist) ||
                                dbList.Contains(tableInfo.Item1.Catalog.ToLower()) ||
                                dbList.Contains(tableInfo.Item1.Schema.ToLower())) {
                                tables.Add(tableInfo.Item1);
                            }
                        } else if (InputConfig.ProcessViews && !string.IsNullOrEmpty(tableInfo.Item1.Type) &&
                                   (tableInfo.Item1.Type.Equals("VIEW", StringComparison.InvariantCultureIgnoreCase) ||
                                    tableInfo.Item1.Type.Equals("SYNONYM", StringComparison.InvariantCultureIgnoreCase))) {
                            if (string.IsNullOrEmpty(InputConfig.DatabaseWhitelist) ||
                                dbList.Contains(tableInfo.Item1.Catalog.ToLower()) ||
                                dbList.Contains(tableInfo.Item1.Schema.ToLower())) {
                                tables.Add(tableInfo.Item1);
                            }
                        } else {
                            System.Diagnostics.Debug.WriteLine(tableInfo.Item1.Type);
                        }
                    }
                }
            } else {
                var reader = new StreamReader(InputConfig.TableWhitelist);
                while (!reader.EndOfStream) {
                    var table = reader.ReadLine();
                    tables.Add(new DbTableInfo(string.Empty, string.Empty, table, "TABLE", string.Empty,
                                               string.Empty));
                }
            }
        }

        public void ImportTable(DbTableInfo table, IProfile profile, IImportDbStructureProgress importProgress)
        {
            _log.ContextLog(LogLevelEnum.Debug, "");

            var t = profile.CreateTable();

            //t.Count = tableInfo.Item2;                
            t.Catalog = table.Catalog;
            t.Schema = table.Schema;
            t.Name = table.Name;
            t.Type = table.Type;
            t.Comment = table.Remarks;
            t.DoExport = true;

            lock (importProgress)
            {
                importProgress.AddProcessedTable(t);
            }
            
            var _isAdo = InputConfig.UseAdo ? "ADO" : "GenericODBC";
            var template=DbTemplateManager.GetTemplate(InputConfig.DbTemplateName);

            using (var conn = ConnectionManager.CreateConnection(_isAdo,InputConfig.ConnectionString,template))
            {
                conn.Open();
                conn.AddColumnInfos(table);
                t.Count = conn.CountTableWithFilter(InputConfig.UseSchema ? table.Schema : string.Empty,
                                                    InputConfig.UseCatalog ? table.Catalog : string.Empty, table.Name,
                                                    string.Empty);
            }

            foreach (var column in table.Columns) {
                bool alreadyFound = false;
                foreach(var col in t.Columns) {
                    if (col.Name.Equals(column.Name, StringComparison.InvariantCultureIgnoreCase)) alreadyFound = true;
                }
                if (alreadyFound) continue;
                var c = t.CreateColumn();
                c.Name = column.Name;
                c.Comment = column.Comment;
                c.MaxLength = column.MaxLength;
                c.NumericScale = column.NumericScale;
                c.OrdinalPosition = column.OrdinalPosition;
                c.DoExport = true;

                c.DbType = column.Type;
                c.DefaultValue = column.DefaultValue;
                c.AllowDBNull = column.AllowDBNull;
                c.AutoIncrement = column.AutoIncrement;
                c.NumericScale = column.NumericScale;
                c.IsPrimaryKey = column.IsPrimaryKey;
                c.IsUnsigned = column.IsUnsigned;
                c.IsIdentity = column.IsIdentity;
                c.OrdinalPosition = column.OrdinalPosition;

                c.TypeName = column.OriginalType;
                switch (column.Type)
                {
                    case DbColumnTypes.DbInt:
                    case DbColumnTypes.DbBigInt:
                    case DbColumnTypes.DbNumeric:
                        c.Type = ColumnTypes.Numeric;
                        break;

                    case DbColumnTypes.DbBool:
                        c.Type = ColumnTypes.Bool;
                        break;

                    case DbColumnTypes.DbText:
                    case DbColumnTypes.DbLongText:
                        c.Type = ColumnTypes.Text;
                        break;

                    case DbColumnTypes.DbDate:
                        c.Type = ColumnTypes.Date;
                        break;

                    case DbColumnTypes.DbTime:
                        c.Type = ColumnTypes.Time;
                        break;

                    case DbColumnTypes.DbDateTime:
                        c.Type = ColumnTypes.DateTime;
                        break;

                    case DbColumnTypes.DbBinary:
                        c.Type = ColumnTypes.Binary;
                        break;

                    case DbColumnTypes.DbUnknown:
                        c.Type = ColumnTypes.Text;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
                t.AddColumn(c);
            }
            lock (profile)
            {
                profile.AddTable(t);
            }

            lock (importProgress)
            {
                importProgress.RemoveProcessedTable(t);
            }
        }

        public void Cleanup() {
            //ConnManager.Dispose();            
        }
        #endregion Methods
    }
}
