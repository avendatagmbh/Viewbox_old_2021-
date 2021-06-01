using System.Windows.Media;
using DbSearch.Manager;
using DbSearchLogic.SearchCore.Result;
using DbSearchLogic.Structures.TableRelated;
using Utils;

namespace DbSearch.Models.Result {
    public class TableMappingsModel {

        #region TableMappingsModel
        public TableMappingsModel(Query query) {
            ColumnMappings = query.UserColumnMappings.ColumnMappings;
            _userColumnMappings = query.UserColumnMappings;
        }
        #endregion TableMappingsModel

        #region Properties
        private UserColumnMappings _userColumnMappings;
        public ObservableCollectionAsync<ColumnMapping> ColumnMappings { get; set; }
        #endregion
    }
}
