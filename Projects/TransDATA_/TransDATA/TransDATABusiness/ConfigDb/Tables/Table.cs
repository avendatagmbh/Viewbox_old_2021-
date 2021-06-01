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
using System.Collections.ObjectModel;
using DbAccess;
using DbAccess.Attributes;


namespace TransDATABusiness.ConfigDb.Tables {

    /// <summary>
    /// This class contains all profile independent table properties.
    /// </summary>
    [DbTable(TABLENAME)]
    public class Table : TableBase, INotifyPropertyChanged {

        internal const string TABLENAME = "table";
        internal const string TABLE_INFO = "tableInfoId";
        internal const string IS_CHECKED = "isChecked";
        internal const string PROFILE_ID = "profileId";
        //internal const string IS_VISIBLE = "isVisbie";

        /// <summary>
        /// Initializes a new instance of the <see cref="Table"/> class.
        /// </summary>
        public Table() {
            this.TableInfo = new TableInfo() ;
            this._columns = new ObservableCollection<Column>();
            //IsVisible = true;
            IsChecked = true;
        }

        /**************************************************************************************/

        #region fields

        /// <summary>
        /// See property TableInfo.
        /// </summary>
        private TableInfo _tableInfo;

        /// <summary>
        /// See property Columns.
        /// </summary>
        private ObservableCollection<Column> _columns;

        /// <summary>
        /// See property IsChecked.
        /// </summary>
        private bool _isChecked;

        /// <summary>
        /// See property ProfileId.
        /// </summary>
        private int _profileId;

        ///// <summary>
        ///// See property IsVisible.
        ///// </summary>
        //private bool _isVisible;

        #endregion fields

        /************************************************************************************/
        
        #region properties
        
        /// <summary>
        /// Gets or sets the table info.
        /// </summary>
        /// <value>The table info.</value>
         [DbColumn(TABLE_INFO, AllowDbNull = false)]
        public TableInfo TableInfo {
            get { return _tableInfo; }
            set {
                if (_tableInfo != value) {
                    _tableInfo = value;
                    OnPropertyChanged("TableInfo");
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
        /// Gets or sets the profile id.
        /// </summary>
        /// <value>The profile id.</value>
        [DbColumn(PROFILE_ID, AllowDbNull = false)]
        public int ProfileId {
            get { return _profileId; }
            set {
                if (_profileId != value) {
                    _profileId = value;
                    OnPropertyChanged("ProfileId");
                }
            }
        }

        ///// <summary>
        ///// Gets or sets a value indicating whether this instance is visible.
        ///// </summary>
        ///// <value>
        ///// 	<c>true</c> if this instance is visible; otherwise, <c>false</c>.
        ///// </value>
        //[DbColumn(IS_VISIBLE, AllowDbNull = false)]
        //public bool IsVisible {
        //    get { return _isVisible; }
        //    set {
        //        if (_isVisible != value) {
        //            _isVisible = value;
        //            OnPropertyChanged("IsVisible");
        //        }
        //    }
        //}

        /// <summary>
        /// Gets or sets the columns.
        /// </summary>
        /// <value>The columns.</value>
        [DbCollection("Table", LazyLoad=false)]
        public ObservableCollection<Column> Columns {
            get     { return _columns; }
            set {
                if (_columns != value) {
                    _columns = value;
                    OnPropertyChanged("Columns");
                }
            }
        }


        #endregion properties 

        /************************************************************************************/

        #region methods

        public static List<Table> Load(IDatabase conn) {
            List<Table> tables = conn.DbMapping.Load<Table>();
            return tables;
        }

        public void Save(IDatabase conn) {

                this.SaveOrUpdate(conn);

            }
        
        #endregion methods

        /************************************************************************************/
       
    }
  }

