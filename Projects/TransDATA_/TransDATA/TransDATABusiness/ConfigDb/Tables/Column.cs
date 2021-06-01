/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * hel                  2010-11-19      initial implementation
 *************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using TransDATABusiness.ConfigDb.Tables.Base;
using DbAccess;
using DbAccess.Attributes;

namespace TransDATABusiness.ConfigDb.Tables {
    
    /// <summary>
    /// This class contains all profile independent column properties.
    /// </summary>
    [DbTable(TABLENAME)]
    public class Column : TableBase, INotifyPropertyChanged {

        internal const string TABLENAME = "column";
        internal const string COLUMN_INFO = "columnInfoId";
        internal const string IS_CHECKED = "isChecked";
        internal const string FILTER = "filter";
        internal const string TABLE_ID = "tableId";

        /// <summary>
        /// Initializes a new instance of the <see cref="Column"/> class.
        /// </summary>
        public Column() {
            this.ColumnInfo = new ColumnInfo();
            IsChecked = true;
            Filter = "";
        }

        /***************************************************************************************/

        #region fields

        /// <summary>
        /// See property ColumnInfo
        /// </summary>
        private ColumnInfo _columnInfo;
              
        /// <summary>
        /// See property IsChecked.
        /// </summary>
        private bool _isChecked;

        /// <summary>
        /// See property Filter.
        /// </summary>
        private string _filter;

        /// <summary>
        /// See property TableId.
        /// </summary>
        private Table _tableId;

        #endregion fields 

        /***************************************************************************/

        #region properties
        
        /// <summary>
        /// Gets or sets the table id.
        /// </summary>
        /// <value>The table id.</value>
        [DbColumn(TABLE_ID, AllowDbNull = false)]
        public Table Table {
            get { return _tableId; }
            set {
                if (_tableId != value) {
                    _tableId = value;
                    OnPropertyChanged("Table");
                }
            }
        }

        /// <summary>
        /// Gets or sets the column info.
        /// </summary>
        /// <value>The column info.</value>
        [DbColumn(COLUMN_INFO, AllowDbNull = false)]
        public ColumnInfo ColumnInfo {
            get { return _columnInfo; }
            set {
                if (_columnInfo != value) {
                    _columnInfo = value;
                    OnPropertyChanged("ColumnInfo");
                }
            }
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether this instance is checked.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is checked; otherwise, <c>false</c>.
        /// </value>
        [DbColumn(IS_CHECKED, AllowDbNull = false)]
        public bool IsChecked {
            get { return _isChecked; }
            set {
                if (_isChecked != value) {
                    _isChecked = value;
                    OnPropertyChanged("IsChecked");
                }
            }
        }

        /// <summary>
        /// Gets or sets the filter.
        /// </summary>
        /// <value>The filter.</value>
        [DbColumn(FILTER, AllowDbNull = false, Length = 1024)]
        public string Filter {
            get { return _filter; }
            set {
                if (_filter != value) {
                    _filter = value;
                    OnPropertyChanged("Filter");
                }
            }
        }

        #endregion properties

        /***************************************************************************/

        #region methods

        public static List<Column> Load(IDatabase conn) {
            return conn.DbMapping.Load<Column>();
        }

        public void Save(IDatabase conn) {    
            SaveOrUpdate(conn);
        }

        #endregion methods

    }
}
