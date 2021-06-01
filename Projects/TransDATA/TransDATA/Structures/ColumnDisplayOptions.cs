// --------------------------------------------------------------------------------
// author: Marcus Gerlach
// since: 2012-02-03
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using Config.Structures;
using Utils;

namespace TransDATA.Structures {
    public class ColumnDisplayOptions : NotifyPropertyChangedBase {
        private string _filter;
        private ColumnSortType _sortType;

        public string Filter {
            get { return _filter; }
            set {
                _filter = value;
                OnPropertyChanged("Filter");
            }
        }

        public ColumnSortType SortType {
            get { return _sortType; }
            set {
                _sortType = value;
                OnPropertyChanged("SortType");
            }
        }

        /// <summary>
        /// Resets the table display options without invoking the property changed events (used from  Profile.ClearTables).
        /// </summary>
        internal void Reset() {
            _filter = null;
            SortType = ColumnSortType.Original;
        }
    }
}
namespace Config.Structures {
    public enum ColumnSortType {
        NameAsc,
        NameDesc,
        Original,
        UserDefined
    }
}