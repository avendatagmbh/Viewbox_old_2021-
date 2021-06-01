using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using DbSearchDatabase.Factories;
using DbSearchDatabase.Interfaces;
using DbSearchDatabase.TableRelated;
using DbSearchLogic.Config;
using DbSearchLogic.SearchCore.Result;
using Utils;

namespace DbSearchLogic.Structures.TableRelated {
    public class Queries : INotifyPropertyChanged {
        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
        #endregion Events

        #region Constructor
        public Queries(Profile profile) {
            Profile = profile;
            DbQueries = DatabaseObjectFactory.CreateDbTables(profile.DbProfile);
            Items = new ObservableCollectionAsync<Query>();
        }
        #endregion

        #region Properties
        private Profile Profile { get; set; }
        private IDbQueries DbQueries { get; set; }
        public ObservableCollection<Query> Items { get; set; }
        private bool _loaded = false;
        //Need to store them in this dictionary, as there will be snapshots of queries, but all snapshots of a query should share the same user mappings
        private Dictionary<int, UserColumnMappings> _queryIdToUserColumnMappings = new Dictionary<int, UserColumnMappings>(); 
        #endregion Properties

        #region Methods
        private void AddQuery(Query query) {
            Items.Add(query);
            _queryIdToUserColumnMappings[query.DbQuery.Id] = new UserColumnMappings(query);
        }

        private void RemoveQuery(Query query) {
            Items.Remove(query);
            _queryIdToUserColumnMappings.Remove(query.DbQuery.Id);
        }
    
    

        public void AddFromValidationDatabase(List<ImportTable> tables) {
            List<IDbQuery> dbTables =new List<IDbQuery>();
            DbQueries.AddFromValidationDatabase(dbTables, tables);

            foreach (var dbTable in dbTables) {
                Query query = new Query(Profile, dbTable);
                AddQuery(query);
                query.OptimalRows(15);
            }
            OnPropertyChanged("Items");
        }

        public void Load() {
            if (_loaded) return;
            IEnumerable<IDbQuery> dbTables = DbQueries.Load();
            
            Items.Clear();
            foreach (var dbTable in dbTables) {
                AddQuery(new Query(Profile, dbTable));
            }
            _loaded = true;
        }

        public Query FindQuery(string name) {
            foreach (var query in Items)
                if (query.Name == name)
                    return query;
            return null;
        }

        public void Delete(IEnumerable<Query> queriesToDelete) {
            foreach(var query in queriesToDelete) {
                DbQueries.DeleteQuery(query.DbQuery);
                RemoveQuery(query);
            }
        }


        public UserColumnMappings GetUserColumnMappings(Query query) {
            UserColumnMappings userColumnMappings;
            if (!_queryIdToUserColumnMappings.TryGetValue(query.DbQuery.Id, out userColumnMappings)) {
                userColumnMappings = new UserColumnMappings(query);
                _queryIdToUserColumnMappings[query.DbQuery.Id] = userColumnMappings;
            }
            return userColumnMappings;
        }

        public void AddEmptyQuery(Profile profile, string name) {
            Query query = new Query(profile, DatabaseObjectFactory.CreateDbQuery(profile.DbProfile, name));
            AddQuery(query);
            //query.DbQuery.Save();
        }
        #endregion Methods
    }
}
