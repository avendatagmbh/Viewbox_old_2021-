// --------------------------------------------------------------------------------
// author: Benjamin Held
// since:  2011-06-21
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Xml;
using DbAccess;
using DbAccess.Structures;

namespace XmlDatabaseBackup {
    public class DatabaseBackup {
        #region Delegates
        public delegate bool TableValid(string tableName);
        #endregion

        private string DBName { get; set; }

        #region Constructor
        public DatabaseBackup(string dbName) { DBName = dbName; }
        #endregion

        #region Events
        public event EventHandler Progress;
        //Fired after a new table has been imported
        private void OnProgress() { if (Progress != null) Progress(null, new EventArgs()); }

        public event EventHandler<ErrorEventArgs> Error;
        private void OnError(Exception ex) { if (Error != null) Error(this, new ErrorEventArgs(ex)); }

        public event EventHandler Finished;
        private void OnFinished() { if (Finished != null) Finished(null, new EventArgs()); }
        #endregion

        public void BackupDatabase(IDatabase conn, XmlWriter writer, string programString, string programVersion,
                                   string comment = "") {
            writer.WriteStartDocument();
            writer.WriteStartElement("Backup");
            writer.WriteAttributeString("Name", programString);
            writer.WriteAttributeString("Version", programVersion);
            writer.WriteAttributeString("Created", DateTime.Now.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("Comment", comment);
            writer.WriteStartElement("Database");
            writer.WriteAttributeString("Name", DBName);
            BackupDatabase(conn, writer);
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Close();
        }

        //This assumes that the document has already been opened
        public void BackupDatabase(IDatabase conn, XmlWriter writer) {
            List<string> tables = conn.GetTableList(DBName);

            foreach (string table in tables) {
                if (table == "sqlite_sequence") continue;
                WriteTable(conn, writer, table);
            }
        }

        //ImportDatabase expects that the tables have all been created correctly
        //Attention: It will clear the tables
        public void ImportDatabase(IDatabase conn, string filename, TableValid tableValidFunc) {
            var reader = new XmlTextReader(filename);
            ImportDatabase(conn, reader, tableValidFunc);
        }

        public void ImportDatabase(IDatabase conn, XmlReader reader, TableValid tableValidFunc) {
            var importData = new ImportData {Reader = reader, Conn = conn, TableValidFunc = tableValidFunc};            
            try {
                while (reader.Read()) {
                    switch (reader.NodeType) {
                        case XmlNodeType.Document:
                            break;
                        case XmlNodeType.Element:
                            HandleBeginElement(importData);
                            break;
                        case XmlNodeType.EndElement:
                            HandleEndElement(importData);
                            break;
                    }
                }
                OnFinished();
            } catch (Exception ex) {
                conn.RollbackTransaction();
                OnError(ex);
            }
        }

        private void HandleEndElement(ImportData data) {
            switch (data.Reader.Name) {
                case "Database":
                    break;
                case "Columns":

                    break;
                case "Table":
                    if (data.ValidTable)
                        data.Conn.CommitTransaction();
                    break;
                case "Rows":
                    if (data.ValidTable) {
                        try {
                            data.Conn.InsertInto(data.Table, data.CurrentRowValues);
                            if (data.Conn.DbConfig.DbType == "SQLServer") {
                                //Exception can occur if the table has no identity property
                                //try { data.Conn.ExecuteNonQuery("SET IDENTITY_INSERT dbo." + data.Conn.Enquote(data.Table) + " OFF"); } catch (Exception) {}
                                if (HasIdentityColumn(data))
                                    data.Conn.ExecuteNonQuery("SET IDENTITY_INSERT dbo." + data.Conn.Enquote(data.Table) +
                                                              " OFF");
                            }
                        } catch (Exception ex) {
                            throw;
                        }
                        OnProgress();
                    }
                    break;
            }
        }

        private void HandleBeginElement(ImportData data) {
            switch (data.Reader.Name) {
                case "Backup":
                    break;
                case "Database":
                    data.SourceDatabaseType = data.Reader.GetAttribute("Type");
                    break;
                case "Table":
                    data.Table = data.Reader.GetAttribute("Name");
                    if (data.TableValidFunc(data.Table)) {
                        data.Conn.BeginTransaction();
                        data.ValidTable = true;
                    } else data.ValidTable = false;
                    break;
                case "Columns":
                    data.Columns.Clear();
                    break;
                case "Column":
                    Tuple<string, DbColumnInfo> columnInfo = ReadColumn(data.Reader);

                    // fix (import SQLite backup into MS SQL or MySQL)
                    // if the database where the backup is from is SQLite and the ColumnType is DbText we check if the original Type (we created) was DbDateTime
                    // and set it back to DbDateTime because the import of the backup is into a different db system
                    if (columnInfo.Item2.Type == DbColumnTypes.DbText && data.SourceDatabaseType.Equals("SQLite") && !data.Conn.DbConfig.DbType.Equals("SQLite")) {
                        var columns = data.Conn.GetColumnInfos(data.Table);
                        if (columns != null) {
                            var foundColumn = columns.Find(x => x.Name == columnInfo.Item2.Name);
                            if(foundColumn != null) {
                                var dbColType = foundColumn.Type;
                                if (dbColType == DbColumnTypes.DbDateTime)
                                    columnInfo.Item2.Type = DbColumnTypes.DbDateTime;
                            }
                        }
                    }

                    //These have falsely been text types in some former versions
                    if (data.Conn.DbConfig.DbType.Equals("SQLServer") && 
                        (data.Table.ToLower() == "accounts" && (columnInfo.Item2.Name.ToLower() == "group_id" || columnInfo.Item2.Name.ToLower() == "id"))||
                        (data.Table.ToLower() == "values_gaap" && columnInfo.Item2.Name.ToLower() == "id") ||
                        (data.Table.ToLower() == "values_gcd" && columnInfo.Item2.Name.ToLower() == "id") ||
                        (data.Table.ToLower() == "values_gcd_company" && columnInfo.Item2.Name.ToLower() == "id") ||
                        (data.Table.ToLower() == "transfer_hbst_lines" && (columnInfo.Item2.Name.ToLower() == "id" || columnInfo.Item2.Name.ToLower() == "value_id"))||
                        (data.Table.ToLower() == "log_admin" && (columnInfo.Item2.Name.ToLower() == "id" || columnInfo.Item2.Name.ToLower() == "reference_id") ) ||
                        (data.Table.ToLower().Contains("log_report") && (columnInfo.Item2.Name.ToLower() == "id" || columnInfo.Item2.Name.ToLower() == "reference_id"))
                        ) {
                        columnInfo.Item2.Type = DbColumnTypes.DbBigInt;
                    }
                    data.Columns[columnInfo.Item1] = columnInfo.Item2;
                    break;
                case "Rows":
                    if (data.ValidTable) {
                        data.CurrentRowValues.Clear();
                        data.Conn.ClearTable(data.Table);
                        if (data.Conn.DbConfig.DbType == "SQLServer") {
                            //Exception can occur if the table has no identity property
                            //try { data.Conn.ExecuteNonQuery("SET IDENTITY_INSERT dbo." + data.Conn.Enquote(data.Table) + " ON"); } catch (Exception) { }
                            if (HasIdentityColumn(data))
                                data.Conn.ExecuteNonQuery("SET IDENTITY_INSERT dbo." + data.Conn.Enquote(data.Table) +
                                                          " ON");
                            data.Conn.ExecuteNonQuery("SET DATEFORMAT ymd");
                        }
                    }
                    break;
                case "Row":
                    if (data.ValidTable)
                        InsertRow(data);
                    break;
            }
            if (data.Reader.IsEmptyElement) HandleEndElement(data);
        }

        private bool HasIdentityColumn(ImportData data) {
            List<DbColumnInfo> columnInfos = data.Conn.GetColumnInfos(DBName, "dbo." + data.Table);
            foreach (DbColumnInfo column in columnInfos)
                if (column.IsIdentity) return true;
            return false;
        }

        private Tuple<string, DbColumnInfo> ReadColumn(XmlReader reader) {
            var columnInfo = new DbColumnInfo();
            string identifier = "";

            if (reader.HasAttributes) {
                // Attributsliste durchlaufen
                while (reader.MoveToNextAttribute()) {
                    if (reader.Name == "Identifier")
                        identifier = reader.Value;
                    else if (reader.Name == "Type") {
                        foreach (DbColumnTypes type in Enum.GetValues(typeof (DbColumnTypes))) {
                            if (type.ToString() == reader.Value) {
                                columnInfo.Type = type;
                                break;
                            }
                        }
                    } else if (reader.Name == "Name")
                        columnInfo.Name = reader.Value;
                    else if (reader.Name == "AllowDBNull")
                        columnInfo.AllowDBNull = Convert.ToBoolean(reader.Value);
                    else if (reader.Name == "AutoIncrement")
                        columnInfo.AutoIncrement = Convert.ToBoolean(reader.Value);
                    else if (reader.Name == "DefaultValue") {
                        if (!string.IsNullOrEmpty(reader.Value))
                            columnInfo.DefaultValue = reader.Value;
                    } else if (reader.Name == "IsPrimaryKey")
                        columnInfo.IsPrimaryKey = Convert.ToBoolean(reader.Value);
                    else if (reader.Name == "IsUnsigned")
                        columnInfo.IsUnsigned = Convert.ToBoolean(reader.Value);
                    else if (reader.Name == "MaxLength")
                        columnInfo.MaxLength = Convert.ToInt32(reader.Value);
                    else if (reader.Name == "NumericScale")
                        columnInfo.NumericScale = Convert.ToInt32(reader.Value);
                    else if (reader.Name == "OriginalType")
                        columnInfo.OriginalType = reader.Value;
                }
            }

            return new Tuple<string, DbColumnInfo>(identifier, columnInfo);
        }

        #region Writing
        private void WriteTable(IDatabase conn, XmlWriter writer, string table) {
            //string dboTable = conn.DbConfig.DbType == "SQLServer" ? "dbo." + table : table;
           if(table=="sqlite_sequence") return;

            List<DbColumnInfo> columnInfos = conn.GetColumnInfos(DBName, table);

            writer.WriteStartElement("Table");
            writer.WriteAttributeString("Name", table);
            WriteColumnInfos(conn, writer, columnInfos);
            WriteData(conn, writer, table, columnInfos);
            writer.WriteEndElement();
        }

        private void WriteData(IDatabase conn, XmlWriter writer, string table, List<DbColumnInfo> columnInfos) {
            //DbDataReader reader = conn.ExecuteReader("SELECT * FROM " + conn.Enquote(DBName) + "." + conn.Enquote(table));
            var reader = conn.DbConfig.DbType == "Oracle" ? conn.ExecuteReader("SELECT * FROM " + conn.Enquote(table)) : conn.ExecuteReader("SELECT * FROM " + conn.Enquote(DBName, table));

            try {
                writer.WriteStartElement("Rows");
                while (reader.Read()) {
                    writer.WriteStartElement("Row");
                    for (int i = 0; i < columnInfos.Count; ++i) {
                        string value = conn.DbValueToString(reader[columnInfos[i].Name]);
                        writer.WriteAttributeString("c" + i, value);
                    }
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            } catch (Exception ex) {
                throw ex;
            } finally {
                if (reader != null && !reader.IsClosed) {
                    reader.Close();
                }
            }
        }

        private void WriteColumnInfos(IDatabase conn, XmlWriter writer, List<DbColumnInfo> columnInfos) {
            writer.WriteStartElement("Columns");
            int index = 0;
            foreach (DbColumnInfo columnInfo in columnInfos) {
                writer.WriteStartElement("Column");
                writer.WriteAttributeString("Identifier", "c" + index++);
                writer.WriteAttributeString("Type", columnInfo.Type.ToString());
                writer.WriteAttributeString("Name", columnInfo.Name);
                writer.WriteAttributeString("AllowDBNull", Convert.ToString(columnInfo.AllowDBNull));
                writer.WriteAttributeString("AutoIncrement", Convert.ToString(columnInfo.AutoIncrement));
                writer.WriteAttributeString("DefaultValue", columnInfo.DefaultValue);
                writer.WriteAttributeString("HasDefaultValue", Convert.ToString(columnInfo.HasDefaultValue));
                writer.WriteAttributeString("IsPrimaryKey", Convert.ToString(columnInfo.IsPrimaryKey));
                writer.WriteAttributeString("IsUnsigned", Convert.ToString(columnInfo.IsUnsigned));
                writer.WriteAttributeString("MaxLength", Convert.ToString(columnInfo.MaxLength));
                writer.WriteAttributeString("NumericScale", Convert.ToString(columnInfo.NumericScale));
                writer.WriteAttributeString("OriginalType", columnInfo.OriginalType);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }
        #endregion

        #region InsertRow
        private void InsertRow(ImportData data) {
            var values = new DbColumnValues();
            foreach (var column in data.Columns) {
                string value = data.Reader.GetAttribute(column.Key);
                values[column.Value.Name] = data.Conn.StringToDbValue(value, column.Value.Type);
            }
            data.CurrentRowValues.Add(values);
            //data.Conn.InsertInto(data.Table, values);
        }
        #endregion

        #region ImportData
        private class ImportData {
            public ImportData() {
                Columns = new Dictionary<string, DbColumnInfo>();
                CurrentRowValues = new List<DbColumnValues>();
            }

            public XmlReader Reader { get; set; }
            public IDatabase Conn { get; set; }
            public string Table { get; set; }
            public Dictionary<string, DbColumnInfo> Columns { get; set; }
            public bool ValidTable { get; set; }
            //public List<string> TablesToImport { get; set; }
            public TableValid TableValidFunc { get; set; }
            public List<DbColumnValues> CurrentRowValues { get; set; }

            public string SourceDatabaseType { get; set; }
            public string DbVersion { get; set; }
        }
        #endregion
    }
}