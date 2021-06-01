using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.ComponentModel;
using DbAccess;
using ViewBuilderBusiness.Persist;
using ProjectDb.Tables;
using SQLParser2.Model;
using DbAccess.Structures;
using ViewBuilderBusiness.Structures.Config;
using System.Linq;
using System.Data;

namespace DBComparisonBusiness.Business
{
    public class ViewScriptInfo
    {
        public string ViewScriptName { get; set; }
        public SQLObjectTree ObjectTree { get; set; }
        public SQLParserError Errors { get; set; }
        public Viewscript ViewScript { get; set; }
    }
    /// <summary>
    /// IComparisonResult implementation is not relevant (therefore empty) for the logic of viewscript - db comparison
    /// as a db should support comparison with many views not only another db.
    /// </summary>
    public class ViewScriptComparisonResult : DBComparisonBusiness.Business.IViewComparisonResult
    {
        #region Fields
        private List<ViewScriptInfo> _viewScriptInfos = new List<ViewScriptInfo>();
        #endregion

        #region Constructor
        public ViewScriptComparisonResult() { 
        }
        #endregion

        #region Properties
        public string DBName { get; set; }
        public string DBHostName { get; set; }
        public ComparisonResult.TableInfoCollection DBTableInfos { get; set; }
        //public IDatabase Connection { get; set; }
        public DbConfig Config { get; set; }

        public List<ViewScriptInfo> ViewScriptInfos
        {
            get { return _viewScriptInfos; }
            set { _viewScriptInfos = value; }
        }
        #endregion
        #region Methods
        Dictionary<string, List<string>> _missingTablesInDB;
        /// <summary>
        /// returns the tables from the cript that are missing in the DB, 
        /// table created by the create statement are ignored.
        /// </summary>
        /// <param name="script"></param>
        /// <returns></returns>
        public List<string> GetMissingTables(ViewScriptInfo script)
        {
            if (_missingTablesInDB == null)
            {
                _missingTablesInDB = new Dictionary<string, List<string>>();
            }
            if (!_missingTablesInDB.ContainsKey(script.ViewScriptName))
            {
                List<string> missingTables = new List<string>();
                //if (script != null && script.ObjectTree != null)
                //    missingTables = (from st in script.ObjectTree.Tables
                //                              where
                //                                  st.StatementType != SQLStatements.Create &&
                //                                  !DBTableInfos.Exists(t => t.Name.ToLower().Equals(st.Name.ToLower()))
                //                              select st.Name).ToList();
                if (script != null && script.ObjectTree != null) {
                    missingTables = script.ObjectTree.Tables.Where(
                        st =>
                        st.StatementType != SQLStatements.Create &&
                        //!DBTableInfos.Exists(t => t.Name.ToLower().Equals(st.Name.ToLower()))).Select(st => st.Name).ToList();
                        !DBTableInfos.Exists(t => CompareTableName(st.Name, t.Name))).Select(st => st.Name).ToList();
                }
                _missingTablesInDB.Add(script.ViewScriptName, missingTables);
            }
            return _missingTablesInDB[script.ViewScriptName];
        }

        /// <summary>
        /// Compare database table name to script table name (script table name can start with the char '_' which should be ignored)
        /// </summary>
        /// <param name="scriptTableName"></param>
        /// <param name="dbTableName"></param>
        /// <returns></returns>
        private bool CompareTableName(string scriptTableName, string dbTableName) {
            if (scriptTableName.ToLower() == dbTableName.ToLower()) return true;
            if (scriptTableName.StartsWith("_")) {
                if (scriptTableName.Substring(1).ToLower() == dbTableName.ToLower()) return true;
            }
            return false;
        }

        /// <summary>
        /// _missingColumnsInDB : per script, holds an array of tables where one
        /// is likelly to be the owner of a corresponding column in the value list. 
        /// </summary>
        Dictionary<string, Dictionary<List<string>, List<string>>> _missingColumnsInDB;
        List<ComparisonResult.ColumnInfo> _allDBColumns;
        /// <summary>
        /// returns a dictionnary of tables with columns 
        /// where a tables list is the owner of a columns list 
        /// (as we cannot be sure what table owns what column in the view script).
        /// </summary>
        /// <param name="script"></param>
        /// <returns></returns>
        public Dictionary<List<string>, List<string>> GetMissingColumns(ViewScriptInfo script)
        {
            //if (_missingColumnsInDB == null)
            //{
            //    _missingColumnsInDB = new Dictionary<string, Dictionary<List<string>, List<string>>>();
            //    _allDBColumns = DBTableInfos.SelectMany(t => t.Columns).ToList();
            //}
            //if (!_missingColumnsInDB.ContainsKey(script.ViewScriptName))
            //{
            //    //_missingColumnsInDB.Add(script.ViewScriptName,
            //    //    new Dictionary<List<string>,List<string>>());
            //    //if (script.ObjectTree != null)
            //    //    foreach (var table in script.ObjectTree.Tables.Where(t => t.StatementType != SQLStatements.Create)) 
            //    //    { 
            //    //        //_missingColumnsInDB[script.ViewScriptName].Add(
            //    //        //    new []{ table.Name }.ToList(),
            //    //        //(from sc in table.Columns 
            //    //        //    where !sc.IsIndexColumn && !_allDBColumns.Exists(c=> c.Name.ToLower().Equals(sc.Name.ToLower()))
            //    //        //     select sc.Name).ToList());
            //    //    }

            //    _missingColumnsInDB.Add(script.ViewScriptName, new Dictionary<List<string>, List<string>>());
            //    if (script.ObjectTree != null)
            //        foreach (DBTable table in script.ObjectTree.Tables.Where(t => t.StatementType != SQLStatements.Create))
            //        {
            //            //_missingColumnsInDB[script.ViewScriptName].Add(
            //            //    new[] { table.Name }.ToList(),
            //            //    table.Columns.Where(sc => !sc.IsIndexColumn && !_allDBColumns.Exists(c => c.Name.ToLower().Equals(sc.Name.ToLower()))).Select(sc => sc.Name).ToList()
            //            // );

            //            // DEVNOTE: use the non temp table if exists
            //            DBTable tableToCheck = table;
            //            if (tableToCheck.Name.StartsWith("_")) {
            //                DBTable tmp = script.ObjectTree.Tables.FirstOrDefault(t => CompareTableName(table.Name, t.Name) && !t.Name.StartsWith("_"));
            //                if (tmp != null)
            //                    tableToCheck = tmp;
            //            }
            //            _missingColumnsInDB[script.ViewScriptName].Add(
            //                new[] { tableToCheck.Name }.ToList(),
            //                tableToCheck.Columns.Where(sc => !sc.IsIndexColumn && !_allDBColumns.Exists(c => c.Name.ToLower().Equals(sc.Name.ToLower()))).Select(sc => sc.Name).ToList()
            //             );
            //        }
            //}
            //return _missingColumnsInDB[script.ViewScriptName];
            if (_missingColumnsInDB == null)
            {
                _missingColumnsInDB = new Dictionary<string, Dictionary<List<string>, List<string>>>();
            }
            if (!_missingColumnsInDB.ContainsKey(script.ViewScriptName)) {

                _missingColumnsInDB.Add(script.ViewScriptName, new Dictionary<List<string>, List<string>>());
                if (script.ObjectTree != null) {
                    using (IDatabase conn = ConnectionManager.CreateDbFromConfig(Config)) {
                        conn.Open();
                        foreach (
                            DBTable table in script.ObjectTree.Tables.Where(t => t.StatementType != SQLStatements.Create)) {

                            // DEVNOTE: use the non temp table if exists
                            //DBTable tableToCheck = table;
                            //if (tableToCheck.Name.StartsWith("_")) {
                            //    DBTable tmp = script.ObjectTree.Tables.FirstOrDefault( t => CompareTableName(table.Name, t.Name) && !t.Name.StartsWith("_"));
                            //    if (tmp != null) tableToCheck = tmp;
                            //}
                            //IEnumerable<ComparisonResult.ColumnInfo> allTableColumns = DBTableInfos.Where(t => CompareTableName(table.Name, t.Name)).SelectMany(t => t.Columns).ToList();
                            //_missingColumnsInDB[script.ViewScriptName].Add(
                            //    new[] { table.Name.StartsWith("_") ? table.Name.Substring(1) : table.Name }.ToList(),
                            //        table.Columns.Where(sc => !sc.IsIndexColumn && !allTableColumns.Any(c => c.Name.ToLower().Equals(sc.Name.ToLower()))).Select(sc => sc.Name).ToList()
                            //    );

                            string tableName = table.Name.StartsWith("_") ? table.Name.Substring(1) : table.Name;
                            List<string> missingTableColumns = new List<string>();

                            IEnumerable<ComparisonResult.TableInfo> allTables = DBTableInfos.Where(t => CompareTableName(table.Name, t.Name)).ToList();
                            ComparisonResult.TableInfo tableToCheck = allTables.FirstOrDefault(t => t.Name.ToLower() == tableName.ToLower());

                            if (tableToCheck != null && tableToCheck.Columns.Count == 0) {
                                GetColumns(conn, tableToCheck);
                            }

                            if (tableToCheck != null) {
                                missingTableColumns = table.Columns.Where(sc => !sc.IsIndexColumn && !tableToCheck.Columns.Any(c => c.Name.ToLower() == sc.Name.ToLower())).Select(sc => sc.Name).ToList();
                            } else {
                                missingTableColumns = table.Columns.Where(sc => !sc.IsIndexColumn).Select(sc => sc.Name).ToList();
                            }
                            _missingColumnsInDB[script.ViewScriptName].Add(new[] {tableName}.ToList(), missingTableColumns);
                        }
                    }
                }
            }
            return _missingColumnsInDB[script.ViewScriptName];
        }

        public void GetColumns(IDatabase conn, ComparisonResult.TableInfo table)
        {
            using (IDataReader reader = conn.ExecuteReader("SELECT * FROM " + conn.Enquote(table.Name) + " LIMIT 1"))
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    ComparisonResult.ColumnInfo colInfo = new ComparisonResult.ColumnInfo(reader.GetName(i), table.Name, reader.GetDataTypeName(i));
                    table.Columns.Add(colInfo);
                }
            }
        }
        #endregion
    }
    
    public class ViewScriptComparer
    {
        private ViewScriptComparisonResult _result;
        private IDatabase _db1;
        private BackgroundWorker _bgWorker;
        private List<SQLObjectTree> _objectTreeScripts = new List<SQLObjectTree>();
        private List<Viewscript> _viewScripts;

        #region Constructor
        public ViewScriptComparer(IDatabase db1, string[] viewScriptsPath, BackgroundWorker bgWorker, ProfileConfig profile = null)
        {
            this._db1 = db1;

            _result = new ViewScriptComparisonResult()
            {
                DBHostName = _db1.DbConfig.Hostname,
                DBName = _db1.DbConfig.DbName
            };

            if (profile == null)
            {
                profile = new ProfileConfig();
                profile.ProjectDb = new ProjectDb.ProjectDb();
                profile.ProjectDb.Init(new DbConfig("MySQL", null)
                {
                    Hostname = _db1.DbConfig.Hostname,
                    DbName = _db1.DbConfig.DbName,
                });
            }

            // loops through all scripts , build the sqltree for each of them + fill the View co;parison result with it.
            foreach (string scriptPath in viewScriptsPath) 
            {
                _viewScripts = ViewscriptParser.Parse(new System.IO.FileInfo(scriptPath), profile, true);

                foreach (Viewscript viewScript in _viewScripts) {
                    // DEVNOTE: guduparser cannot handle DELIMITER $$ ... END $$
                    viewScript.ViewInfo.CompleteStatement = viewScript.ViewInfo.CompleteStatement.Replace("DELIMITER $$", "").Replace("delimiter $$", "").Replace("END $$", "END").Replace("end $$", "end");
                    // DEVNOTE: guduparser cannot handle Truncate command
                    while (viewScript.ViewInfo.CompleteStatement.ToLower().Contains("truncate ")) {
                        int indexTruncate = viewScript.ViewInfo.CompleteStatement.ToLower().IndexOf("truncate ");
                        int indexSemicolon = viewScript.ViewInfo.CompleteStatement.ToLower().IndexOf(";", indexTruncate);
                        viewScript.ViewInfo.CompleteStatement = viewScript.ViewInfo.CompleteStatement.Remove(indexTruncate, indexSemicolon - indexTruncate + 1);
                    }
                    SQLParser2.SQLParser sqlParser = SQLParser2.SQLParser.CreateFromStatements(viewScript.ViewInfo.CompleteStatement);
                    _result.ViewScriptInfos.Add(new ViewScriptInfo { ViewScriptName = viewScript.Name, ObjectTree = sqlParser.SQLObjectTree, Errors = sqlParser.Error, ViewScript = viewScript });
                }
            }

            this._bgWorker = bgWorker;
        }

        public ViewScriptComparisonResult GetCreateResult()
        {
            ComparisonResult.TableInfoCollection tables = new ComparisonResult.TableInfoCollection();
            _result.DBTableInfos = tables;
            //_result.Connection = _db1.DbConfig;
            _result.Config = (DbConfig)_db1.DbConfig.Clone();
            //_result.MissingTablesDict
            // creates the tableinfos of the unique db (similar code as DatabaseComparer)
            IDatabaseInformation dbInfo;
            if (_db1.DatabaseExists(_db1.DbConfig.DbName + "_system"))
                dbInfo = new DatabaseSystemExists(_db1);
            else
                dbInfo = new DatabaseNoSystem(_db1);

            dbInfo.GetTables(tables);

            return _result;
        }

        public List<string> GetScriptsVithoutObjectTree() { return _result.ViewScriptInfos.Where(v => v.ObjectTree == null).Select(v => v.ViewScriptName).ToList(); }

        #endregion
    }
}
