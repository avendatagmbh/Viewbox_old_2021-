using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DbSearch.Models.Search;
using DbSearchDatabase.TableRelated;
using DbSearchLogic.Structures.TableRelated;
using Utils;

namespace DbSearch.Models.Profile {
    public class FinishedLoadingTablesEventArgs : EventArgs {
        public DbSearchLogic.Config.Profile Profile { get; set; }
        public AggregateException Exception { get; private set; }
        public FinishedLoadingTablesEventArgs(DbSearchLogic.Config.Profile profile, AggregateException ex) {
            Profile = profile;
            Exception = ex;
        }
    }

    public class ProfileModel {
        public event EventHandler<FinishedLoadingTablesEventArgs> FinishedLoadingTablesEvent;
        protected void OnFinishedLoadingTablesEvent(object sender, FinishedLoadingTablesEventArgs data) {
            if (FinishedLoadingTablesEvent  != null) FinishedLoadingTablesEvent(this, data);  
        }  

        #region Constructor
        public ProfileModel(DbSearchLogic.Config.Profile profile) {
            Profile = profile;
            TableToSearchModel = new Dictionary<Query, SearchModel>();
        }
        #endregion

        #region Properties
        public DbSearchLogic.Config.Profile Profile { get; set; }
        private Dictionary<Query, SearchModel> TableToSearchModel { get; set; }
        #endregion

        public void ImportValidationTables(ObservableCollectionAsync<ImportTable> tables) {
            Task.Factory.StartNew(() => Profile.Queries.AddFromValidationDatabase(new List<ImportTable>(tables))).ContinueWith(tmp => FinishedLoadingTables(tmp));
        }

        private void FinishedLoadingTables(object tmp) {
            Task task = tmp as Task;
            if (task.Status == TaskStatus.Faulted) {
                OnFinishedLoadingTablesEvent(this, new FinishedLoadingTablesEventArgs(Profile, task.Exception));
            }
            else
                OnFinishedLoadingTablesEvent(this, new FinishedLoadingTablesEventArgs(Profile, null));
        }

        public SearchModel GetSearchModel(Query query) {
            if (query == null) return null;
            if(!TableToSearchModel.ContainsKey(query)) TableToSearchModel[query] = new SearchModel(query);
            return TableToSearchModel[query];
        }

        public void LoadQueries() {
            Task.Factory.StartNew(() => Profile.Queries.Load()).ContinueWith(tmp => FinishedLoadingTables(tmp));
        }
    }
}
