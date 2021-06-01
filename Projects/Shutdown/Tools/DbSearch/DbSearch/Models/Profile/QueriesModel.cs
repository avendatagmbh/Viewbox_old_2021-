using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using DbSearchLogic.Structures.TableRelated;
using Utils;

namespace DbSearch.Models.Profile {
    public class QueriesModel : NotifyPropertyChangedBase{
        public QueriesModel(MainWindowModel mainWindowModel) {
            mainWindowModel.PropertyChanged += mainWindowModel_PropertyChanged;
            _mainWindowModel = mainWindowModel;
        }

        #region EventHandler
        void mainWindowModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == "SelectedProfile" || e.PropertyName == "SelectedQuery") {
                DbSearchLogic.Config.Profile selectedProfile = ((MainWindowModel) sender).SelectedProfile;
                if (selectedProfile == null) Queries = null;
                else {
                    Queries = selectedProfile.Queries.Items;
                    _queries = selectedProfile.Queries;
                }

                OnPropertyChanged("Queries");
            }
        }
        #endregion EventHandler

        #region Properties
        private Queries _queries;
        public ObservableCollection<Query> Queries { get; set; }
        private MainWindowModel _mainWindowModel { get; set; }
        #endregion Properties

        public void DeleteQueries(IEnumerable<Query> queriesToDelete) {
            if (queriesToDelete == null) return;
            _queries.Delete(queriesToDelete);
            if (_mainWindowModel.SelectedQuery != null && queriesToDelete.Contains(_mainWindowModel.SelectedQuery)) {
                _mainWindowModel.SelectedQuery = null;
            }
        }
    }
}
