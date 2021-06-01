using System;
using System.Collections.Generic;
using System.Data;
using DbAccess;
using Utils;
using ViewBuilderBusiness.MetadataUpdate;
using ViewBuilderBusiness.Structures.Config;

namespace ViewBuilderBusiness.Optimizations
{
    public class OptimizationsInstaller
    {
        private readonly ProfileConfig _profileConfig;
        private IDatabase _database;
        private string _systemName;

        #region Event and triggers

        public event EventHandler<System.EventArgs> Clearing;
        public event EventHandler<System.EventArgs> ClearingComplete;
        public event EventHandler<System.EventArgs> Initializing;
        public event EventHandler<System.EventArgs> InitializingComplete;
        public event EventHandler<System.EventArgs> Installing;
        public event EventHandler<System.EventArgs> InstallingComplete;
        public event EventHandler<System.EventArgs> UpdatingMetadata;
        public event EventHandler<System.EventArgs> UpdatingMetadataComplete;
        public event EventHandler<ErrorEventArgs> ErrorOccurred;

        private void OnClearing()
        {
            if (Clearing != null) Clearing(this, System.EventArgs.Empty);
        }

        private void OnClearingComplete()
        {
            if (ClearingComplete != null) ClearingComplete(this, System.EventArgs.Empty);
        }

        private void OnInitializing()
        {
            if (Initializing != null) Initializing(this, System.EventArgs.Empty);
        }

        private void OnInitializingComplete()
        {
            if (InitializingComplete != null) InitializingComplete(this, System.EventArgs.Empty);
        }

        private void OnInstalling()
        {
            if (Installing != null) Installing(this, System.EventArgs.Empty);
        }

        private void OnInstallingComplete()
        {
            if (InstallingComplete != null) InstallingComplete(this, System.EventArgs.Empty);
        }

        private void OnUpdatingMetadata()
        {
            if (UpdatingMetadata != null) UpdatingMetadata(this, System.EventArgs.Empty);
        }

        private void OnUpdatingMetadataComplete()
        {
            if (UpdatingMetadataComplete != null) UpdatingMetadataComplete(this, System.EventArgs.Empty);
        }

        private void OnErrorOccurred(string error)
        {
            if (ErrorOccurred != null) ErrorOccurred(this, new ErrorEventArgs(error));
        }

        #endregion

        public OptimizationsInstaller(ProfileConfig profileConfig)
        {
            _profileConfig = profileConfig;
        }

        public void Install(string systemName)
        {
            OnInstalling();
            _systemName = systemName;
            using (_database = _profileConfig.ViewboxDb.ConnectionManager.GetConnection())
            {
                _database.SetHighTimeout();
                _database.BeginTransaction();
                try
                {
                    ClearTables();
                    InitializeTables();
                    OnUpdatingMetadata();
                    UpdateViewBoxMetadata.UpdateLanguages(_profileConfig, _database, null);
                    UpdateViewBoxMetadata.UpdateOptimizationTexts(_profileConfig, _database, null);
                    _database.CommitTransaction();
                    OnUpdatingMetadataComplete();
                    OnInstallingComplete();
                }
                catch (Exception exception)
                {
                    _database.RollbackTransaction();
                    OnErrorOccurred(exception.Message);
                }
            }
        }

        private void ClearTables()
        {
            OnClearing();
            var tablesToClear = new[]
                                    {
                                        "optimization_group_texts",
                                        "optimization_groups",
                                        "optimization_roles",
                                        "optimization_texts",
                                        "optimization_users",
                                        "optimizations"
                                    };
            foreach (var table in tablesToClear)
            {
                _database.ExecuteNonQuery(string.Format("DELETE FROM {0}", table));
            }
            OnClearingComplete();
        }

        private void InitializeTables()
        {
            OnInitializing();
            FillUpOptimizationGroups();
            FillUpOptimizations();
            OnInitializingComplete();
        }

        private void FillUpOptimizationGroups()
        {
            _database.ExecuteNonQuery("INSERT INTO optimization_groups (id, type) VALUES (1, 2)");
            _database.ExecuteNonQuery("INSERT INTO optimization_groups (id, type) VALUES (2, 5)");
            _database.ExecuteNonQuery("INSERT INTO optimization_groups (id, type) VALUES (3, 3)");
            _database.ExecuteNonQuery("INSERT INTO optimization_groups (id, type) VALUES (4, 4)");
        }

        private void FillUpOptimizations()
        {
            var orderAreas = GetOrderAreas();
            var optimizationTrees = ConvertToOptimizationTree(orderAreas);
            int pad = 0;
            foreach (var optimizationTree in optimizationTrees)
            {
                int count = 0;
                foreach (var node in optimizationTree)
                {
                    // TODO: WARNING! 
                    // In a web-based environment this is an sql-injection vulnerability!
                    // Use DbCommand.Parameters instead!
                    _database.ExecuteNonQuery(string.Format(
                        "INSERT INTO optimizations (id, parent_id, value, optimization_group_id)" +
                        "VALUES ({0}, {1}, {2}, {3})", node.Id + pad,
                        node.ParentId == 0 ? node.ParentId : node.ParentId + pad, GetDbValue(node.Value), node.Level));
                    count++;
                }
                pad += count;
            }
        }

        private object GetDbValue(string value)
        {
            return value == null ? "null" : string.Format("'{0}'", value);
        }

        private DataTable GetOrderAreas()
        {
            var reader = _database.ExecuteReader(
                @"SELECT DISTINCT index_value, split_value, sort_value, t.database
				FROM order_areas o
                    join tables t on t.id = o.table_id
				
				ORDER BY t.database, index_value, split_value, sort_value;");
            var table = new DataTable();
            table.Load(reader);
            return table;
        }

        private IEnumerable<OptimizationTree> ConvertToOptimizationTree(DataTable orderAreas)
        {
            var oldsystemname = "";
            OptimizationTree optimizationTree = null;
            foreach (DataRow row in orderAreas.Rows)
            {
                var systemName = row[3].ToString();
                if (String.IsNullOrEmpty(systemName))
                    systemName = _systemName;
                if (systemName != oldsystemname || optimizationTree == null)
                {
                    if (optimizationTree != null)
                        yield return optimizationTree;
                    optimizationTree = new OptimizationTree(systemName);
                    oldsystemname = systemName;
                }
                var clientCode = row[0].ToString();
                var companyCode = row[1] is DBNull ? null : row[1].ToString();
                var financialYear = row[2] is DBNull ? null : row[2].ToString();
                optimizationTree.Add(systemName, clientCode, 1, null);
                optimizationTree.Add(clientCode, companyCode, 2, null);
                optimizationTree.Add(companyCode, financialYear, 3, clientCode);
            }

            if (optimizationTree != null)
                yield return optimizationTree;
        }
    }
}