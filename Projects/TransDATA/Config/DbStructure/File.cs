// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-11
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using Config.Interfaces.DbStructure;
using DbAccess;
using Utils;
using System;

namespace Config.DbStructure {

    [DbTable("file")]
    public class File : TransferEntityBase, IFile {

        #region Id
        [DbColumn("id", AllowDbNull = false)]
        [DbPrimaryKey]
        public long Id { get; set; }
        #endregion

        #region Path
        private string _path;

        [DbColumn("path", Length = 1024)]
        public string Path {
            get { return _path; }
            set {
                if (_path == value) return;
                _path = value;
                if (Path.Length > 1024) throw new Exception("Path too long.");
                OnPropertyChanged("Path");
            }
        }
        #endregion Path

        #region Filter
        private string _filter;

        [DbColumn("filter", Length = 1024)]
        public string Filter {
            get { return _filter; }
            set {
                if (_filter == value) return;
                _filter = value;
                if (Filter.Length > 1024) throw new Exception("Path too long.");
                OnPropertyChanged("Filter");
            }
        }
        #endregion Filter

        #region Filename
        private string _name;

        [DbColumn("name", Length = 1024)]
        public string Name {
            get { return _name; }
            set {
                if (_name == value) return;
                _name = value;
                if (Name.Length > 1024) throw new Exception("Filename too long.");
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

        #region Size
        [DbColumn("size")]
        public long Size { get; set; }
        #endregion

        #region Part
        [DbColumn("part")]
        public int Part { get; set; }
        #endregion

        #region Columns
        private ObservableCollectionAsync<FileColumn> _columns;

        public IEnumerable<IColumn> Columns { get { return ColumnsDbMapping; } }

        [DbCollection("TableDbMapping", LazyLoad = true)]
        internal ICollection<FileColumn> ColumnsDbMapping {
            get {
                if (_columns == null) LoadColumns();
                return _columns;
            }
        }
        #endregion Columns  
        
        public string DisplayString { get { return Name; } }

        #region LoadColumns
        private void LoadColumns() {
            _columns = new ObservableCollectionAsync<FileColumn>();
            using (var conn = ConfigDb.GetConnection()) {
                foreach (var column in conn.DbMapping.Load<FileColumn>(conn.Enquote("table_id") + "=" + Id)) {
                    var c = (FileColumn)column;
                    c.ProfileDbMapping = ProfileDbMapping;
                    c.FileDbMapping = this;
                    _columns.Add(column);
                    column.DoDbUpdate = true;
                }
            }

        }
        #endregion
    }
}