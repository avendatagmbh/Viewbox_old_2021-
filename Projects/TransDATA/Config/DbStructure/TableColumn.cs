// (C)opyright 2011 AvenDATA GmbH
// ----------------------------------------
// author: Mirko Dibbert
// since : 2011-08-27

using Config.Interfaces.DbStructure;
using DbAccess;
using DbAccess.Structures;
using Utils;

namespace Config.DbStructure {

    public enum ColumnTypes {
        Text,
        Numeric,
        Bool,
        Date,
        Time,
        DateTime,
        Binary
    }

    /// <summary>
    /// This class represents the structure for a table column.
    /// </summary>
    [DbTable("columns")]
    internal class TableColumn : NotifyPropertyChangedBase, ITableColumn {
        #region Id
        [DbColumn("id", AllowDbNull = false)]
        [DbPrimaryKey]
        public long Id { get; set; }
        #endregion

        #region Profile
        [DbColumn("profile_id", IsInverseMapping = true)]
        internal Profile ProfileDbMapping { get; set; }

        public IProfile Profile { get { return ProfileDbMapping; } }
        #endregion

        #region Table
        [DbColumn("table_id", IsInverseMapping = true)]
        internal Table TableDbMapping { get; set; }

        public ITable Table { get { return TableDbMapping; } }
        #endregion

        #region Name
        private string _name;

        [DbColumn("name", Length = 256)]
        public string Name {
            get { return _name; }
            set {
                if (_name == value) return;
                _name = value;
                if (_name != null && _name.Length > 256) _name = _name.Substring(0, 256);
                OnPropertyChanged("Name");
            }
        }
        #endregion Name
        
        #region Comment
        private string _comment;

        [DbColumn("comment", Length = 256)]
        public string Comment {
            get { return _comment; }
            set {
                if (_comment == value) return;
                _comment = value;
                if (_comment != null && _comment.Length > 256) _name = _comment.Substring(0, 256);
                OnPropertyChanged("Comment");
                OnPropertyChanged("HasComment");
            }
        }

        public bool HasComment { get { return !string.IsNullOrEmpty(Comment); } }
        #endregion Comment
        
        #region TypeName
        private string _typeName;

        [DbColumn("TypeName", Length = 64)]
        public string TypeName { 
            get { return _typeName; }
            set {
                if (_typeName == value) return;
                _typeName = value;
                if (_typeName != null && _typeName.Length > 64) _typeName = _typeName.Substring(0, 264);
            }
        }
        #endregion

        #region MaxLength
        [DbColumn("max_length")]
        public int MaxLength { get; set; }
        #endregion

        #region DoExport
        private bool _doExport = true;

        [DbColumn("do_export")]
        public bool DoExport {
            get { return _doExport; }
            set {
                _doExport = value;
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

        [DbColumn("DbType")]
        public DbColumnTypes DbType { get; set; }

        [DbColumn("Type")]
        public ColumnTypes Type { get; set; }

        [DbColumn("DefaultValue")]
        public string DefaultValue { get; set; }

        [DbColumn("AllowDBNull")]
        public bool AllowDBNull { get; set; }

        [DbColumn("AutoIncrement")]
        public bool AutoIncrement { get; set; }

        [DbColumn("NumericScale")]
        public int NumericScale { get; set; }

        [DbColumn("IsPrimaryKey")]
        public bool IsPrimaryKey { get; set; }

        [DbColumn("IsUnsigned")]
        public bool IsUnsigned { get; set; }

        [DbColumn("IsIdentity")]
        public bool IsIdentity { get; set; }

        [DbColumn("OrdinalPosition")]
        public int OrdinalPosition { get; set; }

        public bool DoDbUpdate { get; set; }

        #region Save
        public void Save() { if (DoDbUpdate) ConfigDb.Save(this); }
        #endregion Save
    }
}