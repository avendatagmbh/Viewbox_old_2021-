using System;
using System.Collections.Generic;
using System.Linq;
using DbAccess;
using ViewBuilderBusiness.Structures.Config;

namespace ViewBuilderBusiness.MetadataUpdate
{
    public class MetadataUpdater
    {
        #region Fields

        private readonly SystemDb.SystemDb _metadataDb;
        private readonly ProfileConfig _profileConfig;

        #endregion

        #region Events and triggers

        public event EventHandler<ResolvingTableEventArgs> ResolvingTable;
        public event EventHandler<ResolvingTableEventArgs> ResolvingTableComplete;
        public event EventHandler<ResolvingColumnEventArgs> ResolvingColumn;
        public event EventHandler<ResolvingColumnEventArgs> ResolvingColumnComplete;

        private void OnResolvingTable(string tableName)
        {
            if (ResolvingTable != null) ResolvingTable(this, new ResolvingTableEventArgs(tableName));
        }

        private void OnResolvingTableComplete(string tableName)
        {
            if (ResolvingTableComplete != null)
                ResolvingTableComplete(this,
                                       new ResolvingTableEventArgs(tableName));
        }

        private void OnResolvingColumn(string tableName, string columnName)
        {
            if (ResolvingColumn != null) ResolvingColumn(this, new ResolvingColumnEventArgs(tableName, columnName));
        }

        private void OnResolvingColumnComplete(string tableName, string columnName)
        {
            if (ResolvingColumnComplete != null)
                ResolvingColumnComplete(this,
                                        new ResolvingColumnEventArgs(tableName, columnName));
        }

        private void OnResolvingColumnComplete(string tableName, string columnName, string error)
        {
            if (ResolvingColumnComplete != null)
                ResolvingColumnComplete(this,
                                        new ResolvingColumnEventArgs(tableName, columnName, error));
        }

        #endregion

        #region Ctor

        public MetadataUpdater(ProfileConfig profileConfig)
        {
            _profileConfig = profileConfig;
            _metadataDb = _profileConfig.ViewboxDb;
        }

        #endregion

        /// <summary>
        ///   Resolves table differences and gives back which could fully resolved.
        /// </summary>
        /// <param name="tableDifferences"> The list of table differences. </param>
        /// <returns> The list of fully resolved table differences. </returns>
        public IList<TableDifference> ResolveTableDifferences(IList<TableDifference> tableDifferences)
        {
            using (var db = _metadataDb.ConnectionManager.GetConnection())
            {
                return tableDifferences.Where(t => AllColumnDifferencesWereResolved(t, db)).ToList();
            }
        }

        private bool AllColumnDifferencesWereResolved(TableDifference tableDifference, IDatabase db)
        {
            OnResolvingTable(tableDifference.TableName);
            var resolvedColumnDifferences = ResolveColumnDifferences(tableDifference.ColumnDifferences, db);
            foreach (var resolvedColumnDifference in resolvedColumnDifferences)
                tableDifference.ColumnDifferences.Remove(resolvedColumnDifference);
            OnResolvingTableComplete(tableDifference.TableName);
            return tableDifference.ColumnDifferences.Count == 0;
        }

        private IEnumerable<ColumnDifference> ResolveColumnDifferences(IEnumerable<ColumnDifference> columnDifferences,
                                                                       IDatabase db)
        {
            return columnDifferences.Where(c => ColumnDifferenceWasResolved(c, db)).ToList();
        }

        /// <summary>
        ///   Resolves column differences and gives back which could fully resolved.
        /// </summary>
        /// <param name="columnDifferences"> The list of column differences. </param>
        /// <returns> The list of fully resolved column differences. </returns>
        public IList<ColumnDifference> ResolveColumnDifferences(IList<ColumnDifference> columnDifferences)
        {
            using (var db = _metadataDb.ConnectionManager.GetConnection())
            {
                return columnDifferences.Where(c => ColumnDifferenceWasResolved(c, db)).ToList();
            }
        }

        private bool ColumnDifferenceWasResolved(ColumnDifference columnDifference, IDatabase db)
        {
            OnResolvingColumn(columnDifference.TableName, columnDifference.ColumnName);
            try
            {
                switch (columnDifference.DifferenceType)
                {
                    case ColumnDifference.Type.DataTypeCollisionBinary:
                        ResolveBinaryCollision(columnDifference, db);
                        break;
                    case ColumnDifference.Type.DataTypeCollisionText:
                        ResolveTextCollision(columnDifference, db);
                        break;
                    case ColumnDifference.Type.DataTypeCollisionDouble:
                        ResolveDoubleCollision(columnDifference, db);
                        break;
                    default:
                        throw new NotImplementedException(string.Format(
                            "Resolving this type of collision ({0}) is not supported!", columnDifference.DifferenceType));
                }
                OnResolvingColumnComplete(columnDifference.TableName, columnDifference.ColumnName);
                return true;
            }
            catch (Exception exception)
            {
                OnResolvingColumnComplete(columnDifference.TableName, columnDifference.ColumnName, exception.Message);
                return false;
            }
        }

        private void ResolveDoubleCollision(ColumnDifference columnDifference, IDatabase db)
        {
            columnDifference.MetadataColumn.MaxLength = columnDifference.CustomerColumn.NumericScale;
            db.DbMapping.Save(columnDifference.MetadataColumn);
        }

        private void ResolveTextCollision(ColumnDifference columnDifference, IDatabase db)
        {
            throw new NotImplementedException(string.Format(
                "Resolving this type of collision ({0}) is not supported!", columnDifference.DifferenceType));
        }

        private void ResolveBinaryCollision(ColumnDifference columnDifference, IDatabase db)
        {
            columnDifference.MetadataColumn.IsVisible = false;
            db.DbMapping.Save(columnDifference.MetadataColumn);
        }
    }
}