using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;
using System.Data.Common;
using System.IO;
using System.ComponentModel;

namespace DbComparison.Business {
    #region ComparisonResult
    public class ComparisonResult {
        #region HelperClasses
        public class TableInfoMismatch {
            public TableInfoMismatch(string name, long countOther) {
                this.Name = name;
                this.CountOther = countOther;
            }

            public string Name { get; set; }
            public long CountOther{get;set;}
        }

        // Custom comparer for the Product class
        public class TableInfoComparer : IEqualityComparer<TableInfo> {
            // Products are equal if their names and product numbers are equal.
            public bool Equals(TableInfo x, TableInfo y) {

                //Check whether the compared objects reference the same data.
                if (Object.ReferenceEquals(x, y)) return true;

                //Check whether any of the compared objects is null.
                if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                    return false;

                //Check whether the products' properties are equal.
                return x.Name == y.Name;
            }

            // If Equals() returns true for a pair of objects 
            // then GetHashCode() must return the same value for these objects.

            public int GetHashCode(TableInfo tableInfo) {
                //Check whether the object is null
                if (Object.ReferenceEquals(tableInfo, null)) return 0;

                return  tableInfo.Name == null ? 0 : tableInfo.Name.GetHashCode();
            }

        }


        public class TableInfo {
            public TableInfo(string name, long count) {
                this.Name = name;
                this.RowCount = count;
            }

            public string Name { get; set; }

            #region RowCount
            private long _rowCount;

            public long RowCount {
                get { return _rowCount; }
                set { 
                    _rowCount = value < 0 ? 0 : value; 
                }
            }
            #endregion RowCount
        }

        public class ColumnInfo {
            public ColumnInfo(string name, string table, string dataType) {
                this.Name = name;
                this.TableName = table;
                this.DataType = dataType;

            }

            public string UniqueName { get { return TableName + "???" + Name; } }
            public string Name { get; set; }
            public string TableName { get; set; }
            public string DataType { get; set; }
        }

        public class ColumnInfoMismatch {
            public ColumnInfoMismatch(string name, MismatchType type, string dataTypeOther, string dataTypeOwn, int db){
                this.Name = name;
                this.Mismatch = type;
                this.DataTypeOther = dataTypeOther;
                this.DataTypeOwn = dataTypeOwn;
                this.Db = db;
            }

            public string Name { get; set; }
            public enum MismatchType{ColumnDoesNotExist,TypeMismatch};
            public MismatchType Mismatch { get; set; }
            public string DataTypeOwn { get; set; }
            public string DataTypeOther { get; set; }
            public int Db { get; set; }
        }
        #endregion HelperClasses

        #region MissingTables
        private List<TableInfoMismatch>[] _missingTables = new List<TableInfoMismatch>[2];

        public List<TableInfoMismatch>[] MissingTables {
            get { return _missingTables; }
            set { _missingTables = value; }
        }
        #endregion

        #region EmptyTables
        private List<TableInfoMismatch>[] _emptyTables = new List<TableInfoMismatch>[2];
        //Tables which are only empty in one database
        public List<TableInfoMismatch>[] EmptyTablesInOne {
            get { return _emptyTables; }
            set { _emptyTables = value; }
        }

        #endregion

        #region ColumnMismatchSingle
        SortedDictionary<string, List<ColumnInfoMismatch>> _columnMismatchSingle = new SortedDictionary<string, List<ColumnInfoMismatch>>();

        public SortedDictionary<string, List<ColumnInfoMismatch>> ColumnMismatchSingle {
            get { return _columnMismatchSingle; }
            set { _columnMismatchSingle = value; }
        }
        #endregion ColumnMismatchSingle

        private string[] _dbName = new string[2];

        public string[] DbName {
            get { return _dbName; }
            set { _dbName = value; }
        }

        public string Hostname1 { get; set; }
        public string Hostname2 { get; set; }

        #region TableCount
        private int[] _tableCount = new int[2];

        public int[] TableCount {
            get { return _tableCount; }
            set { _tableCount = value; }
        }
        #endregion TableCount

        #region ColumnCount
        private int[] _columnCount = new int[2];

        public int[] ColumnCount {
            get { return _columnCount; }
            set { _columnCount = value; }
        }
        #endregion

        #region EmptyTablesCount
        private int[] _emptyTablesCount = new int[2];

        public int[] EmptyTablesCount {
            get { return _emptyTablesCount; }
            set { _emptyTablesCount = value; }
        }
        #endregion EmptyTablesCount

        #region MissingTableToColumns
        private SortedDictionary<string, List<ColumnInfo>> _missingTableToColumns = new SortedDictionary<string, List<ColumnInfo>>();

        public SortedDictionary<string, List<ColumnInfo> > MissingTableToColumns {
            get { return _missingTableToColumns; }
            set { _missingTableToColumns = value; }
        }
        #endregion MissingTableToColumns

        #region MissingTablesDict
        private SortedDictionary<string, bool> _missingTablesDict = new SortedDictionary<string, bool>();

        public SortedDictionary<string, bool> MissingTablesDict {
            get { return _missingTablesDict; }
            set { _missingTablesDict = value; }
        }
        #endregion
    }
    #endregion ComparisonResult



    class DatabaseComparer {
        #region Properties
        private IDatabase conn1{get;set;}
        private IDatabase conn2{get;set;}
        private ComparisonResult result;
        private BackgroundWorker BgWorker { get; set; }
        IDatabaseInformation[] info = new IDatabaseInformation[2];
        #endregion

        public DatabaseComparer(IDatabase conn1, IDatabase conn2, BackgroundWorker bgWorker) {
            this.conn1 = conn1;
            this.conn2 = conn2;
            this.BgWorker = bgWorker;

            result = new ComparisonResult() {
                Hostname1 = conn1.DbConfig.Hostname,
                Hostname2 = conn2.DbConfig.Hostname
            };
            result.DbName[0] = conn1.DbConfig.DbName;
            result.DbName[1] = conn2.DbConfig.DbName;

            for (int i = 0; i < 2; ++i) {
                result.MissingTables[i] = new List<ComparisonResult.TableInfoMismatch>();
                result.EmptyTablesInOne[i] = new List<ComparisonResult.TableInfoMismatch>();
            }
        }

        public void DoWorkAndSave(string baseName) {
            
            Compare();
            BgWorker.ReportProgress(50);
            this.WriteCsv(baseName);
            BgWorker.ReportProgress(75);
            this.WritePdf(baseName + "result.pdf");
            BgWorker.ReportProgress(100);
            
        }

        #region Compare

        public ComparisonResult Compare() {
            /*if(!conn1.DatabaseExists(conn1.DbConfig.DbName + "_system"))
                throw new Exception("Die Datenbank \"" + conn1.DbConfig.DbName + "_system\" existiert nicht");
            if(!conn2.DatabaseExists(conn2.DbConfig.DbName + "_system"))
                throw new Exception("Die Datenbank \"" + conn2.DbConfig.DbName + "_system\" existiert nicht");*/
            List<ComparisonResult.TableInfo>[] tables = new List<ComparisonResult.TableInfo>[2];

            IDatabase[] conn = new IDatabase[2] { conn1, conn2 };
            for (int i = 0; i < 2; ++i) {
                tables[i] = new List<ComparisonResult.TableInfo>();
                //string fullDbName = conn[i].DbConfig.DbName + "_system";
                if (conn[i].DatabaseExists(conn[i].DbConfig.DbName + "_system"))
                    info[i] = new DatabaseSystemExists(conn[i]);
                else
                    info[i] = new DatabaseNoSystem(conn[i]);

                info[i].GetTables(tables[i]);
                result.TableCount[i] = tables[i].Count;
                result.EmptyTablesCount[i] = 0;
                foreach (var table in tables[i])
                    if (table.RowCount == 0) result.EmptyTablesCount[i]++;

                /*string sql = "SELECT name,row_count FROM " + fullDbName + "." + conn[i].Enquote("table");
                using (DbDataReader reader = conn[i].ExecuteReader(sql)) {
                    int count = 0;
                    result.EmptyTablesCount[i] = 0;
                    while (reader.Read()) {
                        tables[i].Add(
                            new ComparisonResult.TableInfo(reader["name"].ToString(), Convert.ToInt64(reader["row_count"]))
                            );
                        if (tables[i][tables[i].Count - 1].RowCount == 0) result.EmptyTablesCount[i]++;
                        count++;
                    }
                    result.TableCount[i] = count;
                }*/
            }
            BgWorker.ReportProgress(10);
            CompareTableNames(tables[0], tables[1]);
            BgWorker.ReportProgress(20);
            CompareRowCountsAndColumns(tables[0], tables[1]);
            BgWorker.ReportProgress(50);
            return result;
        }
        #endregion Compare

        #region GetTableListFromNames
        List<ComparisonResult.TableInfoMismatch> GetTableListFromNames(List<ComparisonResult.TableInfo> list, int indexOther) {
            List<ComparisonResult.TableInfoMismatch> resultList = new List<ComparisonResult.TableInfoMismatch>();
            foreach(var table in list){
                resultList.Add(new ComparisonResult.TableInfoMismatch(table.Name, table.RowCount));
                result.MissingTablesDict[table.Name] = true;
            }
            return resultList;
        }
        #endregion GetTableListFromNames

        ComparisonResult.TableInfoMismatch GetTableInfoFromName(string name, int indexOther) {
            return new ComparisonResult.TableInfoMismatch(name, 0);
        }

        private void CompareTableNames(List<ComparisonResult.TableInfo> tables1, List<ComparisonResult.TableInfo> tables2) {
            List<ComparisonResult.TableInfo> tablesNotIn1 = tables2.Except(tables1, new ComparisonResult.TableInfoComparer()).ToList();
            tablesNotIn1.Sort(delegate(ComparisonResult.TableInfo t1, ComparisonResult.TableInfo t2) { return t1.Name.CompareTo(t2.Name); });
            List<ComparisonResult.TableInfo> tablesNotIn2 = tables1.Except(tables2, new ComparisonResult.TableInfoComparer()).ToList();
            tablesNotIn2.Sort(delegate(ComparisonResult.TableInfo t1, ComparisonResult.TableInfo t2) { return t1.Name.CompareTo(t2.Name); });

            result.MissingTables[0] = GetTableListFromNames(tablesNotIn1, 1);
            result.MissingTables[1] = GetTableListFromNames(tablesNotIn2, 0);
            
        }

        private void CompareRowCountsAndColumns(List<ComparisonResult.TableInfo> tables1, List<ComparisonResult.TableInfo> tables2) {
            List<ComparisonResult.TableInfo> tablesInBoth = tables1.Intersect(tables2, new ComparisonResult.TableInfoComparer()).ToList();
            tablesInBoth.Sort(delegate(ComparisonResult.TableInfo t1, ComparisonResult.TableInfo t2) { return t1.Name.CompareTo(t2.Name); });
            List<ComparisonResult.TableInfo>[] tables = new List<ComparisonResult.TableInfo>[2] { tables1, tables2 };
            foreach (var table in tablesInBoth) {
                long count1 = 0, count2 = 0;
                foreach(var table1 in tables1)
                    if (table1.Name == table.Name) { count1 = table1.RowCount; break; }
                foreach (var table2 in tables2)
                    if (table2.Name == table.Name) { count2 = table2.RowCount; break; }

                if (count1 == 0 && count2 != 0)
                    result.EmptyTablesInOne[0].Add(new ComparisonResult.TableInfoMismatch(table.Name, count2));
                if (count1 != 0 && count2 == 0)
                    result.EmptyTablesInOne[1].Add(new ComparisonResult.TableInfoMismatch(table.Name, count1));
            }

            List<ComparisonResult.ColumnInfo>[] columns = new List<ComparisonResult.ColumnInfo>[2];
            //Dictionary<string, ComparisonResult.ColumnInfo>[] columnsDict = new Dictionary<string, ComparisonResult.ColumnInfo>[2];
            Dictionary<string, List<ComparisonResult.ColumnInfo>>[] columnsDict = new Dictionary<string, List<ComparisonResult.ColumnInfo>>[2];
            IDatabase[]conn = new IDatabase[2]{conn1,conn2};
            for (int i = 0; i < 2; ++i) {
                columns[i] = new List<ComparisonResult.ColumnInfo>();
                columnsDict[i] = new Dictionary<string, List<ComparisonResult.ColumnInfo>>();

                info[i].GetColumns(columns[i], columnsDict[i], tables[i]);

                result.ColumnCount[i] = columns[i].Count;

                foreach (var column in columns[i]) {
                    if (!result.MissingTableToColumns.ContainsKey(column.TableName)) result.MissingTableToColumns[column.TableName] = new List<ComparisonResult.ColumnInfo>();
                    result.MissingTableToColumns[column.TableName].Add(column);
                }
                //string fullDbName = conn[i].DbConfig.DbName + "_system";
                //string sql = "SELECT c.name,t.name,c.type FROM " +
                //    fullDbName + "." + conn[i].Enquote("table") + " t," +
                //    fullDbName + "." + conn[i].Enquote("col") + " c" +
                //    " WHERE t.table_id=c.table_id";

                
                //using (DbDataReader reader = conn[i].ExecuteReader(sql)) {
                //    int colCount = 0;
                //    while (reader.Read()) {
                        
                //        ComparisonResult.ColumnInfo column = new ComparisonResult.ColumnInfo(reader[0].ToString(), reader[1].ToString(), reader[2].ToString());
                //        columns[i].Add(column);
                        
                //        if (!columnsDict[i].ContainsKey(column.TableName)) columnsDict[i][column.TableName] = new List<ComparisonResult.ColumnInfo>();
                //        columnsDict[i][column.TableName].Add(column);
                //        colCount++;

                //        if (!result.MissingTableToColumns.ContainsKey(column.TableName)) result.MissingTableToColumns[column.TableName] = new List<ComparisonResult.ColumnInfo>();
                //        result.MissingTableToColumns[column.TableName].Add(column);
                //    }
                //    result.ColumnCount[i] = colCount;
                //}
            }

            AddColumnMismatch(0, tablesInBoth, columnsDict);
            AddColumnMismatch(1, tablesInBoth, columnsDict);                
        }

        private void AddColumnMismatch(int which, List<ComparisonResult.TableInfo> tablesInBoth, Dictionary<string, List<ComparisonResult.ColumnInfo>>[] columnsDict) {
            foreach (var table in tablesInBoth) {
                if (columnsDict[which].ContainsKey(table.Name)) {
                    List<ComparisonResult.ColumnInfo> columnsOwn = columnsDict[which][table.Name];
                    List<ComparisonResult.ColumnInfo> columnsOther = columnsDict[1 - which][table.Name];
                    foreach(var column in columnsOwn){
                        bool found = false;
                        foreach (var columnOther in columnsOther) {
                            if (column.Name == columnOther.Name) {
                                if(which == 0 && column.DataType != columnOther.DataType){
                                    if (!result.ColumnMismatchSingle.ContainsKey(table.Name)) result.ColumnMismatchSingle[table.Name] = new List<ComparisonResult.ColumnInfoMismatch>();
                                    ComparisonResult.ColumnInfoMismatch mismatch = new ComparisonResult.ColumnInfoMismatch(column.Name, ComparisonResult.ColumnInfoMismatch.MismatchType.TypeMismatch, column.DataType, columnOther.DataType, 1-which);
                                    result.ColumnMismatchSingle[table.Name].Add(mismatch);
                                }
                                found = true; break;
                            }
                        }
                        if (!found) {
                            if (!result.ColumnMismatchSingle.ContainsKey(table.Name)) result.ColumnMismatchSingle[table.Name] = new List<ComparisonResult.ColumnInfoMismatch>();
                            ComparisonResult.ColumnInfoMismatch mismatch = new ComparisonResult.ColumnInfoMismatch(column.Name, ComparisonResult.ColumnInfoMismatch.MismatchType.ColumnDoesNotExist, column.DataType, "", 1-which);
                            result.ColumnMismatchSingle[table.Name].Add(mismatch);
                        }
                    }
                }
            }
        }

        #region WriteCsv
        public void WriteCsv(string baseName) {
            string db1 = result.DbName[0];
            string db2 = result.DbName[1];
            if (db1 == db2) {
                db1 = result.Hostname1 + "." + db1;
                db2 = result.Hostname2 + "." + db2;
            }
            string []dbName = new string[]{db1,db2};

            for(int i = 0; i < 2; ++i){
                using (StreamWriter writer = new StreamWriter(baseName + "missing_tables_" + dbName[i] + ".csv")) {
                    writer.WriteLine("Tabellenname;Zeilen in anderer DB (" + dbName[1 - i] + ")");
                    foreach (var table in result.MissingTables[i])
                        writer.WriteLine(table.Name + ";" + table.CountOther);
                }
                using (StreamWriter writer = new StreamWriter(baseName + "empty_tables_" + dbName[i] + ".csv")) {
                    writer.WriteLine("Tabellenname;Zeilen in anderer DB (" + dbName[1 - i] + ")");
                    foreach (var table in result.EmptyTablesInOne[i])
                        writer.WriteLine(table.Name + ";" + table.CountOther);
                }

                using (StreamWriter writer = new StreamWriter(baseName + "column_errors_per_table.csv")) {
                    writer.WriteLine("Tabellenname; Spaltenname; Beschreibung");
                    foreach (var pair in result.ColumnMismatchSingle) {
                        foreach (var columnInfo in pair.Value) {
                            string description = "";
                            if (columnInfo.Mismatch == ComparisonResult.ColumnInfoMismatch.MismatchType.ColumnDoesNotExist)
                                description = "fehlt in " + dbName[columnInfo.Db];
                            else
                                description = "Datentyp anders: " + columnInfo.DataTypeOwn + "/" + columnInfo.DataTypeOther;
                            writer.WriteLine(pair.Key + ";" + columnInfo.Name + ";" + description);
                        }
                    }
                }
            }
        }
        #endregion WriteCsv

        public void WritePdf(string filename) {
            ComparisonResultPdf.WritePdf(result, filename);
        }

    }
}
