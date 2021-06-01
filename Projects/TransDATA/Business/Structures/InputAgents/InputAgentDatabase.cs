// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-11
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Data;
using AV.Log;
using Business.Structures.DateReaders;
using Config.Interfaces.DbStructure;
using System.Collections.Generic;
using DbAccess.Structures;
using DbAccess;
using Config.Interfaces.Config;
using System.Linq;
using GenericOdbc;
using log4net;
using IDataReader = Business.Interfaces.IDataReader;

namespace Business.Structures.InputAgents
{
    internal class InputAgentDatabase : InputAgentBase
    {
        internal ILog _log = LogHelper.GetLogger();

        public InputAgentDatabase(IInputConfig config)
            : base(config)
        {

        }

        public override IDataReader GetDataReader(ITransferEntity entity, bool useAdo = false) {
            _log.ContextLog(LogLevelEnum.Debug, "useAdo: {0}", useAdo?"yes":"no");

            if (!(entity is ITable)) {
                _log.ContextLog(LogLevelEnum.Error, "Invalid entity type. Entity is not ITable");
            }
            System.Diagnostics.Debug.Assert(entity is ITable, "Invalid entity type");



            var conn =
                ConnectionManager.CreateConnection(
                    ((IDatabaseInputConfig)Config.Config).UseAdo || useAdo ? "ADO" : "GenericODBC",
                    ((IDatabaseInputConfig)Config.Config).ConnectionString,
                    DbTemplateManager.GetTemplate(
                        ((IDatabaseInputConfig)Config.Config).DbTemplateName));
            //var conn =
            //   ConnectionManager.CreateConnection(
            //       "MySQL",
            //       ((IDatabaseInputConfig)Config.Config).ConnectionString.Substring(((IDatabaseInputConfig)Config.Config).ConnectionString.IndexOf(";")),
            //       DbTemplateManager.GetTemplate(
            //           ((IDatabaseInputConfig)Config.Config).DbTemplateName));
            conn.Open();
            _log.ContextLog(LogLevelEnum.Debug, "IsOpen: {0}", conn.IsOpen ? "yes" : "no");
            return new TableDataReader(conn, (ITable)entity, DatabaseConfig.UseSchema, DatabaseConfig.UseCatalog);
        }
        IDatabaseInputConfig DatabaseConfig { get { return Config.Config as IDatabaseInputConfig; } }

        public override DataTable GetPreview(ITransferEntity entity, long count)
        {
            _log.ContextLog(LogLevelEnum.Debug, "count: {0}", count);

            if (!(entity is ITable))
            {
                _log.ContextLog(LogLevelEnum.Error, "Invalid entity type. Entity is not ITable");
            }
            System.Diagnostics.Debug.Assert(entity is ITable, "Invalid entity type");


            using (var conn = ConnectionManager.CreateConnection(DatabaseConfig.UseAdo ? "ADO" : "GenericODBC",
                                                                 DatabaseConfig.ConnectionString,
                                                                 DbTemplateManager.GetTemplate(
                                                                     (DatabaseConfig).
                                                                         DbTemplateName))) {
                conn.Open();
                _log.ContextLog(LogLevelEnum.Debug, "IsOpen: {0}", conn.IsOpen ? "yes" : "no");
                TableDataReader reader = new TableDataReader(conn, (ITable)entity, DatabaseConfig.UseSchema,
                                                             DatabaseConfig.UseCatalog);


                _log.ContextLog(LogLevelEnum.Debug, "reader.Load()");
                reader.Load(false);


                DataTable table = new DataTable(entity.Name);
                foreach (var column in entity.Columns) {
                    if (!column.DoExport) continue;
                    table.Columns.Add(column.Name);
                }

                for (int i = 0; i < count; i++) {
                    if (!reader.Read())
                        break;
                    table.Rows.Add(reader.GetData());
                }
                reader.Close();
                return table;
            }
        }

        public override IEnumerable<Tuple<DbTableInfo, long>> GetTableInfos(IDatabase conn, bool doCount = true, bool addColumns = true)
        {
            _log.ContextLog(LogLevelEnum.Debug, "doCount: {0} addColumns: {1}", doCount ? "yes" : "no", addColumns ? "yes" : "no");

            try
            {
                var result = new List<Tuple<DbTableInfo, long>>();
                var tables = conn.GetTableInfos();
                foreach (var table in tables)
                {
                    if (addColumns)
                        conn.AddColumnInfos(table);
                    long count = 0;
                    if (doCount)
                        count = conn.CountTable(table.Name);

                    _log.ContextLog(LogLevelEnum.Debug, "table: {0} count: {1}", table, count);
                    result.Add(new Tuple<DbTableInfo, long>(table, count));
                }
                return result;
            }
            catch (Exception ex) {
                string errorMessage = string.Format(Base.Localisation.ExceptionMessages.CouldNotImportTableStructure,
                                                    ex.Message);

                _log.ContextLog(LogLevelEnum.Error, errorMessage);
                throw new Exception(errorMessage, ex);
            }
        }

        public override long GetCount(ITransferEntity entity) {
            _log.ContextLog(LogLevelEnum.Debug, "");

            if (!(entity is ITable))
            {
                _log.ContextLog(LogLevelEnum.Error, "Invalid entity type. Entity is not ITable");
            }
            System.Diagnostics.Debug.Assert(entity is ITable, "Invalid entity type");

            ITable table = entity as ITable;
            try {
                using (
                    var conn =
                        ConnectionManager.CreateConnection(
                            DatabaseConfig.UseAdo ? "ADO" : "GenericODBC",
                            DatabaseConfig.
                                ConnectionString,
                            DbTemplateManager.GetTemplate(
                                DatabaseConfig.
                                    DbTemplateName))) {
                    conn.Open();
                    return
                        (conn.CountTableWithFilter(DatabaseConfig.UseSchema ? table.Schema : string.Empty,
                                                   DatabaseConfig.UseCatalog ? table.Catalog : string.Empty, table.Name,
                                                   table.Filter));
                }
            } catch (Exception ex) {
                string errorMessage = string.Format(Base.Localisation.ExceptionMessages.CouldNotCountTable, ex.Message);
                _log.ContextLog(LogLevelEnum.Error, errorMessage);
                throw new Exception(errorMessage, ex);
            }
        }

        public override bool CheckDataAccess() {
            _log.ContextLog(LogLevelEnum.Debug,"");
            try
            {
                using (
                    var conn =
                        ConnectionManager.CreateConnection(
                            DatabaseConfig.UseAdo ? "ADO" : "GenericODBC",
                            DatabaseConfig.
                                ConnectionString,
                            DbTemplateManager.GetTemplate(
                                DatabaseConfig.
                                    DbTemplateName))) 
                //var conn =
                //        ConnectionManager.CreateConnection(
                //            "MySQL",
                //            DatabaseConfig.ConnectionString.Substring(DatabaseConfig.ConnectionString.IndexOf(";")),
                //            DbTemplateManager.GetTemplate(
                //                DatabaseConfig.
                //                    DbTemplateName))) 
                                    {
                    conn.Open();

                    _log.ContextLog(LogLevelEnum.Info, "Database accessible. Connectionstring: {0}", DatabaseConfig.ConnectionString);

                    conn.Close();
                }
                return true;
            } catch (Exception ex) {
                _log.ContextLog(LogLevelEnum.Info, "Can not connect. Connectionstring: {0} Error: {1}",
                                DatabaseConfig.ConnectionString, ex.Message);
                return false;
            }
        }

        public override string GetDescription() {
            return "Datenbank" + Environment.NewLine;
        }
    }
}