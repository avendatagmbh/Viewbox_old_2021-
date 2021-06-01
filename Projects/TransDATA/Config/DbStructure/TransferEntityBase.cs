// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-11
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using Config.Interfaces.DbStructure;
using Config.Structures;
using DbAccess;
using Utils;

namespace Config.DbStructure {
    public class TransferEntityBase : NotifyPropertyChangedBase {

        #region Constructor
        public TransferEntityBase() {
            TransferState = new TransferState();
        }
        #endregion

        #region Profile

        public IProfile Profile { get { return ProfileDbMapping; } }

        [DbColumn("profile_id", IsInverseMapping = true)]
        internal Profile ProfileDbMapping { get; set; }
        #endregion

        #region Count
        [DbColumn("count")]
        public long Count { get; set; }
        #endregion

        #region Catalog
        private string _catalog;

        [DbColumn("catalog", Length = 256)]
        public string Catalog {
            get { return _catalog; }
            set {
                if (_catalog == value) return;
                _catalog = value;
                if (_catalog != null && _catalog.Length > 256) _catalog = _catalog.Substring(0, 256);
                OnPropertyChanged("Catalog");
            }
        }
        #endregion Schema

        #region Schema
        private string _schema;

        [DbColumn("schema", Length = 256)]
        public string Schema {
            get { return _schema; }
            set {
                if (_schema == value) return;
                _schema = value;
                if (_schema != null && _schema.Length > 256) _schema = _schema.Substring(0, 256);
                OnPropertyChanged("Schema");
            }
        }
        #endregion Schema

        #region Comment
        private string _comment;

        [DbColumn("comment", Length = 256)]
        public string Comment {
            get { return _comment; }
            set {
                if (_comment == value) return;
                _comment = value;
                if (_comment != null && _comment.Length > 256) _comment = _comment.Substring(0, 256);
                OnPropertyChanged("Comment");
                OnPropertyChanged("HasComment");
            }
        }

        public bool HasComment { get { return !string.IsNullOrEmpty(Comment); } }
        #endregion Comment

        #region TransferState
        public TransferState TransferState { get; set; }

        [DbColumn("tableState")]
        internal TransferStates State {
            get { return TransferState.State; }
            set { TransferState.State = value; }
        }

        [DbColumn("statemessage")]
        internal string Message {
            get { return TransferState.Message; }
            set { TransferState.Message = value; }
        }
        #endregion TransferState

        #region DoExport
        private bool _doExport = true;

        [DbColumn("do_export")]
        public bool DoExport {
            get { return _doExport; }
            set {
                _doExport = value;
                //if(DoDbUpdate)
                Save();
                OnPropertyChanged("DoExport");
            }
        }
        #endregion

        #region IsSelected
        public bool IsSelected {
            get { return _isSelected; }
            set {
                if (_isSelected == value) return;
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }
        private bool _isSelected;
        #endregion         

        public bool DoDbUpdate { get; set; }

        #region Save
        public void Save() { if (DoDbUpdate) ConfigDb.Save(this); }
        #endregion Save
    }
}