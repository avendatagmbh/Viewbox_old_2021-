// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-10-02
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using Config.Structures;
using Utils;

namespace TransDATA.Structures {
    public class TableDisplayOptions : NotifyPropertyChangedBase {
        private string _filter;
        private TableSortType _sortType;
        private TableFilterType _filterType;

        public string Filter {
            get { return _filter; }
            set {
                _filter = value;
                OnPropertyChanged("Filter");
            }
        }

        public TableSortType SortType {
            get { return _sortType; }
            set {
                _sortType = value;
                OnPropertyChanged("SortType");
            }
        }

        public TableFilterType FilterType {
            get { return _filterType; }
            set {
                _filterType = value;
                OnPropertyChanged("FilterType");
            }
        }

        /// <summary>
        /// Resets the table display options without invoking the property changed events (used from  Profile.ClearTables).
        /// </summary>
        internal void Reset() {
            _filter = null;
            SortType = TableSortType.Original;
        }
    }
}

namespace Config.Structures
{
    public enum TableSortType
    {
        NameAsc,
        NameDesc,
        CountAsc,
        CountDesc,
        Original,
        TransferState
    }
}

namespace Config.Structures
{
    public enum TableFilterType
    {
        All,
        NotTransfered,
        TransferedError,
        TransferedCountDifference,
        TransferedOk
    }
}