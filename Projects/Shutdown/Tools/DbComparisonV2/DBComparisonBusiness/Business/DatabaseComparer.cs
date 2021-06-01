using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using DbAccess;
using System.ComponentModel;
using System.Runtime.Serialization.Formatters.Binary;

namespace DBComparisonBusiness.Business
{
    #region ComparisonResult
    public class ComparisonResult : DBComparisonBusiness.Business.IComparisonResult
    {
        #region HelperClasses
        /// <summary>
        /// class that should contain all info resulting of both db
        /// </summary>
        public class DatabasesCommonInfo {
            TableInfoCollection _allTables;

            public TableInfoCollection AllDistinctTables
            {
                get {return _allTables; }
                set { _allTables = value; }
            }

        }
        public class TableInfoMismatch
        {
            public TableInfoMismatch(string name, long countOther)
            {
                this.Name = name;
                this.CountOther = countOther;
            }

            public string Name { get; set; }
            public long CountOther { get; set; }
        }

        // Custom comparer for the Product class
        public class TableInfoComparer : IEqualityComparer<TableInfo>
        {
            // Products are equal if their names and product numbers are equal.
            public bool Equals(TableInfo x, TableInfo y)
            {

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

            public int GetHashCode(TableInfo tableInfo)
            {
                //Check whether the object is null
                if (Object.ReferenceEquals(tableInfo, null)) return 0;

                return tableInfo.Name == null ? 0 : tableInfo.Name.GetHashCode();
            }

        }
        [Serializable]
        public class TableInfoCollection : List<ComparisonResult.TableInfo> , ICloneable
        {
            public TableInfoCollection() : base() { }
            public TableInfoCollection(int capacity) : base(capacity) { }
            public TableInfoCollection(IEnumerable<TableInfo> collection) : base(collection) { }
            public ComparisonResult.TableInfo this[string tableName]
            {
                get { return base.Find(ti => ti.Name == tableName); }
            }

            public object Clone()
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, this);
                    stream.Seek(0, SeekOrigin.Begin);
                    return formatter.Deserialize(stream);
                }
            }
        }
        [Serializable]
        public class TableInfo
        {
            #region fields
            private long _rowCount;
            private ColumnInfoCollection _columns = new ColumnInfoCollection();
            #endregion

            #region constructor
            public TableInfo(string name, long count)
            {
                this.Name = name;
                this.RowCount = count;
            }
            #endregion


            #region properties
            public string Name { get; set; }
            public ColumnInfoCollection Columns { get { return _columns; } set { _columns = value; } }
            public long RowCount
            {
                get { return _rowCount; }
                set
                {
                    _rowCount = value < 0 ? 0 : value;
                }
            }
            #endregion
        }
        [Serializable]
        public class ColumnInfoCollection : List<ComparisonResult.ColumnInfo>
        {
            public ColumnInfoCollection() : base() { }
            public ColumnInfoCollection(int capacity) : base(capacity) { }
            public ColumnInfoCollection(IEnumerable<ColumnInfo> collection) : base(collection) { }
            public ComparisonResult.ColumnInfo this[string ColumnName]
            {
                get { return base.Find(ci => ci.Name == ColumnName); }
            }

        }

        public class ColumnInfoComparer : IEqualityComparer<ColumnInfo>
        {
            // columns are equal if their name are equal
            public bool Equals(ColumnInfo x, ColumnInfo y)
            {
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

            public int GetHashCode(ColumnInfo columnInfo)
            {
                //Check whether the object is null
                if (Object.ReferenceEquals(columnInfo, null)) return 0;

                return columnInfo.Name == null ? 0 : columnInfo.Name.GetHashCode();
            }

        }
        [Serializable]
        public class ColumnInfo
        {
            public ColumnInfo(string name, string table, string dataType)
            {
                this.Name = name;
                this.TableName = table;
                this.DataType = dataType;

            }

            public string UniqueName { get { return TableName + "???" + Name; } }
            public string Name { get; set; }
            public string TableName { get; set; }
            public string DataType { get; set; }
        }

        public class ColumnInfoMismatch
        {
            public ColumnInfoMismatch(string name, MismatchType type, string dataTypeOther, string dataTypeOwn, int db)
            {
                this.Name = name;
                this.Mismatch = type;
                this.DataTypeOther = dataTypeOther;
                this.DataTypeOwn = dataTypeOwn;
                this.Db = db;
            }

            public string Name { get; set; }
            public enum MismatchType { ColumnDoesNotExist, TypeMismatch };
            public MismatchType Mismatch { get; set; }
            public string DataTypeOwn { get; set; }
            public string DataTypeOther { get; set; }
            public int Db { get; set; }
        }
        #endregion HelperClasses


        #region private fields

        private string[] _dbName = new string[2];
        private int[] _tableCount = new int[2];
        private int[] _columnCount = new int[2];
        private int[] _emptyTablesCount = new int[2];
        private SortedDictionary<string, List<ColumnInfo>> _missingTableToColumns = new SortedDictionary<string, List<ColumnInfo>>();
        private List<TableInfoMismatch>[] _missingTables = new List<TableInfoMismatch>[2];
        private SortedDictionary<string, bool> _missingTablesDict = new SortedDictionary<string, bool>();
        SortedDictionary<string, List<ColumnInfoMismatch>> _columnMismatchSingle = new SortedDictionary<string, List<ColumnInfoMismatch>>();
        private List<TableInfoMismatch>[] _emptyTables = new List<TableInfoMismatch>[2];
        private DatabasesCommonInfo _commonDbInfo;
        private ComparisonResult.TableInfoCollection[] _tablesArray;
        #endregion

        #region private methods
        /// <summary>
        /// creates a merged table array with both objects from both DB , table, columns etc ( like a new DB representation with all elements)
        /// </summary>
        public void UpdateCommonDbInfo()
        {
            _commonDbInfo = new DatabasesCommonInfo();
            // clone (to preserve original object from changes on columns) and merges all tables from both DB into one array (distinct table name) 
            _commonDbInfo.AllDistinctTables = new TableInfoCollection(((TableInfoCollection)_tablesArray[0].Clone()).Union((TableInfoCollection)_tablesArray[1].Clone(), new TableInfoComparer()));

            // updates tables of the merged array to represent all columns available regardless of wich DB there are from
            _commonDbInfo.AllDistinctTables.ForEach(t => 
            {
                var tableDb1 = _tablesArray[0].Find(ti => ti.Name == t.Name);
                var tableDb2 = _tablesArray[1].Find(ti => ti.Name == t.Name);

                // simply reassign the columns proerty of the AllDistinctTables tables with an distinct outer join of both tables
                if (tableDb1 != null && tableDb2 != null) {
                    t.Columns = new ColumnInfoCollection(tableDb1.Columns.Union(tableDb2.Columns, new ColumnInfoComparer()));
                } 
            });
            _commonDbInfo.AllDistinctTables.Sort((t1, t2) => t1.Name.CompareTo(t2.Name));
        }

        public void InitializeWithTables(TableInfoCollection[] tables)
        {
            if(tables.Length != 2 || tables[0] == null ||tables[1]== null) throw new ArgumentException("The TableInfoCollection[] parameter is empty or invalid.");

            _tablesArray = tables;
        }
        #endregion
        public List<TableInfoMismatch>[] MissingTables
        {
            get { return _missingTables; }
            set { _missingTables = value; }
        }
        //Tables which are only empty in one database
        public List<TableInfoMismatch>[] EmptyTablesInOne
        {
            get { return _emptyTables; }
            set { _emptyTables = value; }
        }
        public SortedDictionary<string, List<ColumnInfoMismatch>> ColumnMismatchSingle
        {
            get { return _columnMismatchSingle; }
            set { _columnMismatchSingle = value; }
        }
        public string[] DbName
        {
            get { return _dbName; }
            set { _dbName = value; }
        }

        public string Hostname1 { get; set; }
        public string Hostname2 { get; set; }


        public int[] TableCount
        {
            get { return _tableCount; }
            set { _tableCount = value; }
        }

        public int[] ColumnCount
        {
            get { return _columnCount; }
            set { _columnCount = value; }
        }


        public int[] EmptyTablesCount
        {
            get { return _emptyTablesCount; }
            set { _emptyTablesCount = value; }
        }

        public SortedDictionary<string, List<ColumnInfo>> MissingTableToColumns
        {
            get { return _missingTableToColumns; }
            set { _missingTableToColumns = value; }
        }


        public SortedDictionary<string, bool> MissingTablesDict
        {
            get { return _missingTablesDict; }
            set { _missingTablesDict = value; }
        }

        public DatabasesCommonInfo Common { get { return _commonDbInfo; } }
        public ComparisonResult.TableInfoCollection[] Tables 
        {
            get { return _tablesArray; } 
            set 
            { 
                _tablesArray = value;
            } 
        }

    }
    #endregion ComparisonResult



    public class DatabaseComparer
    {
        private ComparisonResult _result;
        private IDatabaseInformation[] info = new IDatabaseInformation[2];
        private IDatabase _db1;
        private IDatabase _db2;
        private BackgroundWorker _bgWorker;

        #region Constructor
        public DatabaseComparer(IDatabase db1, IDatabase db2, BackgroundWorker bgWorker)
        {
            this._db1 = db1;
            this._db2 = db2;
            this._bgWorker = bgWorker;

            _result = new ComparisonResult()
            {
                Hostname1 = db1.DbConfig.Hostname,
                Hostname2 = db2.DbConfig.Hostname
            };
            _result.DbName[0] = db1.DbConfig.DbName;
            _result.DbName[1] = db2.DbConfig.DbName;

            for (int i = 0; i < 2; ++i)
            {
                _result.MissingTables[i] = new List<ComparisonResult.TableInfoMismatch>();
                _result.EmptyTablesInOne[i] = new List<ComparisonResult.TableInfoMismatch>();
            }
        }
        #endregion

        #region public methods
        public ComparisonResult GetCreateResult()
        {

            ComparisonResult.TableInfoCollection[] tables = new ComparisonResult.TableInfoCollection[2];

            IDatabase[] dbArray = new IDatabase[2] { _db1, _db2 };
            for (int i = 0; i < 2; ++i)
            {
                tables[i] = new ComparisonResult.TableInfoCollection();

                if (dbArray[i].DatabaseExists(dbArray[i].DbConfig.DbName + "_system"))
                    info[i] = new DatabaseSystemExists(dbArray[i]);
                else
                    info[i] = new DatabaseNoSystem(dbArray[i]);

                info[i].GetTables(tables[i]);
                _result.TableCount[i] = tables[i].Count;
                _result.EmptyTablesCount[i] = 0;
                foreach (var table in tables[i])
                    if (table.RowCount == 0) _result.EmptyTablesCount[i]++;
            }

            // TODO: this code should be refactored as well as the current function
            // compare logic should be called from the result initialization 
            _result.InitializeWithTables(tables);

            _bgWorker.ReportProgress(10);
            CompareTableNames(tables[0], tables[1]);
            _bgWorker.ReportProgress(20);
            CompareRowCountsAndColumns(tables[0], tables[1]);

            _result.UpdateCommonDbInfo();
            _bgWorker.ReportProgress(50);
            return _result;
        }
        #endregion

        #region private methods
        List<ComparisonResult.TableInfoMismatch> GetTableListFromNames(List<ComparisonResult.TableInfo> list, int indexOther)
        {
            List<ComparisonResult.TableInfoMismatch> resultList = new List<ComparisonResult.TableInfoMismatch>();
            foreach (var table in list)
            {
                resultList.Add(new ComparisonResult.TableInfoMismatch(table.Name, table.RowCount));
                _result.MissingTablesDict[table.Name] = true;
            }
            return resultList;
        }

        ComparisonResult.TableInfoMismatch GetTableInfoFromName(string name, int indexOther)
        {
            return new ComparisonResult.TableInfoMismatch(name, 0);
        }

        private void CompareTableNames(List<ComparisonResult.TableInfo> tables1, List<ComparisonResult.TableInfo> tables2)
        {
            List<ComparisonResult.TableInfo> tablesNotIn1 = tables2.Except(tables1, new ComparisonResult.TableInfoComparer()).ToList();
            tablesNotIn1.Sort(delegate(ComparisonResult.TableInfo t1, ComparisonResult.TableInfo t2) { return t1.Name.CompareTo(t2.Name); });
            List<ComparisonResult.TableInfo> tablesNotIn2 = tables1.Except(tables2, new ComparisonResult.TableInfoComparer()).ToList();
            tablesNotIn2.Sort(delegate(ComparisonResult.TableInfo t1, ComparisonResult.TableInfo t2) { return t1.Name.CompareTo(t2.Name); });

            _result.MissingTables[0] = GetTableListFromNames(tablesNotIn1, 1);
            _result.MissingTables[1] = GetTableListFromNames(tablesNotIn2, 0);

        }

        private void CompareRowCountsAndColumns(ComparisonResult.TableInfoCollection tables1, ComparisonResult.TableInfoCollection tables2)
        {
            List<ComparisonResult.TableInfo> tablesInBoth = tables1.Intersect(tables2, new ComparisonResult.TableInfoComparer()).ToList();
            tablesInBoth.Sort(delegate(ComparisonResult.TableInfo t1, ComparisonResult.TableInfo t2) { return t1.Name.CompareTo(t2.Name); });
            foreach (var table in tablesInBoth)
            {
                long count1 = 0, count2 = 0;
                foreach (var table1 in tables1)
                    if (table1.Name == table.Name) { count1 = table1.RowCount; break; }
                foreach (var table2 in tables2)
                    if (table2.Name == table.Name) { count2 = table2.RowCount; break; }

                if (count1 == 0 && count2 != 0)
                    _result.EmptyTablesInOne[0].Add(new ComparisonResult.TableInfoMismatch(table.Name, count2));
                if (count1 != 0 && count2 == 0)
                    _result.EmptyTablesInOne[1].Add(new ComparisonResult.TableInfoMismatch(table.Name, count1));
            }

            List<ComparisonResult.ColumnInfo>[] columns = new List<ComparisonResult.ColumnInfo>[2];
            //Dictionary<string, ComparisonResult.ColumnInfo>[] columnsDict = new Dictionary<string, ComparisonResult.ColumnInfo>[2];
            Dictionary<string, List<ComparisonResult.ColumnInfo>>[] columnsDict = new Dictionary<string, List<ComparisonResult.ColumnInfo>>[2];
            IDatabase[] conn = new IDatabase[2] { _db1, _db2 };
            for (int i = 0; i < 2; ++i)
            {
                columns[i] = new List<ComparisonResult.ColumnInfo>();
                columnsDict[i] = new Dictionary<string, List<ComparisonResult.ColumnInfo>>();

                info[i].GetColumns(columns[i], columnsDict[i], _result.Tables[i]);
                
                _result.ColumnCount[i] = columns[i].Count;

                foreach (var column in columns[i])
                {
                    if (!_result.MissingTableToColumns.ContainsKey(column.TableName))
                    {
                        _result.MissingTableToColumns[column.TableName] = new List<ComparisonResult.ColumnInfo>();
                    }
                    _result.MissingTableToColumns[column.TableName].Add(column);
                }
            }

            AddColumnMismatch(0, tablesInBoth, columnsDict);
            AddColumnMismatch(1, tablesInBoth, columnsDict);
        }

        private void AddColumnMismatch(int which, List<ComparisonResult.TableInfo> tablesInBoth, Dictionary<string, List<ComparisonResult.ColumnInfo>>[] columnsDict)
        {
            foreach (var table in tablesInBoth)
            {
                if (columnsDict[which].ContainsKey(table.Name))
                {
                    List<ComparisonResult.ColumnInfo> columnsOwn = columnsDict[which][table.Name];
                    List<ComparisonResult.ColumnInfo> columnsOther = columnsDict[1 - which][table.Name];
                    foreach (var column in columnsOwn)
                    {
                        bool found = false;
                        foreach (var columnOther in columnsOther)
                        {
                            if (column.Name == columnOther.Name)
                            {
                                if (which == 0 && column.DataType != columnOther.DataType)
                                {
                                    if (!_result.ColumnMismatchSingle.ContainsKey(table.Name)) _result.ColumnMismatchSingle[table.Name] = new List<ComparisonResult.ColumnInfoMismatch>();
                                    ComparisonResult.ColumnInfoMismatch mismatch = new ComparisonResult.ColumnInfoMismatch(column.Name, ComparisonResult.ColumnInfoMismatch.MismatchType.TypeMismatch, column.DataType, columnOther.DataType, 1 - which);
                                    _result.ColumnMismatchSingle[table.Name].Add(mismatch);
                                }
                                found = true; break;
                            }
                        }
                        if (!found)
                        {
                            if (!_result.ColumnMismatchSingle.ContainsKey(table.Name)) _result.ColumnMismatchSingle[table.Name] = new List<ComparisonResult.ColumnInfoMismatch>();
                            ComparisonResult.ColumnInfoMismatch mismatch = new ComparisonResult.ColumnInfoMismatch(column.Name, ComparisonResult.ColumnInfoMismatch.MismatchType.ColumnDoesNotExist, column.DataType, "", 1 - which);
                            _result.ColumnMismatchSingle[table.Name].Add(mismatch);
                        }
                    }
                }
            }
        }
        #endregion

    }
}
