using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using AV.Log;
using DbAccess.Enums;
using DbAccess.Structures;
using log4net;
using MySql.Data.MySqlClient;
using MySql.Data.Types;

namespace DbAccess
{
    public class DatabaseBase : IDisposable
    {
        protected IDbCommand _command;

        private IDbTransaction _transaction;

        private ILog _log = LogHelper.GetLogger();

        private int stmtCount;

        List<string> lstTables = new List<string>();
        private Dictionary<int, MySqlCommand> preparedCommands = new Dictionary<int, MySqlCommand>();

        private readonly ILog log = LogHelper.GetLogger();

        internal bool IsManaged { private get; set; }

        public DbMappingBase DbMapping { get; private set; }

        public IDbConnection Connection => _command.Connection;

        public DbConfig DbConfig { get; private set; }

        public string ConnectionString
        {
            get
            {
                return DbConfig.ConnectionString;
            }
            protected set
            {
                if (DbConfig.ConnectionString != value)
                {
                    DbConfig.ConnectionString = value;
                    _command.Connection.ConnectionString = value;
                }
            }
        }

        public char QuoteStart { get; protected set; }

        public char QuoteEnd { get; protected set; }

        public bool IsOpen => _command.Connection.State == ConnectionState.Open;

        protected int MaxSQLSize => 64000;

        protected int MaxSQLInsertLines => 999;

        protected int MaxSQLParams => 2000;

        public event EventHandler Disposed;

        protected DatabaseBase(DbConfig dbConfig, char quoteStart, char quoteEnd)
        {
            DbConfig = (DbConfig)dbConfig.Clone();
            QuoteStart = quoteStart;
            QuoteEnd = quoteEnd;
            DbConfig.PropertyChanged += DbConfig_PropertyChanged;
            InitConnection();
            Connection.ConnectionString = DbConfig.ConnectionString;
            DbMapping = new DbMappingBase(this);
        }

        private void OnDisposed()
        {
            if (this.Disposed != null)
            {
                this.Disposed(this, null);
            }
        }

        private void DbConfig_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ConnectionString")
            {
                _command.Connection.ConnectionString = DbConfig.ConnectionString;
            }
        }

        public void Open()
        {
            try
            {
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                //ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                _command.Connection.Open();
            }
            catch (Exception ex)
            {
                try
                {
                    MySqlConnection.ClearPool((MySqlConnection)_command.Connection);
                    _command.Connection.Open();
                }
                catch (Exception ex1)
                {
                    throw ex;
                }
            }
        }

        public void Close()
        {
            _command.Connection.Close();
        }

        public void CancelCommand()
        {
            _command.Cancel();
        }

        public void BeginTransaction(IsolationLevel isolationlevel = IsolationLevel.Serializable)
        {
            _transaction = _command.Connection.BeginTransaction(isolationlevel);
            _command.Transaction = _transaction;
        }

        public bool HasTransaction()
        {
            return _command.Transaction != null;
        }

        public virtual void CommitTransaction()
        {
            if (_transaction != null)
            {
                _transaction.Commit();
                _transaction = null;
                _command.Transaction = null;
                return;
            }
            throw new Exception("No active transaction.");
        }

        public virtual void RollbackTransaction()
        {
            if (_transaction != null)
            {
                _transaction.Rollback();
                _transaction = null;
                _command.Transaction = null;
                return;
            }
            throw new Exception("No active transaction.");
        }

        public IDataReader ExecuteReader(string sql)
        {
            return ExecuteReader(sql, CommandBehavior.Default);
        }

        public IDataReader ExecuteReader(string sql, CommandBehavior behavior, int connectNumber = 1)
        {
            lock (this)
            {
                if (_command.Connection.State == ConnectionState.Closed)
                {
                    Reconnect();
                }
                try
                {
                    if (_command.Connection.State != ConnectionState.Open)
                    {
                        _command.Connection.Open();
                    }
                    _command.CommandTimeout = 86400;
                    _command.CommandText = sql;
                    return _command.ExecuteReader();
                }
                catch (NullReferenceException)
                {
                    throw new OperationCanceledException();
                }
                catch (Exception ex)
                {
                    if (_command.Connection.State == ConnectionState.Closed)
                    {
                        return ExecuteReader(sql, behavior, connectNumber++);
                    }
                    string logline = $"{ex.Message},{sql}";
                    LogHelper.GetLogger().Error(logline);
                    if (IsOperationCanceledException(ex))
                    {
                        throw new OperationCanceledException();
                    }
                    throw new Exception(ex.Message + Environment.NewLine + sql, ex);
                }
            }
        }

        public int ExecuteNonQuery(string sql)
        {
            try
            {
                if (_command.Connection.State != ConnectionState.Open)
                {
                    _command.Connection.Open();
                }
                _command.CommandTimeout = 86400;
                _command.CommandText = sql;
                return _command.ExecuteNonQuery();
            }
            catch (NullReferenceException)
            {
                throw new OperationCanceledException();
            }
            catch (Exception ex)
            {
                string logline = $"{ex.Message},{sql}";
                LogHelper.GetLogger().Error(logline);
                ChangeDatabase(_command.Connection.Database);
                if (IsOperationCanceledException(ex))
                {
                    throw new OperationCanceledException();
                }
                throw new Exception(ex.Message + Environment.NewLine + sql, ex);
            }
        }

        public virtual object ExecuteScalar(string sql)
        {
            try
            {
                if (_command.Connection.State != ConnectionState.Open)
                {
                    _command.Connection.Open();
                }
                _command.CommandTimeout = 86400;
                _command.CommandText = sql;
                return _command.ExecuteScalar();
            }
            catch (NullReferenceException)
            {
                throw new OperationCanceledException();
            }
            catch (Exception ex)
            {
                string logline = $"{ex.Message},{sql}";
                LogHelper.GetLogger().Error(logline);
                ChangeDatabase(_command.Connection.Database);
                if (IsOperationCanceledException(ex))
                {
                    throw new OperationCanceledException();
                }
                throw new Exception(ex.Message + Environment.NewLine + sql, ex);
            }
        }

        public List<object> GetColumnValues(string sql)
        {
            List<object> values = new List<object>();
            using IDataReader reader = ExecuteReader(sql);
            while (reader.Read())
            {
                values.Add(reader[0]);
            }
            return values;
        }

        public List<string> GetColumnStringValues(string sql)
        {
            int counter = 4;
            Exception lastEx = new Exception();
            while (counter > 0)
            {
                try
                {
                    List<string> values = new List<string>();
                    using IDataReader reader = ExecuteReader(sql);
                    while (reader.Read())
                    {
                        values.Add(reader[0].ToString());
                    }
                    return values;
                }
                catch (Exception ex)
                {
                    lastEx = ex;
                    counter--;
                    Thread.Sleep(100);
                }
            }
            throw new Exception("Could not get schema list: " + lastEx.Message + "\n" + lastEx.InnerException.Message);
        }

        public virtual void ChangeDatabase(string dbName)
        {
            if (_command.Connection.State == ConnectionState.Open)
            {
                _command.Connection.ChangeDatabase(dbName);
            }
            DbConfig.DbName = dbName;
        }

        public virtual void CreateDatabase(string dbName)
        {
            try
            {
                ExecuteNonQuery("CREATE DATABASE " + Enquote(dbName));
            }
            catch (Exception ex)
            {
                throw new Exception("Could not create database " + Enquote(dbName) + ": " + ex.Message);
            }
        }

        public virtual void DropDatabase(string dbName)
        {
            try
            {
                ExecuteNonQuery("DROP DATABASE " + Enquote(dbName));
            }
            catch (Exception ex)
            {
                throw new Exception("Could not drop database " + Enquote(dbName) + ": " + ex.Message);
            }
        }

        public virtual void DropProcedureIfExists(string procedureName)
        {
            ExecuteNonQuery("DROP PROCEDURE IF EXISTS " + Enquote(DbConfig.DbName, procedureName));
        }

        public virtual void DropProcedureIfExists(string dbName, string procedureName)
        {
            ExecuteNonQuery("DROP PROCEDURE IF EXISTS " + Enquote(dbName, procedureName));
        }

        public virtual void DropTable(string tableName)
        {
            DropTable(DbConfig.DbName, tableName);
        }

        public virtual void DropTable(string dbName, string tableName)
        {
            try
            {
                ExecuteNonQuery("DROP TABLE " + Enquote(dbName, tableName));
            }
            catch (Exception ex)
            {
                throw new Exception("Could not drop table " + Enquote(dbName, tableName) + ": " + ex.Message);
            }
        }

        public virtual void DropTableIfExists(string tableName)
        {
            DropTableIfExists(DbConfig.DbName, tableName);
        }

        public virtual void ClearTable(string tableName)
        {
            ClearTable(DbConfig.DbName, tableName);
        }

        public virtual void DeleteHistory(string tableName, int userId, int issueId)
        {
            DeleteHistory(DbConfig.DbName, tableName, userId, issueId);
        }

        public virtual void DeleteHistory(string database, string tableName, int userId, int issueId)
        {
            try
            {
                string sql = "DELETE FROM " + Enquote(database, tableName) + " WHERE userId = " + userId + " AND issueId = " + issueId;
                ExecuteNonQuery(sql);
            }
            finally
            {
                _command.Parameters.Clear();
            }
        }

        public virtual void InsertInto(string tableName, DbColumnValues values)
        {
            InsertInto(DbConfig.DbName, tableName, values);
        }

        public virtual void InsertInto(string database, string tableName, DbColumnValues values)
        {
            try
            {
                string sql = "INSERT INTO " + Enquote(database, tableName) + " (" + GetSqlColumnString(values.Columns) + ") VALUES (" + GetSqlValueString(values) + ")";
                ExecuteNonQuery(sql);
            }
            finally
            {
                _command.Parameters.Clear();
            }
        }

        public virtual void InsertInto(string tableName, IEnumerable<DbColumnValues> values)
        {
            InsertInto(DbConfig.DbName, tableName, values);
        }

        public virtual void InsertInto(string database, string tableName, IEnumerable<DbColumnValues> valuesCollection)
        {
            try
            {
                int paramCount = 0;
                DbColumnValues[] dbColumnValueses = (valuesCollection as DbColumnValues[]) ?? valuesCollection.ToArray();
                if (!dbColumnValueses.Any())
                {
                    return;
                }
                string insertHeader = "INSERT INTO " + Enquote(database, tableName) + " (" + GetSqlColumnString(dbColumnValueses.First().Columns) + ") VALUES ";
                List<string> valueStrings = new List<string>();
                int i = 0;
                int curSize = insertHeader.Length;
                DbColumnValues[] array = dbColumnValueses;
                foreach (DbColumnValues values in array)
                {
                    string tmp = "(" + GetSqlValueString(values, i, ref paramCount) + ")";
                    if (valueStrings.Count > 0 && (curSize + tmp.Length > MaxSQLSize || valueStrings.Count >= MaxSQLInsertLines || paramCount > MaxSQLParams))
                    {
                        ExecuteNonQuery(insertHeader + string.Join(",", valueStrings));
                        valueStrings.Clear();
                        i = 0;
                        paramCount = 0;
                        curSize = insertHeader.Length;
                        _command.Parameters.Clear();
                        tmp = "(" + GetSqlValueString(values, i, ref paramCount) + ")";
                    }
                    valueStrings.Add(tmp);
                    curSize += tmp.Length;
                    i++;
                }
                if (valueStrings.Count > 0)
                {
                    ExecuteNonQuery(insertHeader + string.Join(",", valueStrings));
                }
            }
            finally
            {
                _command.Parameters.Clear();
            }
        }

        public virtual void Delete(string tableName, DbColumnValues values)
        {
            Delete(DbConfig.DbName, tableName, values);
        }

        public virtual void Delete(string dbName, string tableName, DbColumnValues valuesToDelete)
        {
            try
            {
                string condition = string.Empty;
                foreach (string fieldName in valuesToDelete.Columns)
                {
                    condition = condition + fieldName + "=" + GetValueString(valuesToDelete[fieldName]) + " AND ";
                }
                ExecuteNonQuery("DELETE FROM " + Enquote(dbName, tableName) + " WHERE " + condition.Remove(condition.Length - 5));
            }
            catch (Exception ex)
            {
                throw new Exception("Could not delete from " + Enquote(dbName, tableName) + ": " + ex.Message);
            }
        }

        public virtual void Delete(string tableName, string filter)
        {
            Delete(DbConfig.DbName, tableName, filter);
        }

        public virtual void Delete(string dbName, string tableName, string filter)
        {
            try
            {
                string sQl = "DELETE FROM " + Enquote(dbName, tableName) + (string.IsNullOrEmpty(filter) ? "" : (" WHERE " + filter));
                ExecuteNonQuery(sQl);
            }
            catch (Exception ex)
            {
                throw new Exception("Could not delete from " + Enquote(dbName, tableName) + ": " + ex.Message);
            }
        }

        public void ReplaceInto(string tableName, DbColumnValues values)
        {
            ReplaceInto(DbConfig.DbName, tableName, values);
        }

        public void ReplaceInto(string dbName, string tableName, DbColumnValues values)
        {
            string sSql = "REPLACE INTO " + Enquote(dbName, tableName) + " (" + GetSqlColumnString(values.Columns) + ") VALUES (" + GetSqlValueString(values) + ")";
            ExecuteNonQuery(sSql);
        }

        public virtual void Update(string tableName, DbColumnValues values, string filter)
        {
            Update(DbConfig.DbName, tableName, values, filter);
        }

        public virtual void Update(string dbName, string tableName, DbColumnValues values, string filter)
        {
            string sSql = "UPDATE " + Enquote(dbName, tableName) + " SET " + GetSqlUpdateString(values) + " WHERE " + filter;
            ExecuteNonQuery(sSql);
            _command.Parameters.Clear();
        }

        public virtual string Enquote(string name)
        {
            StringBuilder sb = new StringBuilder();
            if (!QuoteStart.Equals('\0'))
            {
                sb.Append(QuoteStart);
            }
            sb.Append(name);
            if (!QuoteEnd.Equals('\0'))
            {
                sb.Append(QuoteEnd);
            }
            return sb.ToString();
        }

        public virtual string Enquote(string dbName, string tableName)
        {
            return Enquote(dbName) + "." + Enquote(tableName);
        }

        public void Dispose()
        {
            if (Connection != null)
            {
                Connection.Close();
                Connection.Dispose();
                OnDisposed();
                GC.SuppressFinalize(Connection);
            }
        }

        public virtual bool TestConnection()
        {
            using (this)
            {
                try
                {
                    if (!IsOpen)
                    {
                        Open();
                    }
                    ExecuteScalar("SELECT 1");
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
                finally
                {
                    if (IsOpen)
                    {
                        Close();
                    }
                }
            }
        }

        public bool Reconnect()
        {
            try
            {
                if (_command.Connection.State == ConnectionState.Open)
                {
                    _command.Connection.Close();
                }
            }
            catch (Exception)
            {
            }
            try
            {
                _command.Connection.Dispose();
                _command.Dispose();
            }
            catch (Exception)
            {
            }
            try
            {
                _command.Dispose();
            }
            catch (Exception)
            {
            }
            try
            {
                _command = GetDbCommand();
                _command.CommandTimeout = 1814400;
                _command.Connection = GetDbConnection();
                _command.Connection.ConnectionString = ConnectionString;
                Open();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public DataTable GetDataTable(string sql)
        {
            DataTable oDataTable = new DataTable();
            try
            {
                IDbDataAdapter dbAdapter = GetDbAdapter();
                _command.CommandText = sql;
                dbAdapter.SelectCommand = _command;
                ((DbDataAdapter)dbAdapter).Fill(oDataTable);
                return oDataTable;
            }
            catch (Exception ex)
            {
                throw new Exception("Could not get data table: " + ex.Message);
            }
        }

        public DataTable GetDataTable(string sql, int limit)
        {
            DataTable oDataTable = new DataTable();
            try
            {
                IDbDataAdapter dbAdapter = GetDbAdapter();
                _command.CommandText = sql;
                dbAdapter.SelectCommand = _command;
                DataSet oDataSet = new DataSet();
                ((DbDataAdapter)dbAdapter).Fill(oDataSet, 0, limit, "t1");
                return oDataSet.Tables[1];
            }
            catch (Exception ex)
            {
                throw new Exception("Could not get data table: " + ex.Message);
            }
        }

        public string GetSqlString(string value)
        {
            return "'" + value + "'";
        }

        public virtual string GetSqlColumnString(IEnumerable<string> fields)
        {
            string sSqlFieldList = string.Empty;
            foreach (string sField in fields)
            {
                if (sSqlFieldList.Length > 0)
                {
                    sSqlFieldList += ",";
                }
                sSqlFieldList += Enquote(sField.Trim());
            }
            return sSqlFieldList;
        }

        public IDataReader GetReaderForTable(string tableName, string filter = "")
        {
            return GetReaderForTable(DbConfig.DbName, tableName, filter);
        }

        public IDataReader GetReaderForTable(string dbName, string tableName, string filter = "")
        {
            return ExecuteReader("SELECT " + GetSqlColumnString(GetColumnNames(dbName, tableName)) + " FROM " + Enquote(dbName, tableName) + ((filter.Length > 0) ? (" WHERE " + filter) : ""));
        }

        public IDataReader GetTextReaderForTable(string tableName, List<string> columnNames, string filter = "")
        {
            return GetTextReaderForTable(DbConfig.DbName, tableName, columnNames, filter);
        }

        public IDataReader GetTextReaderForTable(string dbName, string tableName, List<string> columnNames, string filter = "")
        {
            return ExecuteReader("SELECT " + GetSqlColumnString(GetColumnNames(dbName, tableName)) + " FROM " + Enquote(dbName, tableName) + ((filter.Length > 0) ? (" WHERE " + filter) : ""));
        }

        public void ModifyColumn(string tableName, string fieldName, DbColumnInfo info)
        {
            throw new NotImplementedException();
        }

        public List<string> GetTableList()
        {
            return GetTableList(DbConfig.DbName);
        }

        public virtual IEnumerable<DbTableInfo> GetTableInfos()
        {
            return (from DataRow row in ((DbConnection)_command.Connection).GetSchema("Tables").Rows
                    select new DbTableInfo((row.Field<object>(0) == null) ? null : row.Field<object>(0).ToString(), (row.Field<object>(1) == null) ? null : row.Field<object>(1).ToString(), (row.Field<object>(2) == null) ? null : row.Field<object>(2).ToString(), (row.Field<object>(3) == null) ? null : row.Field<object>(3).ToString(), (row.ItemArray.Count() < 5) ? null : ((row.Field<object>(4) == null) ? null : row.Field<object>(4).ToString()))).Distinct().ToList();
        }

        public List<DbColumnInfo> GetColumnInfos(string tableName)
        {
            return GetColumnInfos(DbConfig.DbName, tableName);
        }

        public virtual List<string> GetColumnNames(string tableName)
        {
            return GetColumnNames(DbConfig.DbName, tableName);
        }

        public virtual int GetColumnCount(string tableName)
        {
            return GetColumnNames(tableName).Count;
        }

        public virtual DbColumnInfo GetColumnFromColumnName(string columnName, string dbName, string tableName)
        {
            return GetColumnInfos(dbName, tableName).FirstOrDefault((DbColumnInfo c) => c.Name == columnName);
        }

        public virtual long CountTable(string dbName, string tableName)
        {
            try
            {
                return Convert.ToInt64(ExecuteScalar("SELECT COUNT(*) FROM " + Enquote(dbName, tableName)));
            }
            catch (Exception ex)
            {
                throw new Exception("Error while counting table " + Enquote(tableName) + ": " + ex.Message);
            }
        }

        public virtual long CountTable(string tableName)
        {
            try
            {
                if (!string.IsNullOrEmpty(DbConfig.DbName))
                {
                    return CountTable(DbConfig.DbName, tableName);
                }
                return Convert.ToInt64(ExecuteScalar("SELECT COUNT(*) FROM " + Enquote(tableName)));
            }
            catch (Exception ex)
            {
                throw new Exception("Error while counting table " + Enquote(tableName) + ": " + ex.Message);
            }
        }

        public virtual long CountTableWithFilter(string schema, string catalog, string tableName, string filter)
        {
            string sql = string.Empty;
            StringBuilder from = new StringBuilder();
            try
            {
                string filterString = string.Empty;
                if (!string.IsNullOrEmpty(filter))
                {
                    filterString += filter;
                }
                if (!string.IsNullOrEmpty(catalog))
                {
                    from.Append(Enquote(catalog));
                }
                if (!string.IsNullOrEmpty(schema))
                {
                    if (from.Length > 0)
                    {
                        from.Append('.');
                    }
                    from.Append(Enquote(schema));
                }
                if (from.Length > 0)
                {
                    from.Append('.');
                }
                from.Append(Enquote(tableName));
                sql = string.Concat("SELECT COUNT(*) FROM ", from, filterString);
                return Convert.ToInt64(ExecuteScalar(sql));
            }
            catch (Exception ex)
            {
                throw new Exception(string.Concat("Error while counting table (", from, "): ", ex.Message));
            }
        }

        public virtual long CountTableWithFilter(string dbName, string tableName, string filter)
        {
            string sql = string.Empty;
            try
            {
                string filterString = string.Empty;
                if (!string.IsNullOrEmpty(filter))
                {
                    filterString = filterString + " WHERE " + filter;
                }
                sql = $"SELECT COUNT(*) FROM {Enquote(dbName, tableName) + filterString}";
                return Convert.ToInt64(ExecuteScalar(sql));
            }
            catch (Exception ex)
            {
                throw new Exception("Error while counting table (" + Enquote(dbName, tableName) + "): " + ex.Message);
            }
        }

        public virtual long CountDistinctedTableWithFilter(string dbName, string tableName, IEnumerable<string> columnsForDistinct, string filter)
        {
            string sql = string.Empty;
            try
            {
                string filterString = string.Empty;
                if (!string.IsNullOrEmpty(filter))
                {
                    filterString = filterString + " WHERE " + filter;
                }
                sql = string.Format("SELECT COUNT(DISTINCT {0}) FROM {1}", string.Join(",", columnsForDistinct.Select((string c) => Enquote(c))), Enquote(dbName, tableName) + filterString);
                return Convert.ToInt64(ExecuteScalar(sql));
            }
            catch (Exception ex)
            {
                throw new Exception("Error while counting table (" + Enquote(dbName, tableName) + "): " + ex.Message);
            }
        }

        public virtual bool DatabaseExists(string dbName)
        {
            GetShemaListCollection.getSchemaListValues = GetSchemaList();
            string tmp = dbName.ToLower();
            foreach (string getSchemaListValue in GetShemaListCollection.getSchemaListValues)
            {
                if (getSchemaListValue.ToLower().Equals(tmp))
                {
                    return true;
                }
            }
            return false;
        }

        public object StringToDbValue(string value, DbColumnTypes type)
        {
            return StringToDbValueDefault(value, type);
        }

        public virtual void RenameTable(string dbName, string tableName, string newName)
        {
            ExecuteNonQuery(string.Format("ALTER TABLE {0}.{1} RENAME TO {0}.{2}", Enquote(dbName), Enquote(tableName), Enquote(newName)));
        }

        public virtual void CreateIndex(string tableName, string indexName, IEnumerable<string> columns)
        {
            string sql = "CREATE INDEX " + Enquote(indexName) + " ON " + Enquote(tableName) + "(";
            foreach (string column in columns)
            {
                sql = sql + Enquote(column) + ",";
            }
            sql = sql.Remove(sql.Length - 1, 1);
            sql += ")";
            ExecuteNonQuery(sql);
        }

        public virtual void CreateIndex(string tableName, string indexName, IEnumerable<string> columns, int indexLength)
        {
            string sql = "CREATE INDEX " + Enquote(indexName) + " ON " + Enquote(tableName) + "(";
            foreach (string column in columns)
            {
                sql = sql + Enquote(column) + ",";
            }
            sql = sql.Remove(sql.Length - 1, 1);
            sql = sql + " (" + indexLength + "))";
            ExecuteNonQuery(sql);
        }

        public virtual void CreateIndex(string dbName, string tableName, string indexName, IEnumerable<string> columns)
        {
            string sql = "CREATE INDEX " + Enquote(indexName) + " ON " + Enquote(dbName, tableName) + "(";
            foreach (string column in columns)
            {
                sql = sql + Enquote(column) + ",";
            }
            sql = sql.Remove(sql.Length - 1, 1);
            sql += ")";
            ExecuteNonQuery(sql);
        }

        public virtual void CopyTableStructure(string tableName, string newName)
        {
            ExecuteNonQuery($"CREATE TABLE {Enquote(newName)} LIKE {Enquote(tableName)}");
        }

        public virtual void CopyTableStructure(string dbName, string tableName, string newName)
        {
            ExecuteNonQuery($"CREATE TABLE {Enquote(dbName, newName)} LIKE {Enquote(dbName, tableName)}");
        }

        public virtual void CopyTableStructure(string dbName, string tableName, string newDbName, string newName)
        {
            ExecuteNonQuery($"CREATE TABLE {Enquote(newDbName, newName)} LIKE {Enquote(dbName, tableName)}");
        }

        protected void InitConnection()
        {
            _command = GetDbCommand();
            _command.Connection = GetDbConnection();
            _command.CommandTimeout = 1814400;
        }

        protected virtual string GetSqlValueString(DbColumnValues values, int rowNo = 0)
        {
            int paramCount = 0;
            string sSqlValues = string.Empty;
            int colNo = 0;
            foreach (string sFieldName in values.Columns)
            {
                if (sSqlValues.Length > 0)
                {
                    sSqlValues += ", ";
                }
                object value = values[sFieldName];
                sSqlValues += GetValueString("p_" + colNo + "_" + rowNo, value, ref paramCount);
                colNo++;
            }
            return sSqlValues;
        }

        private string GetSqlValueString(DbColumnValues values, int rowNo, ref int paramCount)
        {
            StringBuilder sSqlValues = new StringBuilder();
            int colNo = 0;
            foreach (string sFieldName in values.Columns)
            {
                if (sSqlValues.Length > 0)
                {
                    sSqlValues.Append(", ");
                }
                object value = values[sFieldName];
                sSqlValues.Append(GetValueString("p_" + colNo + "_" + rowNo, value, ref paramCount));
                colNo++;
            }
            return sSqlValues.ToString();
        }

        protected virtual string GetSqlUpdateString(DbColumnValues values)
        {
            int paramCount = 0;
            string sSqlUpdate = string.Empty;
            int colNo = 0;
            foreach (string columnName in values.Columns)
            {
                if (sSqlUpdate.Length > 0)
                {
                    sSqlUpdate += ", ";
                }
                object value = values[columnName];
                sSqlUpdate = sSqlUpdate + Enquote(columnName) + "=" + GetValueString("p_" + colNo, value, ref paramCount);
                colNo++;
            }
            return sSqlUpdate;
        }

        private object StringToDbValueDefault(string value, DbColumnTypes type)
        {
            if (value == null || value == "NULL")
            {
                return null;
            }
            switch (type)
            {
                case DbColumnTypes.DbBigInt:
                    return Convert.ToInt64(value);
                case DbColumnTypes.DbBinary:
                    throw new NotImplementedException("Type DbBinary is not implemented");
                case DbColumnTypes.DbBool:
                    return Convert.ToBoolean(value);
                case DbColumnTypes.DbInt:
                    if (value == "True")
                    {
                        return 1;
                    }
                    if (value == "False")
                    {
                        return 0;
                    }
                    return Convert.ToInt32(value);
                case DbColumnTypes.DbNumeric:
                    {
                        double val;
                        if (value.Contains(','))
                        {
                            NumberFormatInfo info = new NumberFormatInfo();
                            info.NumberDecimalSeparator = ",";
                            info.NumberGroupSeparator = ".";
                            val = Convert.ToDouble(value, info);
                        }
                        else
                        {
                            val = Convert.ToDouble(value, CultureInfo.InvariantCulture);
                        }
                        return val;
                    }
                case DbColumnTypes.DbDate:
                    return Convert.ToDateTime(value);
                case DbColumnTypes.DbDateTime:
                    return DateTime.ParseExact(value, "yyyy-MM-dd HH:mm:ss", null);
                case DbColumnTypes.DbTime:
                    return Convert.ToDateTime(value);
                case DbColumnTypes.DbText:
                case DbColumnTypes.DbLongText:
                    return value;
                default:
                    throw new Exception("Unknown type.");
            }
        }

        protected string DbValueToStringDefault(object dbValue)
        {
            if (dbValue == null || dbValue.GetType() == typeof(DBNull))
            {
                return "NULL";
            }
            string result = "";
            switch (dbValue.GetType().Name)
            {
                case "String":
                    result = dbValue.ToString();
                    break;
                case "DateTime":
                    result = ((DateTime)dbValue).ToString("yyyy-MM-dd HH:mm:ss");
                    break;
                case "TimeSpan":
                    result = ((TimeSpan)dbValue).Ticks.ToString(CultureInfo.InvariantCulture);
                    break;
                case "Decimal":
                case "Float":
                case "Double":
                    result = dbValue.ToString().Replace(',', '.');
                    break;
                case "Nullable`1":
                    if (dbValue.GetType() == typeof(DateTime?))
                    {
                        DateTime? cval = dbValue as DateTime?;
                        if (cval.HasValue)
                        {
                            result = cval.Value.ToString("yyyy-MM-dd HH:mm:ss");
                            break;
                        }
                        return "NULL";
                    }
                    if (dbValue.GetType() == typeof(int?))
                    {
                        int? cval2 = (int?)dbValue;
                        if (cval2.HasValue)
                        {
                            result = cval2.Value.ToString(CultureInfo.InvariantCulture);
                        }
                        break;
                    }
                    if (dbValue.GetType() == typeof(long?))
                    {
                        long? cval3 = (long?)dbValue;
                        if (cval3.HasValue)
                        {
                            result = cval3.Value.ToString(CultureInfo.InvariantCulture);
                        }
                        break;
                    }
                    if (dbValue.GetType() == typeof(uint?))
                    {
                        uint? cval4 = (uint?)dbValue;
                        if (cval4.HasValue)
                        {
                            result = cval4.Value.ToString(CultureInfo.InvariantCulture);
                        }
                        break;
                    }
                    if (dbValue.GetType() == typeof(ulong?))
                    {
                        ulong? cval6 = (ulong?)dbValue;
                        if (cval6.HasValue)
                        {
                            result = cval6.Value.ToString(CultureInfo.InvariantCulture);
                        }
                        break;
                    }
                    if (dbValue.GetType() == typeof(bool?))
                    {
                        bool? cval8 = (bool?)dbValue;
                        if (cval8.HasValue)
                        {
                            result = cval8.Value.ToString();
                            break;
                        }
                        return "NULL";
                    }
                    if (dbValue.GetType() == typeof(float?))
                    {
                        float? cval10 = (float?)dbValue;
                        if (cval10.HasValue)
                        {
                            result = cval10.Value.ToString(CultureInfo.InvariantCulture).Replace(',', '.');
                            break;
                        }
                        return "NULL";
                    }
                    if (dbValue.GetType() == typeof(double?))
                    {
                        double? cval9 = (double?)dbValue;
                        if (cval9.HasValue)
                        {
                            result = cval9.Value.ToString(CultureInfo.InvariantCulture).Replace(',', '.');
                            break;
                        }
                        return "NULL";
                    }
                    if (dbValue.GetType() == typeof(decimal?))
                    {
                        decimal? cval7 = (decimal?)dbValue;
                        if (cval7.HasValue)
                        {
                            result = cval7.Value.ToString(CultureInfo.InvariantCulture).Replace(',', '.');
                            break;
                        }
                        return "NULL";
                    }
                    if (dbValue.GetType() == typeof(TimeSpan?))
                    {
                        TimeSpan? cval5 = (TimeSpan?)dbValue;
                        if (cval5.HasValue)
                        {
                            result = cval5.Value.Ticks.ToString(CultureInfo.InvariantCulture);
                            break;
                        }
                        return "NULL";
                    }
                    throw new NotImplementedException("Type not implemented.");
                default:
                    result = dbValue.ToString();
                    break;
            }
            return result;
        }

        public object CallProcedure(string commandText, Dictionary<string, object> parameters)
        {
            try
            {
                _command.CommandText = commandText;
                _command.Parameters.Clear();
                foreach (KeyValuePair<string, object> param in parameters)
                {
                    IDbDataParameter p = _command.CreateParameter();
                    p.ParameterName = param.Key;
                    p.Value = param.Value ?? DBNull.Value;
                    _command.Parameters.Add(p);
                }
                _log.Debug(string.Format("Procedure called with Parameters ('{0}') ", string.Join("','", parameters.Values)));
                object result = _command.ExecuteScalar();
                Thread.Sleep(5000);
                return result;
            }
            catch (NullReferenceException)
            {
                throw new OperationCanceledException();
            }
            catch (MySqlException ex2)
            {
                ChangeDatabase(_command.Connection.Database);
                if (ex2.Number == 1317)
                {
                    throw new OperationCanceledException();
                }
                throw ex2;
            }
            catch (Exception)
            {
                ChangeDatabase(_command.Connection.Database);
                throw;
            }
        }

        public IDataReader GetTextReaderFromTable(DbTableInfo tableInfo, List<DbColumnInfo> columns = null)
        {
            StringBuilder from = new StringBuilder();
            if (!string.IsNullOrEmpty(tableInfo.Catalog))
            {
                from.Append(Enquote(tableInfo.Catalog));
            }
            if (!string.IsNullOrEmpty(tableInfo.Schema))
            {
                if (from.Length > 0)
                {
                    from.Append('.');
                }
                from.Append(Enquote(tableInfo.Schema));
            }
            if (from.Length > 0)
            {
                from.Append('.');
            }
            from.Append(Enquote(tableInfo.Name));
            StringBuilder sb = new StringBuilder();
            if (columns == null)
            {
                columns = new List<DbColumnInfo>();
                foreach (DbColumnInfo ci2 in tableInfo.Columns)
                {
                    columns.Add(ci2);
                }
            }
            foreach (DbColumnInfo ci in columns)
            {
                if (sb.Length > 0)
                {
                    sb.Append(',');
                }
                sb.Append(Enquote(ci.Name));
            }
            sb.Append(" FROM ");
            sb.Append(from);
            sb.Append(" ");
            sb.Append(tableInfo.Filter);
            return ExecuteReader("SELECT " + sb);
        }

        public string GetThumbnailPathByPath(string dbName, string tableName, string path)
        {
            string result = string.Empty;
            try
            {
                return Convert.ToString(ExecuteScalar("SELECT `thumbnail_path` FROM " + Enquote(dbName, tableName) + " AS t  WHERE t.`path` ='" + path.Replace("\\", "\\\\") + "' limit 1;"));
            }
            catch (Exception ex)
            {
                throw new Exception("Error: " + Enquote(tableName) + ": " + ex.Message);
            }
        }

        public string GetReserve(string dbName, string tableName, string path)
        {
            string result = string.Empty;
            try
            {
                return Convert.ToString(ExecuteScalar("SELECT `reserve` FROM " + Enquote(dbName, tableName) + " AS t  WHERE t.`path` ='" + path.Replace("\\", "\\\\") + "' limit 1;"));
            }
            catch (Exception ex)
            {
                throw new Exception("Error: " + Enquote(tableName) + ": " + ex.Message);
            }
        }

        public string GetPath(string dbName, string tableName, string filter)
        {
            string result = string.Empty;
            try
            {
                return Convert.ToString(ExecuteScalar("SELECT `path` FROM " + Enquote(dbName, tableName) + " WHERE " + filter + " limit 1;"));
            }
            catch (Exception ex)
            {
                throw new Exception("Error: " + Enquote(tableName) + ": " + ex.Message);
            }
        }

        public int GetMaxOfValue(string dbName, string tableName, string colForMaxValue, string filter)
        {
            int result = 0;
            try
            {
                return Convert.ToInt32(ExecuteScalar("SELECT `" + colForMaxValue + "` FROM " + Enquote(dbName, tableName) + " WHERE " + filter + " limit 1;"));
            }
            catch (Exception ex)
            {
                throw new Exception("Error: " + Enquote(tableName) + ": " + ex.Message);
            }
        }

        public List<string> GetBelegParts(string dbName, string dokId)
        {
            List<string> pathes = new List<string>();
            try
            {
                string query = $"SELECT `path` FROM `{dbName}`.`belegarchiv_file_structure` WHERE `ITEM_ID` IN ({$"SELECT `ITEM_ID` FROM `{dbName}`.`belegarchiv_file_structure` WHERE `OBJECT_ID` = '{dokId}'"}) ORDER BY `PART_NUMBER`";
                using IDataReader reader = ExecuteReader(query);
                while (reader.Read())
                {
                    pathes.Add(reader.GetString(0));
                }
                return pathes;
            }
            catch (Exception ex)
            {
                throw new Exception("Error: " + Enquote("belegarchiv_file_structure") + ": " + ex.Message);
            }
        }

        public bool CheckDataBaseIfExists(string databaseName)
        {
            string cmd = $"select count(*) from INFORMATION_SCHEMA.SCHEMATA where schema_name = '{databaseName}';";
            IDataReader reader = ExecuteReader(cmd);
            if (reader.Read())
            {
                if (int.Parse(reader.GetValue(0).ToString()) < 1)
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        public int Prepare(string sql)
        {
            MySqlCommand cmd = new MySqlCommand(sql, (MySqlConnection)Connection);
            if (_transaction != null)
            {
                cmd.Transaction = _transaction as MySqlTransaction;
            }
            cmd.Prepare();
            int stmtNr = stmtCount++;
            preparedCommands.Add(stmtNr, cmd);
            return stmtNr;
        }

        public void SetParameterForPreparedStmt(int stmtNr, string pName, object pValue)
        {
            if (preparedCommands.TryGetValue(stmtNr, out var cmd))
            {
                foreach (MySqlParameter param in cmd.Parameters)
                {
                    if (param.ParameterName.Equals(pName, StringComparison.OrdinalIgnoreCase))
                    {
                        param.Value = pValue;
                        return;
                    }
                }
                cmd.Parameters.Add(new MySqlParameter(pName, pValue));
                return;
            }
            throw new Exception("The statement number was wrong!");
        }

        public int ExecutePreparedNonQuery(int stmtNr)
        {
            if (preparedCommands.TryGetValue(stmtNr, out var cmd))
            {
                if (!IsOpen)
                {
                    Open();
                }
                return cmd.ExecuteNonQuery();
            }
            throw new Exception("The statement number was wrong!");
        }

        public object ExecutePreparedScalar(int stmtNr)
        {
            if (preparedCommands.TryGetValue(stmtNr, out var cmd))
            {
                if (!IsOpen)
                {
                    Open();
                }
                return cmd.ExecuteScalar();
            }
            throw new Exception("The statement number was wrong!");
        }

        public IDataReader ExecutePreparedReader(int stmtNr)
        {
            if (preparedCommands.TryGetValue(stmtNr, out var cmd))
            {
                if (!IsOpen)
                {
                    Open();
                }
                return cmd.ExecuteReader();
            }
            throw new Exception("The statement number was wrong!");
        }

        public void ClosePreparedStmt(int stmtNr)
        {
            if (preparedCommands.TryGetValue(stmtNr, out var cmd))
            {
                cmd.Dispose();
                preparedCommands.Remove(stmtNr);
            }
        }

        public DatabaseBase(DbConfig dbConfig)
            : this(dbConfig, '`', '`')
        {
        }

        public IDbConnection GetDbConnection()
        {
            return new MySqlConnection();
        }

        public bool ColumnExists(string dbName, string tableName, string columnName)
        {
            if (DatabaseExists(dbName))
            {

                _command.CommandText = $"SHOW COLUMNS FROM {Enquote(dbName, tableName)} WHERE Field = '{columnName.ToLower()}'";
                return _command.ExecuteScalar() != null;
            }
            return false;
        }

        //ARV
        public bool TableExists(string dbName, string tableName)
        {
            if (DatabaseExists(dbName))
            {
                _command.CommandText = string.Format("SHOW TABLES FROM {0} WHERE LOWER({1}) = '{2}'", Enquote(dbName), Enquote("tables_in_" + dbName), tableName.ToLower());
                return _command.ExecuteScalar() != null;
            }
            return false;
        }
        //public bool TableExists(string dbName, string tableName)
        //{
        //    bool isTablePresent = false;
        //    //List<string> lstTables = new List<string>();
        //    if (lstTables.Count > 0 && lstTables.Where(x => x == dbName + "#" + tableName).FirstOrDefault() == dbName + "#" + tableName)
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        _command.CommandText = string.Format("SHOW TABLES FROM {0}", Enquote(dbName));
        //        using (IDataReader reader = _command.ExecuteReader())
        //        {

        //            while (reader.Read())
        //            {
        //                if (lstTables.Where(x => x == dbName + "#" + reader.GetString(0)).FirstOrDefault() ==null)
        //                {
        //                    lstTables.Add(dbName + "#" + reader.GetString(0));
        //                }
        //            }

        //        }

        //        if (lstTables.Count > 0 && lstTables.Where(x => x == dbName + "#" + tableName).FirstOrDefault() == dbName + "#" + tableName)
        //        {
        //            return true;
        //        }
        //        else {
        //            return false;
        //        }

        //    }
        //    //if (dictDbTables.Count > 0 && dictDbTables.ContainsKey(dbName))
        //    //{
        //    //    foreach (var table in dictDbTables.Values)
        //    //    {
        //    //        foreach (var item in table)
        //    //        {
        //    //            if (item == tableName)
        //    //            {
        //    //                return true;
        //    //            }
        //    //        }
        //    //    }
        //    //}
        //    //else
        //    //{
        //    //    _command.CommandText = string.Format("SHOW TABLES FROM {0}", Enquote(dbName));
        //    //    using (IDataReader reader = _command.ExecuteReader())
        //    //    {
        //    //        List<string> lstTables = new List<string>();
        //    //        while (reader.Read())
        //    //        {
        //    //            lstTables.Add(reader.GetString(0));

        //    //            //history.Add(new HistoryValues(reader.GetInt32(3), reader.GetInt32(0), reader.GetInt32(1), reader.GetString(2), reader.GetInt32(7), reader.GetInt32(6), reader.GetBoolean(5)));
        //    //        }
        //    //        if (dictDbTables.ContainsKey(dbName))
        //    //        {
        //    //            dictDbTables[dbName] = lstTables;
        //    //        }
        //    //        else {
        //    //            dictDbTables.Add(dbName,lstTables);
        //    //        }
        //    //    }

        //    //    if ( dictDbTables.ContainsKey(dbName))
        //    //    {
        //    //        foreach (var table in dictDbTables.Values)
        //    //        {
        //    //            foreach (var item in table)
        //    //            {
        //    //                if (item == tableName)
        //    //                {
        //    //                    return true;
        //    //                }
        //    //            }
        //    //        }
        //    //    }
        //    //    //_command.ExecuteScalar() != null;
        //    //}
        //    //if (DatabaseExists(dbName))
        //    //{
        //    //	_command.CommandText = string.Format("SHOW TABLES FROM {0} WHERE LOWER({1}) = '{2}'", Enquote(dbName), Enquote("tables_in_" + dbName), tableName.ToLower());
        //    //	return _command.ExecuteScalar() != null;
        //    //}
        //    //return false;
        //    //return isTablePresent;
        //}

        public bool TableExists(string tableName)
        {
            return TableExists(DbConfig.DbName, tableName);
        }

        public bool DeleteParameterHistory(string dbName, string tableName, int userId, int parameterId, string name)
        {
            try
            {
                _command.CommandText = "DELETE FROM `" + dbName + "`.`" + tableName + "` WHERE user_id = " + userId + " AND parameter_id = " + parameterId + " AND name = '" + name + "';";
                if (_command.Connection.State == ConnectionState.Closed)
                {
                    _command.Connection.Open();
                }
                return Convert.ToBoolean(_command.ExecuteNonQuery());
            }
            finally
            {
                _command.Parameters.Clear();
            }
        }

        public List<HistoryValues> HistoryWriter(string dbName, string tablename, int userId, int parameterId)
        {
            List<HistoryValues> history = new List<HistoryValues>();
            try
            {
                _command.CommandText = "SELECT user_id, parameter_id, value, id, name, user_defined, ordinal, selection_type FROM `" + dbName + "`.`" + tablename + "` WHERE user_id = " + userId + " AND parameter_id = " + parameterId + " ORDER BY id desc";
                if (_command.Connection.State == ConnectionState.Closed)
                {
                    _command.Connection.Open();
                }
                using (IDataReader reader = _command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        history.Add(new HistoryValues(reader.GetInt32(3), reader.GetInt32(0), reader.GetInt32(1), reader.GetString(2), reader.GetInt32(7), reader.GetInt32(6), reader.GetBoolean(5)));
                    }
                }
                return history;
            }
            finally
            {
                _command.Parameters.Clear();
            }
        }

        public void OrdinalOrderBy(string dbName, string tablename, int userId, int parameterId)
        {
            try
            {
                List<string> columnName = new List<string>();
                _command.CommandText = "SELECT name FROM `" + dbName + "`.`" + tablename + "` WHERE user_id = " + userId + " AND parameter_id = " + parameterId + " ORDER BY id DESC";
                if (_command.Connection.State == ConnectionState.Closed)
                {
                    _command.Connection.Open();
                }
                using (IDataReader reader = _command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        columnName.Add(reader.GetString(0));
                    }
                }
                for (int i = 0; i < columnName.Count; i++)
                {
                    _command.CommandText = "UPDATE `" + dbName + "`.`" + tablename + "` SET ordinal = " + i + " WHERE user_id  = " + userId + " AND parameter_id = " + parameterId + " AND name = '" + columnName[i] + "'";
                    _command.ExecuteNonQuery();
                }
            }
            finally
            {
                _command.Parameters.Clear();
            }
        }

        public bool ProcedureExists(string dbName, string procedureName)
        {
            try
            {
                _command.CommandText = "SELECT COUNT(*) FROM information_schema.routines WHERE routine_schema = ?routine_schema AND routine_name = ?routine_name";
                _command.Parameters.Clear();
                _command.Parameters.Add(new MySqlParameter("routine_schema", dbName));
                _command.Parameters.Add(new MySqlParameter("routine_name", procedureName));
                return Convert.ToBoolean(_command.ExecuteScalar());
            }
            finally
            {
                _command.Parameters.Clear();
            }
        }

        public bool ProcedureExists(string procedureName)
        {
            return ProcedureExists(DbConfig.DbName, procedureName);
        }

        public IDbCommand GetDbCommand()
        {
            return new MySqlCommand();
        }

        public IDbDataAdapter GetDbAdapter()
        {
            return new MySqlDataAdapter();
        }

        public string ReplaceSpecialChars(string value)
        {
            return value.Replace(Convert.ToString('Ã') + Convert.ToString('\u0084'), "Ä").Replace(Convert.ToString('Ã') + Convert.ToString('„'), "Ä").Replace(Convert.ToString('Ã') + Convert.ToString('¤'), "ä")
                .Replace(Convert.ToString('Ã') + Convert.ToString('\u0096'), "Ö")
                .Replace(Convert.ToString('Ã') + Convert.ToString('–'), "Ö")
                .Replace(Convert.ToString('Ã') + Convert.ToString('¶'), "ö")
                .Replace(Convert.ToString('Ã') + Convert.ToString('\u009c'), "Ü")
                .Replace(Convert.ToString('Ã') + Convert.ToString('œ'), "Ü")
                .Replace(Convert.ToString('Ã') + Convert.ToString('¼'), "ü")
                .Replace(Convert.ToString('Ã') + Convert.ToString('\u0083'), "ß")
                .Replace(Convert.ToString('Ã') + Convert.ToString('\u009f'), "ß");
        }

        public string GetSqlString2(string value)
        {
            if (value.Contains("\\'"))
            {
                return GetSqlString(value);
            }
            string mySqlEscape = MySqlHelper.EscapeString(value);
            return GetSqlString(mySqlEscape);
        }

        public void CreateDatabaseIfNotExists(string dbName)
        {
            ExecuteNonQuery("CREATE DATABASE IF NOT EXISTS " + dbName);
        }

        public void DropDatabaseIfExists(string dbName)
        {
            ExecuteNonQuery("DROP DATABASE IF EXISTS " + Enquote(dbName));
        }

        public void DropTableIfExists(string dbName, string tableName)
        {
            ExecuteNonQuery("DROP TABLE IF EXISTS " + Enquote(dbName, tableName));
        }

        public IEnumerable<string> GetSchemaList()
        {
            return GetColumnStringValues("SHOW DATABASES");
        }

        public List<string> GetTableList(string dbName)
        {
            try
            {
                return GetColumnStringValues("SHOW TABLES FROM " + Enquote(dbName));
            }
            catch (Exception ex)
            {
                throw new Exception("Could not get table list: " + ex.Message);
            }
        }

        public void ClearTable(string dbName, string tableName)
        {
            ExecuteNonQuery("TRUNCATE TABLE " + Enquote(dbName, tableName));
        }

        protected string GetValueString(string parameterName, object value, ref int paramCount)
        {
            if (value == null || value.GetType() == typeof(DBNull))
            {
                return "NULL";
            }
            MySqlParameter mysqlParam = null;
            string sSqlValues;
            switch (value.GetType().Name.ToLower())
            {
                case "string":
                    mysqlParam = new MySqlParameter(parameterName, MySqlDbType.String);
                    _command.Parameters.Add(mysqlParam);
                    mysqlParam.Value = value;
                    sSqlValues = "?" + parameterName;
                    paramCount++;
                    break;
                case "datetime":
                    sSqlValues = GetSqlString2(((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss"));
                    break;
                case "timespan":
                    sSqlValues = ((TimeSpan)value).Ticks.ToString(CultureInfo.InvariantCulture);
                    break;
                case "mysqldatetime":
                    {
                        MySqlParameter oParamDateTime = new MySqlParameter(parameterName, MySqlDbType.DateTime);
                        _command.Parameters.Add(oParamDateTime);
                        oParamDateTime.Value = value;
                        sSqlValues = "?" + parameterName;
                        paramCount++;
                        break;
                    }
                case "decimal":
                case "float":
                case "double":
                    sSqlValues = GetSqlString2(value.ToString()).Replace(',', '.');
                    break;
                case "boolean":
                    sSqlValues = (((bool)value) ? "1" : "0");
                    break;
                case "byte[]":
                    {
                        MySqlParameter oParam = new MySqlParameter(parameterName, MySqlDbType.LongBlob, ((byte[])value).Length);
                        _command.Parameters.Add(oParam);
                        oParam.Value = value;
                        sSqlValues = "?" + parameterName;
                        paramCount++;
                        break;
                    }
                case "nullable`1":
                    if (value.GetType() == typeof(DateTime?))
                    {
                        DateTime? cval2 = (DateTime?)value;
                        if (cval2.HasValue)
                        {
                            sSqlValues = GetSqlString2(cval2.Value.ToString("yyyy-MM-dd HH:mm:ss"));
                            break;
                        }
                        return "NULL";
                    }
                    if (value.GetType() == typeof(int?))
                    {
                        int? cval4 = (int?)value;
                        if (cval4.HasValue)
                        {
                            sSqlValues = cval4.Value.ToString(CultureInfo.InvariantCulture);
                            break;
                        }
                        return "NULL";
                    }
                    if (value.GetType() == typeof(long?))
                    {
                        long? cval6 = (long?)value;
                        if (cval6.HasValue)
                        {
                            sSqlValues = cval6.Value.ToString(CultureInfo.InvariantCulture);
                            break;
                        }
                        return "NULL";
                    }
                    if (value.GetType() == typeof(uint?))
                    {
                        uint? cval8 = (uint?)value;
                        if (cval8.HasValue)
                        {
                            sSqlValues = cval8.Value.ToString(CultureInfo.InvariantCulture);
                            break;
                        }
                        return "NULL";
                    }
                    if (value.GetType() == typeof(ulong?))
                    {
                        ulong? cval10 = (ulong?)value;
                        if (cval10.HasValue)
                        {
                            sSqlValues = cval10.Value.ToString(CultureInfo.InvariantCulture);
                            break;
                        }
                        return "NULL";
                    }
                    if (value.GetType() == typeof(bool?))
                    {
                        bool? cval9 = (bool?)value;
                        if (cval9.HasValue)
                        {
                            sSqlValues = cval9.Value.ToString();
                            break;
                        }
                        return "NULL";
                    }
                    if (value.GetType() == typeof(float?))
                    {
                        float? cval7 = (float?)value;
                        if (cval7.HasValue)
                        {
                            sSqlValues = cval7.Value.ToString(CultureInfo.InvariantCulture).Replace(',', '.');
                            break;
                        }
                        return "NULL";
                    }
                    if (value.GetType() == typeof(double?))
                    {
                        double? cval5 = (double?)value;
                        if (cval5.HasValue)
                        {
                            sSqlValues = cval5.Value.ToString(CultureInfo.InvariantCulture).Replace(',', '.');
                            break;
                        }
                        return "NULL";
                    }
                    if (value.GetType() == typeof(decimal?))
                    {
                        decimal? cval3 = (decimal?)value;
                        if (cval3.HasValue)
                        {
                            sSqlValues = cval3.Value.ToString(CultureInfo.InvariantCulture).Replace(',', '.');
                            break;
                        }
                        return "NULL";
                    }
                    if (value.GetType() == typeof(TimeSpan?))
                    {
                        TimeSpan? cval = (TimeSpan?)value;
                        if (cval.HasValue)
                        {
                            sSqlValues = cval.Value.Ticks.ToString(CultureInfo.InvariantCulture);
                            break;
                        }
                        return "NULL";
                    }
                    throw new NotImplementedException("Type not implemented.");
                case "int16":
                    _command.Parameters.Add(new MySqlParameter(parameterName, MySqlDbType.Int16)
                    {
                        Value = value
                    });
                    sSqlValues = "?" + parameterName;
                    paramCount++;
                    break;
                case "int32":
                    _command.Parameters.Add(new MySqlParameter(parameterName, MySqlDbType.Int32)
                    {
                        Value = value
                    });
                    sSqlValues = "?" + parameterName;
                    paramCount++;
                    break;
                default:
                    mysqlParam = new MySqlParameter(parameterName, MySqlDbType.String);
                    _command.Parameters.Add(mysqlParam);
                    mysqlParam.Value = value.ToString();
                    sSqlValues = "?" + parameterName;
                    paramCount++;
                    break;
            }
            return sSqlValues;
        }

        public string GetValueString(object value)
        {
            string sSqlValues = null;
            if (value == null || value.GetType() == typeof(DBNull))
            {
                return "NULL";
            }
            switch (value.GetType().Name)
            {
                case "String":
                    sSqlValues = GetSqlString2(value.ToString());
                    break;
                case "DateTime":
                    sSqlValues = GetSqlString2(((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss"));
                    break;
                case "TimeSpan":
                    sSqlValues = ((TimeSpan)value).Ticks.ToString(CultureInfo.InvariantCulture);
                    break;
                case "MySqlDateTime":
                    sSqlValues = null;
                    break;
                case "Decimal":
                case "Float":
                case "Double":
                    sSqlValues = GetSqlString2(value.ToString()).Replace(',', '.');
                    break;
                case "Byte[]":
                    sSqlValues = null;
                    break;
                case "Nullable`1":
                    if (value.GetType() == typeof(DateTime?))
                    {
                        DateTime? cval = (DateTime?)value;
                        if (cval.HasValue)
                        {
                            sSqlValues = GetSqlString2(cval.Value.ToString("yyyy-MM-dd HH:mm:ss"));
                        }
                        break;
                    }
                    if (value.GetType() == typeof(int?))
                    {
                        int? cval2 = (int?)value;
                        if (cval2.HasValue)
                        {
                            sSqlValues = cval2.Value.ToString(CultureInfo.InvariantCulture);
                        }
                        break;
                    }
                    if (value.GetType() == typeof(long?))
                    {
                        long? cval3 = (long?)value;
                        if (cval3.HasValue)
                        {
                            sSqlValues = cval3.Value.ToString(CultureInfo.InvariantCulture);
                        }
                        break;
                    }
                    if (value.GetType() == typeof(uint?))
                    {
                        uint? cval4 = (uint?)value;
                        if (cval4.HasValue)
                        {
                            sSqlValues = cval4.Value.ToString(CultureInfo.InvariantCulture);
                        }
                        break;
                    }
                    if (value.GetType() == typeof(ulong?))
                    {
                        ulong? cval6 = (ulong?)value;
                        if (cval6.HasValue)
                        {
                            sSqlValues = cval6.Value.ToString(CultureInfo.InvariantCulture);
                            break;
                        }
                        return "NULL";
                    }
                    if (value.GetType() == typeof(bool?))
                    {
                        bool? cval8 = (bool?)value;
                        if (cval8.HasValue)
                        {
                            sSqlValues = cval8.Value.ToString();
                            break;
                        }
                        return "NULL";
                    }
                    if (value.GetType() == typeof(float?))
                    {
                        float? cval10 = (float?)value;
                        if (cval10.HasValue)
                        {
                            sSqlValues = cval10.Value.ToString(CultureInfo.InvariantCulture).Replace(',', '.');
                            break;
                        }
                        return "NULL";
                    }
                    if (value.GetType() == typeof(double?))
                    {
                        double? cval9 = (double?)value;
                        if (cval9.HasValue)
                        {
                            sSqlValues = cval9.Value.ToString(CultureInfo.InvariantCulture).Replace(',', '.');
                            break;
                        }
                        return "NULL";
                    }
                    if (value.GetType() == typeof(decimal?))
                    {
                        decimal? cval7 = (decimal?)value;
                        if (cval7.HasValue)
                        {
                            sSqlValues = cval7.Value.ToString(CultureInfo.InvariantCulture).Replace(',', '.');
                            break;
                        }
                        return "NULL";
                    }
                    if (value.GetType() == typeof(TimeSpan?))
                    {
                        TimeSpan? cval5 = (TimeSpan?)value;
                        if (cval5.HasValue)
                        {
                            sSqlValues = cval5.Value.Ticks.ToString(CultureInfo.InvariantCulture);
                            break;
                        }
                        return "NULL";
                    }
                    throw new NotImplementedException("Type not implemented.");
                default:
                    sSqlValues = value.ToString();
                    break;
            }
            return sSqlValues;
        }

        public void RenameColumn(string tableName, string oldName, string newName)
        {
            IEnumerable<DbColumnInfo> colInfo = from col in GetColumnInfos(tableName)
                                                where col.Name.ToLower() == oldName.ToLower()
                                                select col;
            string temp = new Regex("`" + oldName + "`", RegexOptions.IgnoreCase).Replace(DbMapping.GetColumnDefinitionFromColumnInfo(colInfo.FirstOrDefault()), " ");
            if (colInfo.Count() != 1)
            {
                throw new InvalidOperationException("Die Spalte " + oldName + " existiert nicht in der Tabelle " + tableName);
            }
            ExecuteNonQuery("ALTER TABLE " + Enquote(tableName) + " CHANGE COLUMN " + Enquote(oldName) + " " + Enquote(newName) + " " + temp);
        }

        public void RenameTable(string tableName, string newName)
        {
            ExecuteNonQuery("RENAME TABLE " + tableName + " TO " + newName);
        }

        public string DbValueToString(object dbValue)
        {
            if (dbValue == null || dbValue.GetType() == typeof(DBNull))
            {
                return "NULL";
            }
            string name = dbValue.GetType().Name;
            if (name == "MySqlDateTime")
            {
                return ((MySqlDateTime)dbValue).GetDateTime().ToString("yyyy-MM-dd HH:mm:ss");
            }
            return DbValueToStringDefault(dbValue);
        }

        public List<string> GetColumnNames(string dbName, string tableName)
        {
            List<string> columns = new List<string>();
            using IDataReader reader = ExecuteReader("SHOW COLUMNS FROM " + Enquote(dbName, tableName));
            while (reader.Read())
            {
                columns.Add(reader["Field"].ToString());
            }
            return columns;
        }

        public List<DbColumnInfo> GetColumnInfos(string dbName, string tableName)
        {
            string sSQL = "EXPLAIN " + Enquote(dbName) + "." + Enquote(tableName);
            List<DbColumnInfo> lFieldInfos = new List<DbColumnInfo>();
            using (NDC.Push(LogHelper.GetNamespaceContext()))
            {
                using IDataReader reader = ExecuteReader(sSQL);
                try
                {
                    while (reader.Read())
                    {
                        DbColumnInfo oFieldInfo = new DbColumnInfo();
                        oFieldInfo.Name = reader.GetString(0);
                        oFieldInfo.OriginalType = reader.GetString(1);
                        if (reader.FieldCount >= 6)
                        {
                            if (reader.GetString(2) == "YES")
                            {
                                oFieldInfo.AllowDBNull = true;
                            }
                            else
                            {
                                oFieldInfo.AllowDBNull = false;
                            }
                            if (reader.GetString(3) == "PRI")
                            {
                                oFieldInfo.IsPrimaryKey = true;
                            }
                            else
                            {
                                oFieldInfo.IsPrimaryKey = false;
                            }
                            if (!reader.IsDBNull(4))
                            {
                                oFieldInfo.DefaultValue = reader.GetString(4);
                            }
                            if (!reader.IsDBNull(5) && reader.GetString(5).IndexOf("auto_increment", StringComparison.OrdinalIgnoreCase) != -1)
                            {
                                oFieldInfo.AutoIncrement = true;
                            }
                        }
                        if (oFieldInfo.OriginalType.StartsWith("datetime"))
                        {
                            oFieldInfo.Type = DbColumnTypes.DbDateTime;
                        }
                        else if (oFieldInfo.OriginalType.StartsWith("date"))
                        {
                            oFieldInfo.Type = DbColumnTypes.DbDate;
                        }
                        else if (oFieldInfo.OriginalType.StartsWith("time"))
                        {
                            oFieldInfo.Type = DbColumnTypes.DbTime;
                        }
                        else if (oFieldInfo.OriginalType.StartsWith("bigint"))
                        {
                            oFieldInfo.Type = DbColumnTypes.DbBigInt;
                            if (oFieldInfo.OriginalType.Contains("(") && oFieldInfo.OriginalType.Contains(")"))
                            {
                                oFieldInfo.NumericScale = Convert.ToInt32(oFieldInfo.OriginalType.Substring(oFieldInfo.OriginalType.IndexOf("(", StringComparison.Ordinal) + 1, oFieldInfo.OriginalType.IndexOf(")", StringComparison.Ordinal) - (oFieldInfo.OriginalType.IndexOf("(", StringComparison.Ordinal) + 1)));
                            }
                        }
                        else if (oFieldInfo.OriginalType.StartsWith("int") || oFieldInfo.OriginalType.StartsWith("smallint") || oFieldInfo.OriginalType.StartsWith("tinyint"))
                        {
                            oFieldInfo.Type = DbColumnTypes.DbInt;
                            if (oFieldInfo.OriginalType.Contains("(") && oFieldInfo.OriginalType.Contains(")"))
                            {
                                oFieldInfo.NumericScale = Convert.ToInt32(oFieldInfo.OriginalType.Substring(oFieldInfo.OriginalType.IndexOf("(", StringComparison.Ordinal) + 1, oFieldInfo.OriginalType.IndexOf(")", StringComparison.Ordinal) - (oFieldInfo.OriginalType.IndexOf("(", StringComparison.Ordinal) + 1)));
                            }
                        }
                        else if (oFieldInfo.OriginalType.StartsWith("blob") || oFieldInfo.OriginalType.StartsWith("tinyblob") || oFieldInfo.OriginalType.StartsWith("binary(0)"))
                        {
                            oFieldInfo.Type = DbColumnTypes.DbBinary;
                        }
                        else if (oFieldInfo.OriginalType.StartsWith("char") || oFieldInfo.OriginalType.StartsWith("varchar"))
                        {
                            oFieldInfo.Type = DbColumnTypes.DbText;
                            if (oFieldInfo.OriginalType.Contains("(") && oFieldInfo.OriginalType.Contains(")"))
                            {
                                oFieldInfo.MaxLength = Convert.ToInt32(oFieldInfo.OriginalType.Substring(oFieldInfo.OriginalType.IndexOf("(", StringComparison.Ordinal) + 1, oFieldInfo.OriginalType.IndexOf(")", StringComparison.Ordinal) - (oFieldInfo.OriginalType.IndexOf("(", StringComparison.Ordinal) + 1)));
                            }
                        }
                        else if (oFieldInfo.OriginalType.StartsWith("decimal"))
                        {
                            oFieldInfo.Type = DbColumnTypes.DbNumeric;
                            if (oFieldInfo.OriginalType.Contains("(") && oFieldInfo.OriginalType.Contains(",") && oFieldInfo.OriginalType.Contains(")"))
                            {
                                oFieldInfo.MaxLength = Convert.ToInt32(oFieldInfo.OriginalType.Substring(oFieldInfo.OriginalType.IndexOf("(", StringComparison.Ordinal) + 1, oFieldInfo.OriginalType.IndexOf(",", StringComparison.Ordinal) - (oFieldInfo.OriginalType.IndexOf("(", StringComparison.Ordinal) + 1)));
                                oFieldInfo.NumericScale = Convert.ToInt32(oFieldInfo.OriginalType.Substring(oFieldInfo.OriginalType.IndexOf(",", StringComparison.Ordinal) + 1, oFieldInfo.OriginalType.IndexOf(")", StringComparison.Ordinal) - (oFieldInfo.OriginalType.IndexOf(",", StringComparison.Ordinal) + 1)));
                            }
                        }
                        else if (oFieldInfo.OriginalType.StartsWith("double") || oFieldInfo.OriginalType.StartsWith("float"))
                        {
                            oFieldInfo.Type = DbColumnTypes.DbNumeric;
                            if (oFieldInfo.OriginalType.Contains("(") && oFieldInfo.OriginalType.Contains(",") && oFieldInfo.OriginalType.Contains(")"))
                            {
                                oFieldInfo.MaxLength = Convert.ToInt32(oFieldInfo.OriginalType.Substring(oFieldInfo.OriginalType.IndexOf("(", StringComparison.Ordinal) + 1, oFieldInfo.OriginalType.IndexOf(",", StringComparison.Ordinal) - (oFieldInfo.OriginalType.IndexOf("(", StringComparison.Ordinal) + 1)));
                                oFieldInfo.NumericScale = Convert.ToInt32(oFieldInfo.OriginalType.Substring(oFieldInfo.OriginalType.IndexOf(",", StringComparison.Ordinal) + 1, oFieldInfo.OriginalType.IndexOf(")", StringComparison.Ordinal) - (oFieldInfo.OriginalType.IndexOf(",", StringComparison.Ordinal) + 1)));
                            }
                            else
                            {
                                oFieldInfo.MaxLength = 25;
                                oFieldInfo.NumericScale = 7;
                            }
                        }
                        else if (oFieldInfo.OriginalType.StartsWith("longblob") || oFieldInfo.OriginalType.StartsWith("varbinary") || oFieldInfo.OriginalType.StartsWith("mediumblob"))
                        {
                            oFieldInfo.Type = DbColumnTypes.DbBinary;
                        }
                        else if (oFieldInfo.OriginalType.StartsWith("longtext") || oFieldInfo.OriginalType.StartsWith("mediumtext") || oFieldInfo.OriginalType.StartsWith("text"))
                        {
                            oFieldInfo.Type = DbColumnTypes.DbLongText;
                        }
                        else if (oFieldInfo.OriginalType.StartsWith("bit") || oFieldInfo.OriginalType.StartsWith("bool") || oFieldInfo.OriginalType.StartsWith("boolean"))
                        {
                            oFieldInfo.Type = DbColumnTypes.DbBool;
                        }
                        else
                        {
                            oFieldInfo.Type = DbColumnTypes.DbUnknown;
                        }
                        lFieldInfos.Add(oFieldInfo);
                    }
                    return lFieldInfos;
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                    return null;
                }
            }
        }

        public int GetLastInsertId()
        {
            return Convert.ToInt32(ExecuteScalar("SELECT LAST_INSERT_ID()"));
        }

        public string FunctionSubstring(string value, int startindex, int length)
        {
            return "SUBSTR(" + value + "," + startindex + "," + length + ")";
        }

        private bool IsOperationCanceledException(Exception ex)
        {
            if (!(ex.InnerException is MySqlException) || ((ex.InnerException as MySqlException).Number != 1317 && (ex.InnerException as MySqlException).Number != 1028))
            {
                if (ex is MySqlException)
                {
                    if ((ex as MySqlException).Number != 1317)
                    {
                        return (ex as MySqlException).Number == 1028;
                    }
                    return true;
                }
                return false;
            }
            return true;
        }

        public void AddAutoIncrementPrimaryColumn(string tableName, string columnName)
        {
            string sql = string.Format("ALTER TABLE {0} ADD COLUMN {1} INT(11) NOT NULL AUTO_INCREMENT FIRST, ADD PRIMARY KEY ({1})", Enquote(tableName), Enquote(columnName));
            ExecuteNonQuery(sql);
        }

        public void AddAutoIncrementPrimaryColumn(string dbName, string tableName, string columnName)
        {
            string sql = string.Format("ALTER TABLE {0} ADD COLUMN {1} INT(11) NOT NULL AUTO_INCREMENT FIRST, ADD PRIMARY KEY ({1})", Enquote(dbName, tableName), Enquote(columnName));
            ExecuteNonQuery(sql);
        }

        public void AlterAutoIncrementPrimaryColumn(string tableName, string columnName)
        {
            string sql = string.Format("ALTER TABLE {0} CHANGE COLUMN {1} {1} INT(11) NOT NULL AUTO_INCREMENT", Enquote(tableName), Enquote(columnName));
            ExecuteNonQuery(sql);
        }

        public void AlterAutoIncrementPrimaryColumn(string dbName, string tableName, string columnName)
        {
            string sql = string.Format("ALTER TABLE {0} CHANGE COLUMN {1} {1} INT(11) NOT NULL AUTO_INCREMENT", Enquote(dbName, tableName), Enquote(columnName));
            ExecuteNonQuery(sql);
        }

        public void AlterColumnType(string tableName, string columnName, string typeName)
        {
            string sql = string.Format("ALTER TABLE {0} CHANGE COLUMN {1} {1} {2}", Enquote(tableName), Enquote(columnName), typeName);
            ExecuteNonQuery(sql);
        }

        public IDbIndexInfoCollection GetIndexesFromTableName(string dbName, string tableName)
        {
            Dictionary<string, List<string>> tempList = new Dictionary<string, List<string>>();
            if (!TableExists(dbName, tableName))
            {
                return null;
            }
            using (IDataReader dr = ExecuteReader($"SHOW INDEX FROM {(string.IsNullOrWhiteSpace(dbName) ? Enquote(tableName) : Enquote(dbName, tableName))}"))
            {
                string indexName = "";
                while (dr.Read())
                {
                    string columnName = dr[4].ToString();
                    if (!columnName.Equals("_row_no_"))
                    {
                        if (!indexName.Equals(dr[2]))
                        {
                            indexName = dr[2].ToString();
                            tempList.Add(indexName, new List<string>());
                        }
                        tempList[indexName].Add(columnName);
                    }
                }
            }
            IndexInfoList list = new IndexInfoList();
            foreach (KeyValuePair<string, List<string>> index in tempList)
            {
                list.Add(new DbIndexInfo
                {
                    Name = index.Key,
                    TableName = tableName,
                    Type = (index.Key.Equals("PRIMARY") ? DbIndexType.Clustered : DbIndexType.Nonclustered),
                    Columns = index.Value
                });
            }
            return list;
        }

        public IDbIndexInfoCollection GetIndexesFromTableName(string tableName)
        {
            return GetIndexesFromTableName(string.Empty, tableName);
        }

        public string[] GetIndexNames(string tableName)
        {
            List<string> list = new List<string>();
            using (IDataReader rd = ExecuteReader($"SHOW INDEX FROM {Enquote(tableName)}"))
            {
                while (rd.Read())
                {
                    list.Add(rd.GetString(2));
                }
            }
            return list.Distinct().ToArray();
        }

        public IEnumerable<string> GetIndexNames(string dbName, string tableName)
        {
            List<string> list = new List<string>();
            using (IDataReader rd = ExecuteReader($"SHOW INDEX FROM {Enquote(dbName, tableName)}"))
            {
                while (rd.Read())
                {
                    list.Add(rd.GetString(2));
                }
            }
            return list.Distinct().ToArray();
        }

        public long GetEstimatedRowCount(string sql, bool onlySimpleOrDerivedSelectType = false)
        {
            new List<string>();
            long result = 0L;
            using IDataReader rd = ExecuteReader($"EXPLAIN ({sql})");
            int rows = 0;
            while (rd.Read())
            {
                string selectionType = rd.GetString(1).ToUpper();
                if (!onlySimpleOrDerivedSelectType || (onlySimpleOrDerivedSelectType && (selectionType == "DERIVED" || selectionType == "SIMPLE")))
                {
                    result = ((rows != 0) ? (result * long.Parse(rd.GetString(8))) : long.Parse(rd.GetString(8)));
                    rows++;
                }
            }
            return result;
        }

        public void SetHighTimeout()
        {
            new MySqlCommand("set net_write_timeout=99999; set net_read_timeout=99999", (MySqlConnection)Connection).ExecuteNonQuery();
        }

        public string PrepareCreateFromSelect(string dbNameFrom, string tableNameFrom, string selection, string dbNameCreate, string tableNameCreate, bool withouCreateTable = false)
        {
            if (!withouCreateTable)
            {
                return $"CREATE TABLE {Enquote(dbNameCreate, tableNameCreate)} SELECT {selection} FROM {Enquote(dbNameFrom, tableNameFrom)}";
            }
            return string.Format("SELECT {1} FROM {2}", Enquote(dbNameCreate, tableNameCreate), selection, Enquote(dbNameFrom, tableNameFrom));
        }

        public string GetProcedureDefinition(string dbName, string procedureName)
        {
            string proc = string.Empty;
            string procNew = string.Empty;
            using (IDataReader dataReader = ExecuteReader($"SHOW CREATE PROCEDURE {Enquote(dbName, procedureName)}"))
            {
                while (dataReader.Read())
                {
                    proc = dataReader.GetString(2);
                }
            }
            using (IDataReader rd = ExecuteReader("SELECT ROUTINE_DEFINITION FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = '" + dbName + "' AND ROUTINE_TYPE = 'PROCEDURE' AND ROUTINE_NAME = '" + procedureName + "'"))
            {
                while (rd.Read())
                {
                    procNew = rd[0].ToString();
                }
            }
            if (proc == string.Empty)
            {
                proc = procNew;
            }
            return proc;
        }

        public List<string> GetProcedures(string dbName)
        {
            List<string> result = new List<string>();
            using IDataReader rd = ExecuteReader($"SHOW PROCEDURE STATUS WHERE DB='{dbName}'");
            while (rd.Read())
            {
                result.Add(rd["Name"].ToString());
            }
            return result;
        }

        public void CreateTable(string tableName, IEnumerable<DbColumnInfo> fields, string engine = "")
        {
            try
            {
                ExecuteNonQuery("CREATE TABLE " + Enquote(tableName) + " " + CreateTableFields(fields) + " Engine = " + (string.IsNullOrEmpty(engine) ? "MyISAM" : engine) + " DEFAULT CHARSET = utf8");
            }
            catch (Exception ex)
            {
                throw new Exception("Fehler beim Erzeugen der Tabelle " + Enquote(tableName) + ": " + ex.Message, ex);
            }
        }

        private string CreateTableFields(IEnumerable<DbColumnInfo> fields)
        {
            List<string> lPrimaryKeys = new List<string>();
            StringBuilder sb = new StringBuilder();
            sb.Append("(");
            foreach (DbColumnInfo field in fields)
            {
                sb.Append(CreateTableField(field) + ",");
                if (field.IsPrimaryKey)
                {
                    lPrimaryKeys.Add(field.Name);
                }
            }
            if (lPrimaryKeys.Count > 0)
            {
                sb.Append("PRIMARY KEY (");
                for (int i = 0; i < lPrimaryKeys.Count; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(",");
                    }
                    sb.Append(Enquote(lPrimaryKeys[i]));
                }
                sb.Append("),");
            }
            return sb.ToString().Remove(sb.Length - 1) + ")";
        }

        private string CreateTableField(DbColumnInfo field)
        {
            string sql = Enquote(field.Name) + " ";
            switch (field.Type)
            {
                case DbColumnTypes.DbBool:
                    sql += "TINYINT(1)";
                    break;
                case DbColumnTypes.DbDate:
                    sql += "DATE";
                    break;
                case DbColumnTypes.DbDateTime:
                    sql += "DATETIME";
                    break;
                case DbColumnTypes.DbInt:
                    sql = sql + "INTEGER(" + (field.MaxLength + field.NumericScale) + ")" + (field.IsUnsigned ? " UNSIGNED " : "");
                    break;
                case DbColumnTypes.DbBigInt:
                    sql = sql + "BIGINT(" + (field.MaxLength + field.NumericScale) + ")" + (field.IsUnsigned ? " UNSIGNED " : "");
                    break;
                case DbColumnTypes.DbLongText:
                    sql += "TEXT";
                    break;
                case DbColumnTypes.DbNumeric:
                    sql = sql + "NUMERIC(" + (field.MaxLength - field.NumericScale) + "," + field.NumericScale + ")";
                    break;
                case DbColumnTypes.DbText:
                    sql = sql + "VARCHAR(" + field.MaxLength + ")";
                    break;
                case DbColumnTypes.DbTime:
                    sql += "TIME";
                    break;
                case DbColumnTypes.DbBinary:
                    sql += "LONGBLOB";
                    break;
            }
            sql = ((!field.AllowDBNull) ? (sql + " NOT NULL ") : (sql + " NULL "));
            if (field.AutoIncrement)
            {
                sql += "AUTO_INCREMENT ";
            }
            if (field.HasDefaultValue && !string.IsNullOrEmpty(field.DefaultValue))
            {
                sql = sql + "DEFAULT " + field.DefaultValue + " ";
            }
            return sql;
        }

        public void AddColumnInfos(DbTableInfo table)
        {
            foreach (DataRow row in ((DbConnection)_command.Connection).GetSchema("Columns", new string[3] { table.Catalog, table.Schema, table.Name }).Rows)
            {
                DbColumnInfo ci = new DbColumnInfo
                {
                    Name = ((row.Field<object>(3) == null) ? null : row.Field<object>(3).ToString())
                };
                string type = row.Field<object>(7).ToString();
                switch (type.ToLower())
                {
                    case "binary":
                        ci.Type = DbColumnTypes.DbBinary;
                        break;
                    case "char":
                    case "varchar":
                        ci.Type = DbColumnTypes.DbText;
                        break;
                    case "text":
                    case "longtext":
                    case "mediumtext":
                        ci.Type = DbColumnTypes.DbLongText;
                        break;
                    case "double":
                    case "decimal":
                        ci.Type = DbColumnTypes.DbNumeric;
                        break;
                    case "bigint":
                        ci.Type = DbColumnTypes.DbBigInt;
                        break;
                    case "int":
                    case "smallint":
                    case "tinyint":
                        ci.Type = DbColumnTypes.DbInt;
                        break;
                    case "integer unsigned":
                        ci.Type = DbColumnTypes.DbInt;
                        ci.IsUnsigned = true;
                        break;
                    case "date":
                        ci.Type = DbColumnTypes.DbDate;
                        break;
                    case "time":
                        ci.Type = DbColumnTypes.DbTime;
                        break;
                    case "datetime":
                        ci.Type = DbColumnTypes.DbDateTime;
                        break;
                    default:
                        ci.Type = DbColumnTypes.DbUnknown;
                        break;
                }
                ci.OriginalType = type;
                ci.MaxLength = (int)((row.Field<object>(8) == null) ? 0 : row.Field<ulong>(8));
                ci.NumericScale = (int)((row.Field<object>(10) == null) ? 0 : row.Field<ulong>(10));
                ci.AllowDBNull = row.Field<object>(6) != null && row.Field<string>(6) == "YES";
                ci.Comment = ((row.Field<object>(17) == null) ? null : row.Field<object>(17).ToString());
                ci.OrdinalPosition = (int)((row.Field<object>(4) == null) ? 0 : row.Field<ulong>(4));
                table.Columns.Add(ci);
            }
        }

        public bool IsIndexExisting(string database, string table, IEnumerable<string> columns)
        {
            Dictionary<string, List<string>> dict = new Dictionary<string, List<string>>(StringComparer.InvariantCultureIgnoreCase);
            string sql = $"SHOW INDEX FROM {Enquote(database, table)} ";
            using (IDataReader rd = ExecuteReader(sql))
            {
                while (rd.Read())
                {
                    string name = rd.GetString(2);
                    if (dict.ContainsKey(name))
                    {
                        dict[name].Add(rd.GetString(4).ToLower());
                        dict[name].Sort();
                    }
                    else
                    {
                        dict.Add(name, new List<string> { rd.GetString(4).ToLower() });
                    }
                }
            }
            List<string> columnsToLower = columns.Select((string column) => column.ToLower()).ToList();
            return dict.Values.Any((List<string> v) => v.All(columnsToLower.Contains) && columnsToLower.All(v.Contains));
        }

        public bool IsIndexExisting(string table, List<string> columns)
        {
            return IsIndexExisting(DbConfig.DbName, table, columns);
        }

        public void LoadCSVData(string database, string tableName, string fileName, string encoding, string fieldSeperator, string lineSeperator, string optionallyEnclosedBy, int ignoreLines)
        {
            string cmd = "LOAD DATA LOCAL INFILE '" + fileName + "' INTO TABLE " + (string.IsNullOrEmpty(database) ? "" : (Enquote(database) + ".")) + Enquote(tableName) + " CHARACTER SET " + encoding + " COLUMNS TERMINATED BY '" + fieldSeperator + "'" + (string.IsNullOrEmpty(optionallyEnclosedBy) ? "" : (" OPTIONALLY ENCLOSED BY '" + optionallyEnclosedBy + "'")) + " ESCAPED BY '' LINES TERMINATED BY '" + lineSeperator + "' IGNORE " + ignoreLines + " LINES";
            ExecuteNonQuery(cmd);
        }

        public string GetCreateTableStructure(string database, string tableName)
        {
            string result = string.Empty;
            using (IDataReader rd = ExecuteReader($"SHOW CREATE TABLE {Enquote(database, tableName)}"))
            {
                while (rd.Read())
                {
                    result = rd.GetString(1);
                }
            }
            return result.Replace(Enquote(tableName.ToLower()), Enquote(database, tableName));
        }

        public string GetCreateTableStructure(string tableName)
        {
            string result = string.Empty;
            using IDataReader rd = ExecuteReader($"SHOW CREATE TABLE {Enquote(tableName)}");
            while (rd.Read())
            {
                result = rd.GetString(1);
            }
            return result;
        }

        public void DropTableIndex(string database, string tableName, string indexName)
        {
            try
            {
                ExecuteNonQuery($"DROP INDEX {Enquote(indexName)} ON {Enquote(database, tableName)}");
            }
            catch (Exception ex)
            {
                throw new Exception("Error while droping index of table " + Enquote(database, tableName) + ": " + ex.Message, ex);
            }
        }

        public void DropTableIndex(string tableName, string indexName)
        {
            try
            {
                ExecuteNonQuery($"DROP INDEX {Enquote(indexName)} ON {Enquote(tableName)}");
            }
            catch (Exception ex)
            {
                throw new Exception("Error while droping index of table " + Enquote(tableName) + ": " + ex.Message, ex);
            }
        }

        public long GetRowCount(string database, string tableName)
        {
            try
            {
                return Convert.ToInt64(ExecuteScalar($"SELECT COUNT(*) FROM {Enquote(database, tableName)}"));
            }
            catch (Exception ex)
            {
                throw new Exception("Error while reading row count of table " + Enquote(tableName) + ": " + ex.Message, ex);
            }
        }

        public long GetRowCount(string tableName)
        {
            try
            {
                return (long)ExecuteScalar($"SELECT COUNT(*) FROM {Enquote(tableName)}");
            }
            catch (Exception ex)
            {
                throw new Exception("Error while reading row count of table " + Enquote(tableName) + ": " + ex.Message, ex);
            }
        }

        public int GetMaxValueOfColumn(string database, string table, string column)
        {
            try
            {
                object idObj = ExecuteScalar($"SELECT MAX({Enquote(column)}) FROM {Enquote(database, table)}");
                return (!(idObj is DBNull)) ? Convert.ToInt32(idObj) : 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error while trying to get column max value " + Enquote(column) + ": " + ex.Message, ex);
            }
        }

        public int GetMaxValueOfColumn(string table, string column)
        {
            try
            {
                object idObj = ExecuteScalar($"SELECT MAX({Enquote(column)}) FROM {Enquote(table)}");
                return (!(idObj is DBNull)) ? Convert.ToInt32(idObj) : 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error while trying to get column max value " + Enquote(column) + ": " + ex.Message, ex);
            }
        }

        public int GetMaxValueOfColumn(string database, string table, string column, string filter)
        {
            try
            {
                object idObj = ExecuteScalar($"SELECT MAX({Enquote(column)}) FROM {Enquote(database, table)} WHERE {filter}");
                return (!(idObj is DBNull)) ? Convert.ToInt32(idObj) : 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error while trying to get column max value " + Enquote(column) + ": " + ex.Message, ex);
            }
        }

        public void AlterTableEngine(string database, string tableName, string engine)
        {
            try
            {
                ExecuteNonQuery($"ALTER TABLE {Enquote(database, tableName)} ENGINE={Enquote(engine)}");
            }
            catch (Exception ex)
            {
                throw new Exception("Error while altering table engine of table " + Enquote(database, tableName) + ": " + ex.Message, ex);
            }
        }

        public void AlterTableEngine(string tableName, string engine)
        {
            try
            {
                ExecuteNonQuery($"ALTER TABLE {Enquote(tableName)} ENGINE={Enquote(engine)}");
            }
            catch (Exception ex)
            {
                throw new Exception("Error while altering table engine of table " + Enquote(tableName) + ": " + ex.Message, ex);
            }
        }

        public void CopyTableData(string fromTableName, string newTableName)
        {
            try
            {
                ExecuteNonQuery($"INSERT INTO {Enquote(newTableName)} SELECT * FROM {Enquote(fromTableName)}");
            }
            catch (Exception ex)
            {
                throw new Exception("Error while copying data to " + Enquote(newTableName) + ": " + ex.Message, ex);
            }
        }

        public void CopyTableData(string database, string fromTableName, string newTableName)
        {
            try
            {
                ExecuteNonQuery($"INSERT INTO {Enquote(database, newTableName)} SELECT * FROM {Enquote(database, fromTableName)}");
            }
            catch (Exception ex)
            {
                throw new Exception("Error while copying data to " + Enquote(database, newTableName) + ": " + ex.Message, ex);
            }
        }

        public void AddProperty(string key, string value, int type, string name, string country_code, string text)
        {
            string query = $"INSERT INTO {DbConfig.DbName}.properties (`key`, `value`, `type`) VALUES('{key}', '{value}', {type})";
            try
            {
                ExecuteNonQuery(query);
            }
            catch
            {
            }
            query = $"SELECT id FROM {DbConfig.DbName}.properties WHERE `key`='{key}'";
            using IDataReader reader = ExecuteReader(query);
            try
            {
                if (reader.Read())
                {
                    int ref_id = reader.GetInt32(0);
                    query = $"INSERT INTO {DbConfig.DbName}.property_texts (name, ref_id, country_code, text) VALUES('{name}', {ref_id}, '{country_code}', '{text}')";
                }
            }
            catch
            {
            }
        }

        public EngineTypes GetTableEngineByName(string databaseName, string tableName)
        {
            EngineTypes engineType = EngineTypes.Undefined;
            string query = $"SHOW TABLE STATUS FROM `{databaseName}` LIKE '{tableName}'";
            using IDataReader reader = ExecuteReader(query);
            if (reader.Read())
            {
                string engine = reader["Engine"].ToString();
                if (!string.IsNullOrWhiteSpace(engine))
                {
                    Enum.TryParse<EngineTypes>(engine, out engineType);
                    return engineType;
                }
                return engineType;
            }
            return engineType;
        }
    }
}
