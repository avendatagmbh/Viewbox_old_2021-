using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SystemDb;
using SystemDb.Internal;
using Utils;
using ViewBuilderBusiness.Manager;
using ViewBuilderBusiness.Persist;
using ViewBuilderBusiness.Structures.Config;
using ProjectDb.Tables;
using gudusoft.gsqlparser;
using DbAccess;
using ViewBuilder.Structures;
using log4net;
using AV.Log;
using System.Windows;

namespace ViewBuilder.Models
{

    public class ExtendedColumnInformationModel : NotifyPropertyChangedBase
    {
        #region Properties

        private ILog _logger = LogHelper.GetLogger();
        public object Owner;


        public string FilePath
        {
            get { return Settings.CurrentProfileConfig.ScriptSource.ExtendedColumnInformationDirectory; }
            set
            {
                Settings.CurrentProfileConfig.ScriptSource.ExtendedColumnInformationDirectory = value;
                ProfileManager.Save(Settings.CurrentProfileConfig);
                OnPropertyChanged("FilePath");
            }
        }

        public ObservableCollection<ExtScript> Items { get; set; }

        public static List<ExtendedColumnInformationCsvItem> Informations = new List<ExtendedColumnInformationCsvItem>();

        #endregion Properties


        public ExtendedColumnInformationModel()
        {
            Items = new ObservableCollection<ExtScript>();


        }

        public void Refresh()
        {
            RefreshInformations();
            Items.Clear();
            DirectoryInfo di = new DirectoryInfo(FilePath);

            foreach (FileInfo fileInfo in di.GetFiles())
            {
                List<Viewscript> _viewScripts = ViewscriptParser.Parse(fileInfo, Settings.CurrentProfileConfig, true);
                foreach (Viewscript viewScript in _viewScripts)
                {
                    ExtScript item = new ExtScript()
                    {
                        FileName = fileInfo.FullName,
                        Script = viewScript
                    };
                    item.Parse();
                    Items.Add(item);
                    OnPropertyChanged("Items");
                }
            }
            MessageBox.Show("Ready");
        }

        public void NextFile()
        {
            Items.Clear();
            OnPropertyChanged("Items");
            DirectoryInfo di = new DirectoryInfo(FilePath);
            if (di.Parent == null) return;
            foreach (FileInfo fileInfo in di.GetFiles("*.sql"))
            {
                fileInfo.CopyTo(di.Parent.FullName + "\\FinishedScripts\\" + fileInfo.Name);
                fileInfo.Delete();
            }
            var file = new DirectoryInfo(di.Parent.FullName + "\\Scripts\\").GetFiles("*.sql").OrderBy(w => w.Name).FirstOrDefault();
            if (file != null)
            {
                file.CopyTo(di + "\\" + file.Name);
                file.Delete();
                MessageBox.Show("Next file ready - " + file.Name);
                return;
            }
            MessageBox.Show("Next file ready - no more files");
        }

        public void CheckExtInfos()
        {
            using (IDatabase conn = Settings.CurrentProfileConfig.ViewboxDb.ConnectionManager.GetConnection())
            {
                List<ExtendedColumnInformation> extColumnInformations = conn.DbMapping.Load<ExtendedColumnInformation>();

                List<TableObject> tables = new List<TableObject>();
                tables.AddRange(conn.DbMapping.Load<Table>("type = " + (int)TableType.Table));
                tables.AddRange(conn.DbMapping.Load<SystemDb.Internal.View>("type = " + (int)TableType.View));
                tables.AddRange(conn.DbMapping.Load<Issue>("type = " + (int)TableType.Issue));
                List<Column> columns = conn.DbMapping.Load<Column>();
                List<string> delete = new List<string>();
                foreach (var info in extColumnInformations)
                {
                    var sourceColumn = columns.FirstOrDefault(w => w.Id == info.ParentColumnId);
                    var targetColumn = columns.FirstOrDefault(w => w.Id == info.ChildColumnId);
                    var informationColumn = columns.FirstOrDefault(w => w.Id == info.InformationColumnId);
                    var informationColumn2 = columns.FirstOrDefault(w => w.Id == info.InformationColumn2Id);

                    if (sourceColumn == null || targetColumn == null || informationColumn == null)
                    {
                        _logger.Info("Column not found - info.id=" + info.Id);
                        return;
                    }

                    var sourceTable = tables.FirstOrDefault(w => w.Id == sourceColumn.TableId);
                    var targetTable = tables.FirstOrDefault(w => w.Id == targetColumn.TableId);

                    if (sourceTable == null || targetTable == null)
                    {
                        _logger.Info("Table not found - info.id=" + info.Id);
                        return;
                    }

                    string infoStr = "id=" + info.Id + "source=" + sourceTable.TableName + "." + sourceColumn.Name
                        + " target=" + targetTable.TableName + "." + targetColumn.Name
                        + " info=" + targetTable.TableName + "." + informationColumn.Name
                        + (informationColumn2 == null ? "" : " info2=" + targetTable.TableName + "." + informationColumn2.Name);

                    _logger.Info("------------------------------");
                    _logger.Info(infoStr);

                    bool error;

                    var res = ExtScript.GetResult(conn, "", "SELECT " + conn.Enquote(sourceColumn.Name)
                                                            + " FROM " +
                                                            conn.Enquote(sourceTable.Database, sourceTable.TableName)
                                                            + " WHERE " + conn.Enquote(sourceColumn.Name) +
                                                            " IS NOT NULL AND " + conn.Enquote(sourceColumn.Name) +
                                                            " <> ''"
                                                            + " LIMIT 10");

                    if (res.HasResult)
                    {
                        foreach (var result in res.Result)
                        {
                            var res2 = ExtScript.GetResult(conn, "", "SELECT " + conn.Enquote(targetColumn.Name)
                                                                     + " FROM " +
                                                                     conn.Enquote(targetTable.Database,
                                                                                  targetTable.TableName)
                                                                     + " WHERE " + conn.Enquote(targetColumn.Name) + " = '" + result[0] + "'"
                                                                     + " AND " + conn.Enquote(informationColumn.Name) + " IS NOT NULL AND "
                                                                     + conn.Enquote(informationColumn.Name) + " <> ''"
                                                                     + " LIMIT 10");
                            if (!res2.HasResult)
                            {
                                _logger.Info("    No result for where select " + info.Id);
                                if (!res2.Error)
                                    delete.Add("DELETE FROM extended_column_information WHERE ID = " + info.Id + ";");
                            }
                        }
                    }
                    else
                    {
                        _logger.Info("    No result for base select " + info.Id);
                        if (!res.Error)
                            delete.Add("DELETE FROM extended_column_information WHERE ID = " + info.Id + ";");
                    }
                
                }
                _logger.Info("delete statements");
                foreach (string dele in delete)
                {
                    _logger.Info(dele);
                }
            }
        }

        public bool HasResult(IDatabase conn, string sql, bool log, out bool error)
        {
            bool ret = false;
            error = false;
            try
            {
                using (var reader = conn.ExecuteReader(sql))
                {
                    while (reader.Read())
                    {
                        string lg = "    ";
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            if (reader.IsDBNull(i))
                                lg += "NULL|";
                            else
                                lg += reader.GetValue(i).ToString() + "|";
                        }
                        if (log)
                            _logger.Info(lg);
                        if (!reader.IsDBNull(0) && !String.IsNullOrEmpty(reader.GetValue(0).ToString()))
                            ret = true;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("    ", ex);
                error = true;
                return false;
            }
            return ret;
        }

        public void Generate()
        {
            RefreshInformations();



            using (IDatabase conn = Settings.CurrentProfileConfig.ViewboxDb.ConnectionManager.GetConnection())
            {
                using (IDatabase dataconn = Settings.CurrentProfileConfig.ConnectionManager.GetConnection())
                {
                    List<ExtendedColumnInformation> extColumnInformations =
                        conn.DbMapping.Load<ExtendedColumnInformation>();

                    List<TableObject> tables = new List<TableObject>();
                    tables.AddRange(conn.DbMapping.Load<Table>("type = " + (int)TableType.Table));
                    tables.AddRange(conn.DbMapping.Load<SystemDb.Internal.View>("type = " + (int)TableType.View));
                    tables.AddRange(conn.DbMapping.Load<Issue>("type = " + (int)TableType.Issue));

                    List<Column> columns = conn.DbMapping.Load<Column>();

                    foreach (var item in Informations)
                    {
                        foreach (
                            var sourceTable in tables.Where(w => w.TableName.ToUpper() == item.SourceTable.ToUpper()))
                        {
                            var targetTable =
                                tables.FirstOrDefault(w => w.TableName.ToUpper() == item.TargetTable.ToUpper() && w.Database.ToUpper() == sourceTable.Database.ToUpper());

                            if (targetTable == null)
                                continue;

                            var sourceColumn =
                                GetColumn(item.SourceColumn, sourceTable.TableName, sourceTable.Id, columns, conn,
                                          dataconn);

                            var targetColumn =
                                GetColumn(item.TargetColumn, targetTable.TableName, targetTable.Id, columns, conn,
                                          dataconn);

                            var informationColumn =
                                GetColumn(item.Information, targetTable.TableName, targetTable.Id, columns, conn,
                                          dataconn);

                            var informationColumn2 =
                                String.IsNullOrEmpty(item.Information2)
                                    ? null
                                    : GetColumn(item.Information2, targetTable.TableName, targetTable.Id, columns, conn,
                                                dataconn);

                            if (sourceColumn == null || targetColumn == null || informationColumn == null)
                                continue;

                            if (!String.IsNullOrEmpty(item.Information2) && informationColumn2 == null)
                                continue;

                            if (
                                extColumnInformations.Any(
                                    w => w.ParentColumnId == sourceColumn.Id && w.ChildColumnId == targetColumn.Id))
                                continue;

                            var extendedInformation = new ExtendedColumnInformation()
                                                          {
                                                              ParentColumnId = sourceColumn.Id,
                                                              ChildColumnId = targetColumn.Id,
                                                              InformationColumnId = informationColumn.Id,
                                                              InformationColumn2Id =
                                                                  informationColumn2 == null ? 0 : informationColumn2.Id,
                                                              RelationType = item.Type
                                                          };
                            extColumnInformations.Add(extendedInformation);
                            conn.DbMapping.Save(extendedInformation);
                        }
                    }


                    DirectoryInfo di = new DirectoryInfo(FilePath);
                    foreach (FileInfo fileInfo in di.GetFiles())
                    {
                        try
                        {
                            List<Viewscript> _viewScripts = ViewscriptParser.Parse(fileInfo,
                                                                                   Settings.CurrentProfileConfig,
                                                                                   true);
                            foreach (Viewscript viewScript in _viewScripts)
                            {
                                ExtScript item = new ExtScript()
                                                     {
                                                         FileName = fileInfo.FullName,
                                                         Script = viewScript
                                                     };
                                item.Parse(true);

                                var table =
                                    item.Tables.Values.FirstOrDefault(
                                        w => w.Name.ToUpper() == item.Script.Name.ToUpper());
                                if (table == null)
                                    continue;
                                foreach (var col in table.ScriptColumns.Values.Where(w => w.Found != null && String.IsNullOrEmpty(w.HasInTable)))
                                {
                                    var it = col.Found;
                                    foreach (var sourceTable in
                                        tables.Where(w => w.TableName.ToUpper() == table.Name.ToUpper()))
                                    {
                                        var targetTable =
                                            tables.FirstOrDefault(w => w.TableName.ToUpper() == it.TargetTable.ToUpper() && w.Database.ToUpper() == sourceTable.Database.ToUpper());

                                        if (targetTable == null)
                                            continue;

                                        var sourceColumn =
                                            GetColumn(col.Name, sourceTable.TableName, sourceTable.Id, columns, conn,
                                                      dataconn);

                                        var targetColumn =
                                            GetColumn(it.TargetColumn, targetTable.TableName, targetTable.Id, columns,
                                                      conn,
                                                      dataconn);

                                        var informationColumn =
                                            GetColumn(it.Information, targetTable.TableName, targetTable.Id, columns,
                                                      conn,
                                                      dataconn);

                                        var informationColumn2 =
                                            String.IsNullOrEmpty(it.Information2)
                                                ? null
                                                : GetColumn(it.Information2, targetTable.TableName, targetTable.Id,
                                                            columns, conn,
                                                            dataconn);

                                        if (sourceColumn == null || targetColumn == null || informationColumn == null)
                                            continue;

                                        if (!String.IsNullOrEmpty(it.Information2) && informationColumn2 == null)
                                            continue;

                                        if (
                                            extColumnInformations.Any(
                                                w =>
                                                w.ParentColumnId == sourceColumn.Id &&
                                                w.ChildColumnId == targetColumn.Id))
                                            continue;

                                        var extInfo = new ExtendedColumnInformation()
                                                          {
                                                              ParentColumnId = sourceColumn.Id,
                                                              ChildColumnId = targetColumn.Id,
                                                              InformationColumnId = informationColumn.Id,
                                                              InformationColumn2Id =
                                                                  informationColumn2 == null ? 0 : informationColumn2.Id,
                                                              RelationType = it.Type
                                                          };
                                        extColumnInformations.Add(extInfo);
                                        conn.DbMapping.Save(extInfo);
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }

            MessageBox.Show("Generate ready");
        }

        public Column GetColumn(string columnName, string tableName, int tableId, List<Column> columns, IDatabase viewboxConn, IDatabase dataConn)
        {
            columnName = columnName.ToUpper();
            tableName = tableName.ToUpper();
            var col = columns.FirstOrDefault(w => w.Name.ToUpper() == columnName.ToUpper() && w.TableId == tableId);
            if (col == null)
            {
                var columnInfo = dataConn.GetColumnInfos(tableName).FirstOrDefault(w => w.Name.ToUpper() == columnName);
                if (columnInfo != null)
                {
                    int id = columns.Max(w => w.Id) + 1;
                    var tableColumns = columns.Where(w => w.TableId == tableId).ToList();
                    var ordinal = tableColumns.Any() ? tableColumns.Max(w => w.Ordinal) + 1 : 1;

                    col = new Column()
                              {
                                  TableId = tableId,
                                  Name = columnName,
                                  IsVisible = true,
                                  Id = id,
                                  Ordinal = ordinal,
                                  IsEmpty = false,
                                  UserDefined = false
                              };

                    switch (columnInfo.Type)
                    {
                        case DbAccess.Structures.DbColumnTypes.DbInt:
                        case DbAccess.Structures.DbColumnTypes.DbBool:
                        case DbAccess.Structures.DbColumnTypes.DbBigInt:
                            col.DataType = SqlType.Integer;
                            break;

                        case DbAccess.Structures.DbColumnTypes.DbNumeric:
                            col.MaxLength = columnInfo.NumericScale;
                            col.DataType = SqlType.Decimal;
                            break;

                        case DbAccess.Structures.DbColumnTypes.DbDate:
                            col.DataType = SqlType.Date;
                            break;

                        case DbAccess.Structures.DbColumnTypes.DbTime:
                            col.DataType = SqlType.Time;
                            break;

                        case DbAccess.Structures.DbColumnTypes.DbDateTime:
                            col.DataType = SqlType.DateTime;
                            break;

                        case DbAccess.Structures.DbColumnTypes.DbText:
                        case DbAccess.Structures.DbColumnTypes.DbBinary:
                        case DbAccess.Structures.DbColumnTypes.DbLongText:
                        case DbAccess.Structures.DbColumnTypes.DbUnknown:
                            col.MaxLength = columnInfo.MaxLength;
                            col.DataType = SqlType.String;
                            break;
                    }
                    columns.Add(col);
                    viewboxConn.DbMapping.Save(col);
                }
            }
            return col;

        }

        public void RefreshInformations()
        {
            string dir = Settings.CurrentProfileConfig.ScriptSource.ExtendedColumnInformationDirectory;
            if (!dir.EndsWith("\\"))
                dir += "\\";

            var csvReader = new CsvReader(dir + "ExtendedInformations.csv") { Separator = ';' };
            var dataTable = csvReader.GetCsvData(0, Encoding.UTF8);
            Informations.Clear();
            foreach (DataRow dataRow in dataTable.Rows)
            {
                var extinf = new ExtendedColumnInformationCsvItem()
                                 {
                                     SourceTable = dataRow["Source table"].ToString(),
                                     SourceColumn = dataRow["Source column"].ToString(),
                                     TargetTable = dataRow["Target table"].ToString(),
                                     TargetColumn = dataRow["Target column"].ToString(),
                                     Information = dataRow["Information"].ToString(),
                                     Information2 =
                                         dataRow["Information 2"] == null ? "" : dataRow["Information 2"].ToString(),
                                     Type = dataRow["Type"] == null ? "" : dataRow["Type"].ToString()
                                 };
                if (extinf.SourceTable != extinf.TargetTable)
                    Informations.Add(extinf);
            }
        }
    }

    public class ExtScript
    {
        #region Properties

        public string FileName { get; set; }
        public Viewscript Script { get; set; }

        public Dictionary<string, ExtTable> Tables { get; set; }
        public List<ExtTable> TableValues
        {
            get
            {
                return Tables.Values.ToList();
            }
        }

        private ObservableCollection<ExtColumn> _columns;
        public ObservableCollection<ExtColumn> Columns
        {
            get
            {
                var ret = new ObservableCollection<ExtColumn>();

                foreach (var table in Tables.Values)
                {
                    foreach (var item in table.ScriptColumns.Values.OrderByDescending(h => h.Results.Count(w => w.HasResult)).ToList())
                        ret.Add(item);
                }
                return ret;
            }
            set { _columns = value; }
        }

        private readonly Dictionary<string, string> _tempTableNames = new Dictionary<string, string>();

        private static TSourceTokenList lcTableTokens = new TSourceTokenList(false);
        private static TSourceTokenList lcFieldTokens = new TSourceTokenList(false);

        #endregion Properties

        public void Parse(bool withoutCheckTable = false)
        {
            Tables = new Dictionary<string, ExtTable>();

            TGSqlParser mysqlParser = new TGSqlParser(TDbVendor.DbVMysql);

            mysqlParser.SqlText.Text = GetScriptWithTempTableNames(RemoveTruncate(RemoveDelimiters(Script.ViewInfo.ProcedureCreateTempTables)));
            int result = mysqlParser.Parse();
            for (int i = 0; i < mysqlParser.SqlStatements.Count(); i++)
            {
                var tempProcData = (mysqlParser.SqlStatements[i] as TMySQLCreateProcedure);
                if (tempProcData != null)
                {
                    for (int j = 0; j < tempProcData.SqlStatements.Count(); j++)
                    {
                        ParseStatements(tempProcData.SqlStatements[j]);
                    }
                }
                else
                    ParseStatements(mysqlParser.SqlStatements[i]);

            }

            mysqlParser.SqlText.Text = GetScriptWithTempTableNames(RemoveTruncate(RemoveDelimiters(Script.ViewInfo.CompleteStatement)));
            result = mysqlParser.Parse();
            for (int i = 0; i < mysqlParser.SqlStatements.Count(); i++)
            {
                var tempProcData = (mysqlParser.SqlStatements[i] as TMySQLCreateProcedure);
                if (tempProcData != null)
                {
                    for (int j = 0; j < tempProcData.SqlStatements.Count(); j++)
                    {
                        ParseStatements(tempProcData.SqlStatements[j]);
                    }
                }
                else
                    ParseStatements(mysqlParser.SqlStatements[i]);

            }

            foreach (var table in Tables.Values)
            {
                foreach (var column in table.ScriptColumns.Values)
                {
                    GetRealInfoForColumn(table, column);
                }
            }

            using (IDatabase conn = Settings.CurrentProfileConfig.ConnectionManager.GetConnection())
            {
                foreach (var table in Tables.Values)
                {
                    foreach (var column in table.ScriptColumns.Values)
                    {
                        if (String.IsNullOrEmpty(column.RealName) || String.IsNullOrEmpty(column.RealTable))
                            continue;

                        SelectAndResult res = GetResult(conn, "from " + column.RealTable,
                                                        "select " + column.RealName + " FROM " + column.RealTable +
                                                        (column.RealTable.ToUpper() == "BSEG" || column.RealTable.ToUpper() == "MSEG" || column.RealTable.ToUpper() == "BKPF"
                                                        ? "" : " WHERE " + column.RealName + " IS NOT NULL AND " + column.RealName + " <> ''") +
                                                        " LIMIT 5");
                        column.Results.AddItem(res);

                        if (!res.Result.Any() && column.RealTable.StartsWith("_"))
                        {
                            res = GetResult(conn, "from " + column.RealTable.Substring(1),
                                            "select " + column.RealName + " FROM " +
                                            column.RealTable.Substring(1) +
                                            (column.RealTable.Substring(1).ToUpper() == "BSEG" || column.RealTable.Substring(1).ToUpper() == "MSEG" || column.RealTable.Substring(1).ToUpper() == "BKPF"
                                            ? "" : " WHERE " + column.RealName + " IS NOT NULL AND " + column.RealName + " <> ''") +
                                            " LIMIT 5");
                            if (res.Result.Any())
                            {
                                column.RealTable = column.RealTable.Substring(1);
                            }
                            column.Results.AddItem(res);
                        }

                        column.Found =
                            ExtendedColumnInformationModel.Informations.FirstOrDefault(w => w.SourceTable == column.RealTable && w.SourceColumn == column.RealName);

                        if (withoutCheckTable)
                            continue;

                        res = GetResult(conn, " from dd031 c=" + column.RealName,
                                                    " select checktable, MAX(IF(tabname = \"" + column.RealTable + "\", 1, 0)), count(tabname) " +
                                                    " from dd03l " +
                                                    " where fieldname = \"" + column.RealName + "\"" +
                                                    "   and checktable <> \"\" and checktable <> \"*\"" +
                                                    " group by checktable " +
                                                    " order by 2 desc, 3 desc LIMIT 5 ");

                        column.Results.AddItem(res);

                        foreach (var allFieldResult in res.Result)
                        {
                            var checkTable = allFieldResult.FirstOrDefault();

                            if (!String.IsNullOrEmpty(checkTable))
                            {
                                checkTable = checkTable.Trim();
                            }

                            if (String.IsNullOrEmpty(checkTable))
                                continue;
                            column.Results.AddItem(GetResult(conn, " from " + checkTable,
                                                    "select * FROM " + checkTable + " LIMIT 10", column.RealName), true);

                            column.Results.AddItem(GetResult(conn, " from " + checkTable + "t",
                                                        "select * FROM " + checkTable + "t LIMIT 10", column.RealName), true);

                            column.Results.AddItem(GetResult(conn, " from " + checkTable.Substring(0, checkTable.Length - 1) + "t",
                                                        "select * FROM " + checkTable.Substring(0, checkTable.Length - 1) + "t LIMIT 10", column.RealName), true);
                        }

                    }
                }

                foreach (var table in Tables.Values)
                {
                    foreach (var column in table.ScriptColumns.Values.Where(w=>w.Found != null))
                    {
                        if (table.ScriptColumns.Any(w=>w.Value.RealTable == column.Found.TargetTable && (
                            w.Value.RealName == column.Found.Information || w.Value.RealName == column.Found.Information2
                            )))
                        {
                            column.HasInTable = "yes";
                        }
                    }
                }
            }
        }

        private void ParseStatements(TCustomSqlStatement stmt)
        {
            var tempData = (stmt as TCreateTableSqlStatement);
            if (tempData != null && !String.IsNullOrEmpty(tempData.Table.DisplayName))
            {
                ParseStatement(tempData.SelectStmt, tempData.Table.DisplayName);
            }

            var tempViewData = (stmt as TCreateViewSqlStatement);
            if (tempViewData != null && !String.IsNullOrEmpty(tempViewData.ViewName))
            {
                ParseStatement(tempViewData.SubQuery, tempViewData.ViewName);
            }

            var tempInsertData = (stmt as TInsertSqlStatement);
            if (tempInsertData != null && tempInsertData.Table != null && !String.IsNullOrEmpty(tempInsertData.Table.DisplayName))
            {
                ParseStatement(tempInsertData.subquery, tempInsertData.Table.DisplayName);
            }
        }

        public static SelectAndResult GetResult(IDatabase conn, string header, string select, string columnToFind = "")
        {
            var ret = new SelectAndResult(columnToFind) { Header = header, Select = select };
            if (String.IsNullOrEmpty(select))
                return ret;
            try
            {
                using (var reader = conn.ExecuteReader(select))
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        ret.Columns.Add(reader.GetName(i));
                    }
                    while (reader.Read())
                    {
                        var values = new List<string>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            values.Add((reader.IsDBNull(i) ? "NULL" : reader.GetValue(i).ToString()));
                        }
                        ret.Result.Add(values);
                    }
                }
            }
            catch (Exception ex)
            {
                ret.ErrorString = ex.ToString();
                ret.Error = true;
            }
            return ret;
        }

        private void GetRealInfoForColumn(ExtTable table, ExtColumn column)
        {
            var tab = table.ScriptTables.Values.FirstOrDefault(w => w.Alias == column.Alias);
            if (tab == null)
            {
                return;
            }
            column.TableName = tab.Name;

            column.RealName = "?";
            column.RealTable = "?";

            if (Tables.ContainsKey(tab.Name.ToUpper()))
            {
                var extTable = Tables[tab.Name];

                var extColumn = extTable.ScriptColumns.Values.FirstOrDefault(w => w.Name.Split('.').Last().ToUpper() == column.ColumnName.ToUpper());
                if (extColumn != null)
                {
                    column.RealName = extColumn.RealName;
                    column.RealTable = extColumn.RealTable;
                }

                return;
            }

            column.RealName = column.ColumnName;
            column.RealTable = column.TableName;
        }

        public void ParseStatement(TSelectSqlStatement selectstmt, string statName)
        {
            if (selectstmt == null || Tables.ContainsKey(statName.ToUpper())) return;

            ExtTable table = new ExtTable()
                                 {
                                     Name = statName.ToUpper()
                                 };
            if (table.Name.StartsWith("$"))
                table.Name = table.Name.Substring(1);
            Tables.Add(table.Name, table);


            for (int j = 0; j < selectstmt.Fields.Count(); j++)
            {
                ExtColumn column = new ExtColumn()
                                           {
                                               Name = selectstmt.Fields[j].DisplayName.ToUpper(),
                                               Alias = selectstmt.Fields[j].FieldPrefix.ToUpper(),
                                               ColumnName = selectstmt.Fields[j].FieldName.ToUpper()
                                           };
                table.ScriptColumns.Add(column.Name, column);

                #region Get Alias from expression
                if (String.IsNullOrEmpty(column.Alias))
                {
                    if (selectstmt.Fields[j].FieldExpr != null)
                    {
                        if (selectstmt.Fields[j].FieldExpr.oper == TLzOpType.Expr_FuncCall)
                        {
                            TLz_FuncCall func = (TLz_FuncCall)selectstmt.Fields[j].FieldExpr.lexpr;
                            if (func == null || func.args == null) continue;
                            for (int k = 0; k < func.args.Count(); k++)
                            {
                                if (func.args[k] is TSourceToken)
                                {
                                    TSourceToken ts = (TSourceToken)func.args[k];
                                }
                                else if (func.args[k] is TLz_CastArg)
                                {
                                    TLz_CastArg castArg = (TLz_CastArg)func.args[k];
                                    var dataArray = (castArg._ndexpr.AsText).Split('.');
                                    if (dataArray.Length == 2)
                                    {
                                        column.Alias = dataArray[0];
                                        column.ColumnName = dataArray[1];
                                    }
                                }
                                else
                                {
                                    lcTableTokens.Clear();
                                    lcFieldTokens.Clear();
                                    GetDbObjectsFromNode((TLz_Node)(func.args[k]));

                                    if (lcFieldTokens.Count() == 1 && lcTableTokens.Count() == 1)
                                    {
                                        column.Alias = lcTableTokens[0].AsText.ToUpper();
                                        column.ColumnName = lcFieldTokens[0].AsText.ToUpper();
                                    }
                                    else
                                        if (lcFieldTokens.Count() == 1 && lcTableTokens.Count() == 0)
                                        {
                                            column.Alias = "";
                                            column.ColumnName = lcFieldTokens[0].AsText.ToUpper();
                                        }
                                }
                            }
                        }
                    }
                }
                #endregion Get Alias from expression
            }

            for (int j = 0; j < selectstmt.Tables.Count(); j++)
            {
                ExtTable fromTable = new ExtTable();
                fromTable.Name = selectstmt.Tables[j].Name.ToUpper();
                fromTable.Alias = selectstmt.Tables[j].TableAlias.ToUpper();
                if (fromTable.Name.StartsWith("$"))
                    fromTable.Name = fromTable.Name.Substring(1);
                table.ScriptTables.Add(fromTable.Name + " " + fromTable.Alias, fromTable);
            }

            int i = 0;
            foreach (var tab in table.ScriptTables.Where(w => String.IsNullOrEmpty(w.Value.Alias)))
            {
                if (table.ScriptTables.All(w => w.Value.Alias != tab.Value.Name))
                {
                    tab.Value.Alias = tab.Value.Name;
                }
            }

            using (IDatabase conn = Settings.CurrentProfileConfig.ConnectionManager.GetConnection())
            {
                foreach (var column in table.ScriptColumns.Where(w => w.Value.ColumnName == "*").ToList())
                {
                    ExtTable tab = null;
                    if (String.IsNullOrEmpty(column.Value.Alias) && table.ScriptTables.Count == 1)
                    {
                        tab = table.ScriptTables.Values.FirstOrDefault();
                    }
                    else
                        if (!String.IsNullOrEmpty(column.Value.Alias))
                        {
                            tab = table.ScriptTables.Values.FirstOrDefault(w => w.Alias == column.Value.Alias);
                        }
                    if (tab != null)
                    {
                        string sql = "select * from " + tab.Name + " LIMIT 1; ";
                        try
                        {
                            using (var reader = conn.ExecuteReader(sql))
                            {
                                for (int j = 0; j < reader.FieldCount; j++)
                                {
                                    string name = reader.GetName(j);
                                    ExtColumn newColumn = new ExtColumn()
                                    {
                                        Name = tab.Alias + "." + name,
                                        Alias = tab.Alias,
                                        ColumnName = name
                                    };
                                    table.ScriptColumns.Add(newColumn.Name, newColumn);
                                }

                            }
                        }
                        catch (Exception)
                        {
                            if (tab.Name.StartsWith("_"))
                            {
                                sql = "select * from " + tab.Name.Substring(1) + " LIMIT 1; ";
                                try
                                {
                                    using (var reader = conn.ExecuteReader(sql))
                                    {
                                        for (int j = 0; j < reader.FieldCount; j++)
                                        {
                                            string name = reader.GetName(j);
                                            ExtColumn newColumn = new ExtColumn()
                                                                      {
                                                                          Name = tab.Alias + "." + name,
                                                                          Alias = tab.Alias,
                                                                          ColumnName = name
                                                                      };
                                            table.ScriptColumns.Add(newColumn.Name, newColumn);
                                        }

                                    }
                                }
                                catch (Exception)
                                {

                                }
                            }
                        }
                    }
                }

                foreach (var column in table.ScriptColumns.Where(w => String.IsNullOrEmpty(w.Value.Alias)))
                {
                    if (table.ScriptTables.Count == 1)
                        column.Value.Alias = table.ScriptTables.FirstOrDefault().Value.Alias.ToUpper();
                    else
                        foreach (var tab in table.ScriptTables)
                        {
                            string sql = "select " + column.Value.ColumnName + " from " + tab.Value.Name + " LIMIT 1; ";
                            try
                            {
                                conn.ExecuteScalar(sql);
                                column.Value.Alias = tab.Value.Alias.ToUpper();
                                break;
                            }
                            catch (Exception ex)
                            {

                            }
                            if (tab.Value.Name.StartsWith("_"))
                            {
                                string sql2 = "select " + column.Value.ColumnName + " from " + tab.Value.Name.Substring(1) + " LIMIT 1; ";
                                try
                                {
                                    conn.ExecuteScalar(sql2);
                                    column.Value.Alias = tab.Value.Alias.ToUpper();
                                    break;
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                        }
                }
            }
        }

        #region Helpers
        private string RemoveTruncate(string sqlScript)
        {
            StringBuilder sb = new StringBuilder(sqlScript);
            Match m = null;

            while ((m = Regex.Match(sb.ToString(), @"\s*TRUNCATE\s+TABLE.+?;", RegexOptions.Singleline)) != null
                && m.Success)
            {
                sb = sb.Replace(m.Value, "", m.Index, m.Length);
            }
            return sb.ToString();
        }

        private string RemoveDelimiters(string sqlScript)
        {
            if (String.IsNullOrEmpty(sqlScript))
                return "";
            StringBuilder sb = null;
            Match m = Regex.Match(sqlScript, @"^\s*DELIMITER\s*(?<sign>\S+)\s*", RegexOptions.Multiline);
            string delimiterSign = "";
            if (m != null && m.Success)
            {
                sb = new StringBuilder(sqlScript);
                sb = sb.Replace(m.Value, "", m.Index, m.Value.Length);
                delimiterSign = m.Groups["sign"].Value;
            }
            if (sb != null)
            {
                while ((m = Regex.Match(sb.ToString(), string.Join("", delimiterSign.Select(c => @"\" + c)))) != null && m.Success)
                {
                    sb = sb.Replace(delimiterSign, ";", m.Index, delimiterSign.Length);
                }
                return sb.ToString();
            }
            return sqlScript;
        }

        private string GetScriptWithTempTableNames(string sqlScript)
        {
            StringBuilder sb = new StringBuilder(sqlScript);
            Match match = null;

            while ((match = Regex.Match(sb.ToString(), @"\s\d{1,2}[\w\S]*[A-z]+[\w\S]*", RegexOptions.None)) != null
                && match.Success)
            {
                string tableName = match.Value.Trim();
                string tempTableName = "$" + tableName;
                if (!_tempTableNames.ContainsKey(tempTableName))
                {
                    _tempTableNames.Add(tempTableName, tableName);
                }
                sb = sb.Replace(tableName, tempTableName, match.Index, tempTableName.Length);
            }
            return sb.ToString();
        }

        public void GetDbObjectsFromNode(TLz_Node pNode)
        {
            //Fetch table and field objects into lcTableTokens,lcFieldTokens

            lcTableTokens.Clear();
            lcFieldTokens.Clear();

            TLzGetDbObjectsVisitor av = new TLzGetDbObjectsVisitor();
            av.ParseTree = pNode;
            av.TableTokens = lcTableTokens;
            av.FieldTokens = lcFieldTokens;
            av.Doit();

        }
        #endregion Helpers
    }

    public class ExtTable
    {
        public string Name { get; set; }
        public string Alias { get; set; }

        public Dictionary<string, ExtColumn> ScriptColumns = new Dictionary<string, ExtColumn>();
        public Dictionary<string, ExtTable> ScriptTables = new Dictionary<string, ExtTable>();

        public ExtTable Clone()
        {
            ExtTable result = new ExtTable();
            result.Name = Name;
            result.ScriptColumns = ScriptColumns;
            result.ScriptTables = ScriptTables;
            return result;
        }

        public List<ExtColumn> ScriptColumnValues
        {
            get
            {
                return ScriptColumns.Values.OrderByDescending(w => w.Results.Count).ToList();
            }
        }
    }

    public class ResultCollection : ObservableCollection<SelectAndResult>
    {
        public void AddItem(SelectAndResult res, bool withoutError = false)
        {
            if (withoutError && res.Error)
                return;
            res.Index = Count.ToString();
            Add(res);
        }
    }

    public class ExtColumn
    {
        public string Name { get; set; }
        public string Alias { get; set; }
        public string ColumnName { get; set; }
        public string TableName { get; set; }

        public string RealName { get; set; }
        public string RealTable { get; set; }
        public string HasInTable { get; set; }
        public ExtendedColumnInformationCsvItem Found { get; set; }
        public string FoundString { get { return Found == null ? "" : Found.ToString(); } }

        private ResultCollection _results = new ResultCollection()
                                                                     {
                                                                         new SelectAndResult()
                                                                             {Header = ".", Select = "."}
                                                                     };
        public ResultCollection Results { get { return _results; } set { _results = value; } }


    }

    public class SelectAndResult
    {
        public SelectAndResult(string columnToFind = "")
        {
            Columns = new List<string>();
            Result = new List<List<string>>();
            _columnToFind = columnToFind;
        }

        public string Index { get; set; }
        public string Header { get; set; }
        public string Select { get; set; }
        public string ErrorString { get; set; }
        public List<string> Columns { get; set; }
        public List<List<string>> Result { get; set; }
        private string _columnToFind;
        public bool Error { get; set; }
        public bool HasResult
        {
            get
            {
                if (Result.Count > 1)
                    return true;

                var result = Result.FirstOrDefault();
                if (result != null)
                {
                    foreach (var res in result)
                    {
                        if (!String.IsNullOrEmpty(res) && !String.IsNullOrEmpty(res.Trim()))
                            return true;
                    }
                }
                return false;
            }
        }
        public string FirstOrDefault()
        {
            var res = Result.FirstOrDefault();
            return res == null ? null : res.FirstOrDefault();
        }

        public string Content
        {
            get
            {
                if (Error)
                    return Select + Environment.NewLine + ErrorString;
                string res = "";
                int i = 0;
                foreach (var col in Columns)
                {
                    var found = (col.Equals(_columnToFind));
                    res += " ||" + (found ? "**" : "") + i + (found ? "**" : "") + "|| " + col;
                    i++;
                }
                res += Environment.NewLine;
                foreach (var row in Result)
                {
                    i = 0;
                    foreach (var data in row)
                    {
                        res += " ||" + i + "|| " + data;
                        i++;
                    }
                    res += Environment.NewLine;
                }
                return Select + Environment.NewLine + res;
            }
        }
    }

}
