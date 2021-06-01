// (C)opyright 2011 AvenDATA GmbH
// ----------------------------------------
// author: Mirko Dibbert
// since : 2011-08-27

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Config.Interfaces.DbStructure;
using DbAccess;
using Utils;
using System;

namespace Config.DbStructure {
    /// <summary>
    /// This class represents the structure for a table.
    /// </summary>
    [DbTable("tables")]
    internal class Table : TransferEntityBase, ITable {
        #region Constructor
        public Table() {
            FileNames = new List<string>();
            
        }
        #endregion Constructor

        #region Id
        [DbColumn("id", AllowDbNull = false)]
        [DbPrimaryKey]
        public long Id { get; set; }
        #endregion

        //#region Count
        //[DbColumn("count")]
        //public long Count { get; set; }
        //#endregion

        //#region Catalog
        //private string _catalog;

        //[DbColumn("catalog", Length = 256)]
        //public string Catalog {
        //    get { return _catalog; }
        //    set {
        //        if (_catalog == value) return;
        //        _catalog = value;
        //        if (_catalog != null && _catalog.Length > 256) _catalog = _catalog.Substring(0, 256);
        //        OnPropertyChanged("Catalog");
        //    }
        //}
        //#endregion Schema

        //#region Schema
        //private string _schema;

        //[DbColumn("schema", Length = 256)]
        //public string Schema {
        //    get { return _schema; }
        //    set {
        //        if (_schema == value) return;
        //        _schema = value;
        //        if (_schema != null && _schema.Length > 256) _schema = _schema.Substring(0, 256);
        //        OnPropertyChanged("Schema");
        //    }
        //}
        //#endregion Schema

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

        #region DestinationName
        private string _destinationName;

        [DbColumn("destinationName", Length = 1024)]
        public string DestinationName {
            get { return _destinationName; }
            set {
                if (_destinationName == value) return;
                _destinationName = value;
                if (_destinationName.Length > 1024) throw new Exception("Filename too long.");
                OnPropertyChanged("DestinationName");
            }
        }
        #endregion DestinationName

        #region Filter
        private string _filter;

        [DbColumn("filter", Length = 512)]
        public string Filter {
            get { return _filter; }
            set {
                if (_filter == value) return;
                _filter = value;
                if (_filter != null && _filter.Length > 512) _filter = string.Empty;
                OnPropertyChanged("Filter");
            }
        }
        #endregion Filter

        #region Type
        private string _type;

        [DbColumn("type", Length = 64)]
        public string Type {
            get { return _type; }
            set {
                if (_type == value) return;
                _type = value;
                if (_type != null && _type.Length > 64) _type = _type.Substring(0, 64);
                OnPropertyChanged("Type");
            }
        }
        #endregion Schema
        
        #region Columns
        private ObservableCollectionAsync<TableColumn> _columns;

        public IEnumerable<IColumn> Columns { get { return ColumnsDbMapping; } }

        [DbCollection("TableDbMapping", LazyLoad = true)]
        internal ICollection<TableColumn> ColumnsDbMapping {
            get {
                if (_columns == null) LoadColumns();
                return _columns;
            }
        }
        #endregion Columns  

        #region FileNames
        public List<string> FileNames { get; set; }
        [DbColumn("filenames")]
        internal string FilenamesDb {
            get { return string.Join("?", FileNames); }
            set {
                FileNames.Clear();
                FileNames = value.Split(new char[] { '?' }).ToList();
            }
        }
        #endregion FileNames

        public string DisplayString { get { return Name; } }

        #region methods

        #region LoadColumns
        private void LoadColumns() {
            _columns = new ObservableCollectionAsync<TableColumn>();
            using (var conn = ConfigDb.GetConnection()) {
                foreach (var column in conn.DbMapping.Load<TableColumn>(conn.Enquote("table_id") + "=" + Id)) {
                    var c = (TableColumn) column;
                    c.ProfileDbMapping = ProfileDbMapping;
                    c.TableDbMapping = this;
                    _columns.Add(column);
                    column.DoDbUpdate = true;
                }
            }

        }
        #endregion

        #region ToString
        public override string ToString() {
            string result = string.Empty;
            if (!string.IsNullOrEmpty(Schema)) result += (result.Length > 0 ? "." : string.Empty) + Schema;
            if (!string.IsNullOrEmpty(Name)) result += (result.Length > 0 ? "." : string.Empty) + Name;
            return result;
        }

        #endregion

        #region CreateColumn
        public ITableColumn CreateColumn() {
            return new TableColumn { ProfileDbMapping = ProfileDbMapping, TableDbMapping = this };
        }
        #endregion

        #region AddColumn
        public void AddColumn(ITableColumn tableColumn) {
            Debug.Assert(tableColumn is TableColumn, "Column object must be of type Config.DbStructure.Column!");
            var c = (TableColumn) tableColumn;
            ColumnsDbMapping.Add(c);
            c.DoDbUpdate = true;
        }
        #endregion

        #endregion methods
    }
}