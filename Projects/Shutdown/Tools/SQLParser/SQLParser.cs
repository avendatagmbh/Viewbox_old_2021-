using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gudusoft.gsqlparser;
using SQLParser2.Model;
using System.Text.RegularExpressions;

namespace SQLParser2
{
    public class SQLParser
    {
        #region nested class
        /// <summary>
        /// Provides the TGSqlParser class as a singleton.
        /// </summary>
        private class SQLNativeParser 
        {
            internal static TGSqlParser _mysqlParser = new  TGSqlParser(TDbVendor.DbVMysql);
        }
        
        private class TableInfo
    {
        public TCustomSqlStatement stmt;
        public TSourceToken database, schema, table, tableAlias;

        public string getDatabaseName()
        {
            if (database == null)
            {
                return "";
            }
            else
            {
                return database.AsText;
            }
        }

        public string getSchemaName()
        {
            if (schema == null)
            {
                return "";
            }
            else
            {
                return schema.AsText;
            }
        }

        public string getTableName()
        {
            if (table == null)
            {
                return "";
            }
            else
            {
                return table.AsText;
            }
        }

        public string getTableAliasName()
        {
            if (tableAlias == null)
            {
                return "";
            }
            else
            {
                return tableAlias.AsText;
            }
        }

        public TableInfo(TCustomSqlStatement s)
        {
            stmt = s;
        }
    }
        private class ColumnInfo
        {
            public TableInfo table;
            public TSourceToken column;
            public string columnAlias;
            //public int lineNo, columnNo;
            public TLzOwnerLocation location;
            public TLzExpression columnExpr;
            public ColumnInfo(TableInfo t)
            {
                table = new TableInfo(t.stmt);
                table.database = t.database;
                table.schema = t.schema;
                table.table = t.table;
                table.tableAlias = t.tableAlias;
            }
        }
        #endregion

        #region fields
        string _sqlScript;
        SQLObjectTree _sqlObjectTree;
        SQLParserError _error;
        Dictionary<string, string> _tempTableNames = new Dictionary<string, string>();
        #endregion

        private SQLParser(string sqlScript) 
        {
            _sqlScript = PrepareSqlForParser(sqlScript);
            NativeParser.SqlText.Text = _sqlScript;

            InitObjectTree();
            UpdateObjectTreeTempTableNames();
        }

        private string PrepareSqlForParser(string sqlScript)
        {

            return GetScriptWithTempTableNames(RemoveDelimiters(sqlScript));
        }
        /// <summary>
        /// carryon here: remode delimiter in sql in procedure
        /// see sql script sample : 2sap_ust_voranmeldung
        /// </summary>
        /// <param name="sqlScript"></param>
        /// <returns></returns>
        private string RemoveDelimiters(string sqlScript)
        {
            //StringBuilder sb = null;
            //Match m = Regex.Match(sqlScript, @"^\s*DELIMITER", RegexOptions.IgnoreCase);
            //if (m != null && m.Success) 
            //{
            //    sb = new StringBuilder(sqlScript);
            //    sb= sb.Replace(m.Value, "", m.Index, m.Value.Length);
            //}
            //if (sb != null)
            //{
            //    while ((m = Regex.Match(sb.ToString(), "\D+", )
            //    {

            //    }
            //}
            return sqlScript;
        }

        /// <summary>
        /// Change the object tree with the original table name stored in _tablesWithNumberPrefix
        /// </summary>
        private void UpdateObjectTreeTempTableNames()
        {
            if(_sqlObjectTree != null && _sqlObjectTree.Tables.Count >0)
            {
                foreach (var tempTableName in _tempTableNames.Keys)
                {
                    if (_sqlObjectTree.Tables.Exists(tempTableName))
                        _sqlObjectTree.Tables[tempTableName].Name = _tempTableNames[tempTableName];
                }
                // removes tablename that starts with $ (those in create stmts but unused in select)
                DBTable tempTable=null;
                while ((tempTable = _sqlObjectTree.Tables.SingleOrDefault(t => t.Name.StartsWith("$"))) != null) 
                {
                    _sqlObjectTree.Tables.Remove(tempTable);
                }
            }
        }
        /// <summary>
        /// Change tablenames who starts with numbers+underscore to $tableName
        /// as the parser doent accept it. 
        /// Keeps track of changes in the _tempTableNames dictionary to roll back to previous state.
        /// </summary>
        /// <param name="sqlScript"></param>
        /// <returns></returns>
        private string GetScriptWithTempTableNames(string sqlScript)
        {
            StringBuilder sb = new StringBuilder(sqlScript);
            Match match = null;

            while (((match = Regex.Match(sb.ToString(), @"\s\d{1,2}[\w\S]*[A-z]+[\w\S]*", RegexOptions.None)) != null 
                && match.Success)) 
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

        #region instance members
        /// <summary>
        /// initialize the sql tree from the parser result
        /// check third party sample for examples using the code.
        /// Currently the tree being built doesnt parse all statments (insert, update), check TCustomSqlStatement condition to change this.
        /// </summary>
        private void InitObjectTree()
        {
            int result = NativeParser.Parse();

            Error = new SQLParserError();
            if (!string.IsNullOrEmpty(NativeParser.ErrorMessages)) {
                Error.ErrorMessage = "SQLParser error: " + NativeParser.ErrorMessages;
            }
            Error.CompleteStatement = NativeParser.SqlText.Text;
            foreach (var error in NativeParser.SyntaxErrors)
            {
                if (!string.IsNullOrEmpty(error.Token))
                    Error.SyntaxErrors.Add(String.Format("{0} X:{1} Y:{2} ", error.Token, error.XPosition, error.YPosition));
            }

            // DEVNTE: despite of errors the object tree should be built because for example the gudu parser cannot recognize TRUNCATE commands
            //if (result == 0)
            //{
                _sqlObjectTree = new SQLObjectTree();
                for (int y = 0; y < NativeParser.SqlStatements.Count(); y++)
                {
                    TCustomSqlStatement sql = NativeParser.SqlStatements[y];

                    BuildSQLTree(sql,0);

                    for (int j = 0; j < sql.ChildNodes.Count(); j++)
                    {
                        if (sql.ChildNodes[j] is TCustomSqlStatement)
                        {
                            BuildSQLTree(sql.ChildNodes[j] as TCustomSqlStatement, 0);
                        }
                        else if(sql.ChildNodes[j] is TLzConstraint)
                        {
                            // if we are inside of an index constraint of a create statement
                            // makes sure we specify the index column name as such , not a concrete table column name.
                            if (sql is TCreateTableSqlStatement) 
                            {
                                var indexColumnList = (sql.ChildNodes[j] as TLzConstraint).ColumnList;
                                if (indexColumnList != null) {
                                    for (var i = 0; i < indexColumnList.Count(); i++) {
                                        var indexColumn = (indexColumnList[i] as TLz_Attr);

                                        if (indexColumn != null) {
                                            _sqlObjectTree.AddColumn(
                                                new DBColumn {Name = indexColumn.AsText, IsIndexColumn = true},
                                                _sqlObjectTree.Tables.Last());
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            //}
            //else // sets error
            //{
            //    Error = new SQLParserError();
            //    Error.ErrorMessage = "SQLParser error: " + NativeParser.ErrorMessages;
            //    Error.CompleteStatement = NativeParser.SqlText.Text;
            //    foreach (var error in NativeParser.SyntaxErrors)
            //    {
            //        if (!string.IsNullOrEmpty(error.Token))
            //            Error.SyntaxErrors.Add(String.Format("{0} X:{1} Y:{2} ", error.Token, error.XPosition, error.YPosition));
            //    }
            //}

        }
        private void BuildSQLTree(TCustomSqlStatement sql, int level)
        {
            TableInfo tableInfo = new TableInfo(sql);
            TSourceTokenList tokenList = sql.TableTokens;


            TSourceToken st = null;
            string tablestr = null;
            string tablealias = null;

            for (int i = 0; i < tokenList.Count(); i++)
            {
                st = tokenList[i];
                tableInfo.table = st;
                tablestr = st.AsText;
                if (st.RelatedToken != null)
                {
                    tablealias = "alias: " + st.RelatedToken.AsText;
                    tableInfo.tableAlias = st.RelatedToken;
                }
                if (st.ParentToken != null)
                {
                    //schema
                    tablestr = st.ParentToken.AsText + "." + tablestr;
                    tableInfo.schema = st.ParentToken;

                    if (st.ParentToken.ParentToken != null)
                    {
                        //database
                        tablestr = st.ParentToken.ParentToken.AsText + "." + tablestr;
                        tableInfo.database = st.ParentToken.ParentToken;
                    }
                }

                DBTable currentTable = new DBTable { Name = st.AsText };
                if (sql is TCreateTableSqlStatement)
                {
                    currentTable.StatementType =  SQLStatements.Create;
                }
                this.SQLObjectTree.Tables.Add(currentTable);

                if (st.RelatedToken != null)
                {
                    // declared table alias token
                    TSourceToken rt = st.RelatedToken;
                    TSourceToken rrt = null;
                    for (int j = 0; j < rt.RelatedTokens.Count(); j++)
                    {
                        rrt = rt.RelatedTokens[j];
                        if (rrt.ChildToken != null)
                        {
                            ColumnInfo columnInfo = new ColumnInfo(tableInfo);
                            columnInfo.column = rrt.ChildToken;

                            this.SQLObjectTree.AddColumn(new DBColumn { Name = columnInfo.column.AsText }, currentTable);
                        }
                    }
                }

                TSourceToken rtt = null;
                for (int j = 0; j < st.RelatedTokens.Count(); j++)
                {
                    // reference table token
                    rtt = st.RelatedTokens[j];
                    if (rtt.DBObjType == TDBObjType.ttObjField)
                    {
                        // get all field tokens link with table token (those token not linked by syntax like tablename.fieldname)
                        // but like this : select f from t

                        ColumnInfo columnInfo = new ColumnInfo(tableInfo);
                        columnInfo.column = rtt;
                        this.SQLObjectTree.AddColumn(new DBColumn { Name = columnInfo.column.AsText }, currentTable);

                    }
                    if (rtt.ChildToken != null)
                    {
                        ColumnInfo columnInfo = new ColumnInfo(tableInfo);
                        columnInfo.column = rtt.ChildToken;

                        this.SQLObjectTree.AddColumn(new DBColumn { Name = columnInfo.column.AsText }, currentTable);
                    }
                }
            }
        }

        public SQLObjectTree SQLObjectTree
        {
            get { return _sqlObjectTree; }
            set { _sqlObjectTree = value; }
        }
        public SQLParserError Error
        {
            get { return _error; }
            private set { _error = value; }
        } 
        #endregion

        #region class members
        public static SQLParser CreateFromStatements(string sqlStatements){
            return new SQLParser(sqlStatements);
        }
        /// <summary>
        /// Singleton access to the parser.
        /// </summary>
        private static TGSqlParser NativeParser 
        {
            get { return SQLNativeParser._mysqlParser; }
        }
        #endregion
    }
}
