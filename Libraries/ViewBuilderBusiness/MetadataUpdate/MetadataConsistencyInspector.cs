using System;
using System.Collections.Generic;
using System.Linq;
using SystemDb;
using SystemDb.Internal;
using DbAccess;
using DbAccess.Structures;
using ViewBuilderBusiness.Structures.Config;

namespace ViewBuilderBusiness.MetadataUpdate
{
    public class MetadataConsistencyInspector
    {
        #region Fields

        private readonly SystemDb.SystemDb _metadataDb;
        private readonly ProfileConfig _profileConfig;
        private IList<string> _customerTables;
        private List<IssueExtension> _issueExtensions;
        private IList<string> _metadataTables;

        #endregion

        #region Ctor

        public MetadataConsistencyInspector(ProfileConfig profileConfig)
        {
            _profileConfig = profileConfig;
            _metadataDb = _profileConfig.ViewboxDb;
        }

        #endregion

        #region Event and triggers

        public event EventHandler<System.EventArgs> Initializing;
        public event EventHandler<System.EventArgs> InitializationComplete;
        public event EventHandler<System.EventArgs> CollectMissingTables;
        public event EventHandler<System.EventArgs> CollectMissingTablesComplete;
        public event EventHandler<System.EventArgs> CompareComplete;
        public event EventHandler<InspectTablesEventArgs> InspectingTables;
        public event EventHandler<InspectTableEventArgs> InspectingTable;
        public event EventHandler<System.EventArgs> InspectingTablesComplete;

        public void OnInitializing()
        {
            if (Initializing != null) Initializing(this, System.EventArgs.Empty);
        }

        public void OnInitializationComplete()
        {
            if (InitializationComplete != null) InitializationComplete(this, System.EventArgs.Empty);
        }

        public void OnCollectMissingTables()
        {
            if (CollectMissingTables != null) CollectMissingTables(this, System.EventArgs.Empty);
        }

        public void OnCollectMissingTablesComplete()
        {
            if (CollectMissingTablesComplete != null) CollectMissingTablesComplete(this, System.EventArgs.Empty);
        }

        public void OnInspectingTable(string table)
        {
            if (InspectingTable != null) InspectingTable(this, new InspectTableEventArgs(table));
        }

        public void OnInspectingTables(int count)
        {
            if (InspectingTables != null) InspectingTables(this, new InspectTablesEventArgs(count));
        }

        public void OnInspectingTablesComplete()
        {
            if (InspectingTablesComplete != null) InspectingTablesComplete(this, System.EventArgs.Empty);
        }

        public void OnCompareComplete()
        {
            if (CompareComplete != null) CompareComplete(this, System.EventArgs.Empty);
        }

        #endregion

        #region Logic

        public IList<TableDifference> CompareTableInformation()
        {
            using (var customerDb = _profileConfig.ConnectionManager.GetConnection())
            {
                customerDb.SetHighTimeout();
                InitTableLists(customerDb);
                List<string> tablesToInspect;
                var tableDifferences = CollectMissingTableDifferences(out tablesToInspect);

                OnInspectingTables(tablesToInspect.Count);
                foreach (var table in tablesToInspect)
                {
                    OnInspectingTable(table);
                    var difference = InspectTable(customerDb, table);
                    if (difference.DifferenceType != TableDifference.Type.NoDifferences)
                    {
                        tableDifferences.Add(difference);
                    }
                }
                OnInspectingTablesComplete();
                OnCompareComplete();
                return tableDifferences;
            }
        }

        private IList<TableDifference> CollectMissingTableDifferences(out List<string> tablesToInspect)
        {
            OnCollectMissingTables();
            var missingTables = _metadataTables
                .Except(_customerTables).ToList();
            tablesToInspect = _metadataTables.Except(missingTables).ToList();

            var tableDifferences = missingTables
                .Where(table => !IsFilterTable(table))
                .Select(table => new TableDifference(table, TableDifference.Type.MissingTable)).ToList();
            OnCollectMissingTablesComplete();
            return tableDifferences;
        }

        private bool IsFilterTable(string table)
        {
            // Issue => TableType == 3			
            var issue = _metadataDb.Issues.FirstOrDefault(i => i.TableName == table);
            return issue != null && _issueExtensions.Any(ie => ie.RefId == issue.Id && ie.TableObjectId != 0);
        }

        private TableDifference InspectTable(IDatabase customerDb, string table)
        {
            var tableDifference = new TableDifference(table, TableDifference.Type.NoDifferences);
            IList<DbColumnInfo> customerColumns = customerDb.GetColumnInfos(table);
            IList<IColumn> metadataColumns = _metadataDb.Columns.Where(c => c.Table.TableName == table).ToList();
            foreach (IColumn metadataColumn in metadataColumns)
            {
                var customerColumnsWithSameName = customerColumns.Where(
                    customerColumn =>
                    customerColumn.Name.Equals(metadataColumn.Name, StringComparison.InvariantCultureIgnoreCase)
                    ).ToList();
                if (customerColumnsWithSameName.Count == 0 && IsNotFakeColumn(metadataColumn))
                {
                    tableDifference.ColumnDifferences.Add(
                        new ColumnDifference(table, metadataColumn.Name, ColumnDifference.Type.MissingColumn));
                }
                else if (customerColumnsWithSameName.Count > 1)
                {
                    tableDifference.ColumnDifferences.Add(
                        new ColumnDifference(table, metadataColumn.Name, ColumnDifference.Type.MultipleColumns));
                }
                else
                {
                    var customerColumn = customerColumnsWithSameName[0];
                    var columnDifference = GetDifference(customerColumn, metadataColumn);
                    if (columnDifference != ColumnDifference.Type.NoDifference)
                    {
                        tableDifference.ColumnDifferences.Add(
                            new ColumnDifference(table, metadataColumn.Name, columnDifference)
                                {
                                    CustomerColumn = customerColumn,
                                    MetadataColumn = metadataColumn
                                });
                    }
                }
            }
            if (tableDifference.ColumnDifferences.Any())
            {
                tableDifference.DifferenceType = TableDifference.Type.ColumnDifferences;
            }
            return tableDifference;
        }

        private bool IsNotFakeColumn(IColumn metadataColumn)
        {
            bool isFakeColumn = metadataColumn.IsEmpty || metadataColumn.IsOneDistinct;
            return !isFakeColumn;
        }

        private ColumnDifference.Type GetDifference(DbColumnInfo customerColumn, IColumn metadataColumn)
        {
            var dataTypeOfCustomerColumn = Parse(customerColumn.OriginalType);
            var dataTypeOfMetadataColumn = Parse(metadataColumn.DataType);

            if (dataTypeOfCustomerColumn == dataTypeOfMetadataColumn)
            {
                return ColumnDifference.Type.NoDifference;
            }
            else
            {
                if (dataTypeOfCustomerColumn == DataType.Text)
                {
                    return ColumnDifference.Type.DataTypeCollisionText;
                }
                if (dataTypeOfCustomerColumn == DataType.Binary)
                {
                    return metadataColumn.IsVisible
                               ? ColumnDifference.Type.DataTypeCollisionBinary
                               : ColumnDifference.Type.NoDifference;
                }
                if (dataTypeOfCustomerColumn == DataType.Double)
                {
                    return customerColumn.NumericScale > 0 && metadataColumn.MaxLength > 0 &&
                           customerColumn.NumericScale == metadataColumn.MaxLength
                               ? ColumnDifference.Type.NoDifference
                               : ColumnDifference.Type.DataTypeCollisionDouble;
                }
                return ColumnDifference.Type.DataTypeCollision;
            }
        }

        private DataType Parse(SqlType sqlType)
        {
            switch (sqlType)
            {
                case SqlType.String:
                    return DataType.String;
                case SqlType.Integer:
                    return DataType.Integer;
                case SqlType.Decimal:
                    return DataType.Decimal;
                case SqlType.Numeric:
                    return DataType.Numeric;
                case SqlType.Date:
                    return DataType.Date;
                case SqlType.Time:
                    return DataType.Time;
                case SqlType.DateTime:
                    return DataType.DateTime;
                case SqlType.Boolean:
                    return DataType.Boolean;
                default:
                    throw new ArgumentOutOfRangeException("sqlType");
            }
        }

        private DataType Parse(string sqlTypeAsString)
        {
            var str = sqlTypeAsString.ToLower();
            if (str.StartsWith("char") ||
                str.StartsWith("varchar"))
            {
                return DataType.String;
            }
            if (str.StartsWith("datetime"))
            {
                return DataType.DateTime;
            }
            if (str.StartsWith("date"))
            {
                return DataType.Date;
            }
            if (str.StartsWith("time"))
            {
                return DataType.Time;
            }
            if (str.StartsWith("bigint") ||
                str.StartsWith("int") ||
                str.StartsWith("smallint") ||
                str.StartsWith("tinyint"))
            {
                return DataType.Integer;
            }
            if (str.StartsWith("decimal") ||
                str.StartsWith("float"))
            {
                return DataType.Decimal;
            }
            //
            // The special cases:
            //
            if (str.StartsWith("blob") ||
                str.StartsWith("tinyblob") ||
                str.StartsWith("binary") ||
                str.StartsWith("longblob") ||
                str.StartsWith("varbinary") ||
                str.StartsWith("mediumblob"))
            {
                return DataType.Binary;
            }
            if (str.StartsWith("longtext") ||
                str.StartsWith("mediumtext") ||
                str.StartsWith("text"))
            {
                return DataType.Text;
            }
            if (str.StartsWith("double"))
            {
                return DataType.Double;
            }
            return DataType.Unknown;
        }

        #region Init

        private void InitTableLists(IDatabase customerDb)
        {
            OnInitializing();
            _customerTables = customerDb.GetTableList();
            _metadataTables = GetAllMetadataTable();
            _issueExtensions = GetIssueExtensions();
            OnInitializationComplete();
        }

        private IList<string> GetAllMetadataTable()
        {
            var tables = _metadataDb.Tables.Select(table => table.TableName).ToList();
            var views = _metadataDb.Views.Select(view => view.TableName).ToList();
            var issues = _metadataDb.Issues.Select(issue => issue.TableName).ToList();
            var fakes = _metadataDb.GetFakeTables(_profileConfig.ViewboxDbName);
            tables.AddRange(views);
            tables.AddRange(issues);
            tables.AddRange(fakes);
            return tables;
        }

        private List<IssueExtension> GetIssueExtensions()
        {
            using (var db = _metadataDb.ConnectionManager.GetConnection())
            {
                return db.DbMapping.Load<IssueExtension>();
            }
        }

        #endregion

        internal enum DataType
        {
            String,
            DateTime,
            Date,
            Time,
            Integer,
            Decimal,
            Numeric,
            Boolean,
            Binary,
            Text,
            Double,
            Unknown
        }

        #endregion
    }
}