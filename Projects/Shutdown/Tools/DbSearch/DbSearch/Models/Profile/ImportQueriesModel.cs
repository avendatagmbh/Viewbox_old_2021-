using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;
using DbAccess.Structures;
using DbSearchDatabase.TableRelated;
using DbSearchLogic.SearchCore.Structures.Db;
using DbSearchLogic.Structures.TableRelated;
using Utils;


namespace DbSearch.Models.Profile {
    class ImportQueriesModel : NotifyPropertyChangedBase{
        public ImportQueriesModel(DbSearchLogic.Config.Profile profile) {
            Profile = profile;
            Tables = new ObservableCollectionAsync<ImportTable>();
            ValidationPaths = new ObservableCollectionAsync<string>();
        }

        #region Properties
        private DbSearchLogic.Config.Profile Profile { get; set; }
        public ObservableCollectionAsync<ImportTable> Tables { get; private set; }
        public ObservableCollectionAsync<string> ValidationPaths { get; private set; } 
        #endregion Properties

        public void AddValidationPath(string path) {
            if (!string.IsNullOrEmpty(path)) {
                using (IDatabase conn = ConnectionManager.CreateConnection(new DbConfig("Access") { Hostname = path })) {
                    conn.Open();
                    foreach (var table in conn.GetTableList()) {
                        Tables.Add(new ImportTable(table, path));
                    }
                }
                ValidationPaths.Add(path);
                OnPropertyChanged("Tables");
                OnPropertyChanged("ValidationPath");
            }
        }

        public void DeleteValidationPaths(IList items) {
            List<string> paths = new List<string>();
            foreach (string path in items) paths.Add(path);
            foreach (string path in paths) {
                ValidationPaths.Remove(path);
                
                for(int i = Tables.Count-1; i >= 0; --i)
                    if(Tables[i].ValidationPath == path)
                        Tables.RemoveAt(i);
            }   
        }
    }
}
