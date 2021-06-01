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
    /// This class contains all profile dependent table properties.
    /// </summary>
    [DbTable(TABLENAME)]
    public class TableInfo : TableBase, INotifyPropertyChanged {

        internal const string TABLENAME = "tableinfo";
        internal const string SCHEMA = "schema";
        internal const string NAME = "name";
        internal const string DESCRIPTION = "description";
        internal const string COUNT = "count";
        internal const string STRCOUNT = "strcount";
        internal const string COUNTSTRING = "countstring";
        internal const string CLMCOUNT = "clmcount";
        internal const string STRCLMCOUNT = "strclmcount";
        internal const string RUNNINGNUMBER = "runningnumber";
         /// <summary>
        /// Initializes a new instance of the <see cref="TableInfo"/> class.
        /// </summary>
        public TableInfo() {
        }

        /*********************************************************************************/

        #region fields
        
        ///<summary>
        /// See property Schema.
        /// </summary>
        private string _schema; 
       
        /// <summary>
        /// See propertry Name.
        /// </summary>
        private string _name;

        /// <summary>
        /// See property Description.
        /// </summary>
        private string _description;

        /// <summary>
        /// See property Count.
        /// </summary>
        private long _count;

        /// <summary>
        /// See property StrCount.
        /// </summary>
        private string _strCount;
        private string _countString;

        /// <summary>
        /// See property Count.
        /// </summary>
        private int _clmCount;

        /// <summary>
        /// See property StrClmCount.
        /// </summary>
        private string _strClmCount;

        /// <summary>
        /// See property RunningNumber.
        /// </summary>
        private int _runningNumber;  

        #endregion fields 

        /********************************************************************************/

        #region properties

        /// <summary>
        /// Gets or sets the schema.
        /// </summary>
        /// <value>The schema.</value>
        [DbColumn(SCHEMA, AllowDbNull = false, Length = 128)]
        public string Schema {
            get{ return _schema;}
            set{
                if (_schema != value){
                    _schema = value;
                    OnPropertyChanged("Schema");
                }
        
            }
        }
        
        /// <summary>
        /// Gets or sets the name of the table.
        /// </summary>
        /// <value>The name of the table.</value>
        [DbColumn(NAME, AllowDbNull = false, Length = 128)]
        public string Name {
            get { return _name; }
            set {
                if (_name != value) {
                    _name = value;
                    OnPropertyChanged("Name");
                }

            }
        }

        /// <summary>
        /// Gets or sets the Description.
        /// </summary>
        /// <value>The Description.</value>
        [DbColumn(DESCRIPTION, AllowDbNull = false, Length = 128)]
        public string Description {
            get { return _description; }
            set {
                if (_description != value) {
                    _description = value;
                    OnPropertyChanged("Description");
                }

            }
        }

        [DbColumn(COUNT, AllowDbNull = false, Length = 128)]
        public long Count {
            get { return _count; }
            set {
                if (_count != value) {
                    _count = value;
                    OnPropertyChanged("Count");
                }
            }
        }

        [DbColumn(STRCOUNT, AllowDbNull = false, Length = 128)]
        public string StrCount {
            get { return _strCount; }
            set {
                if (_strCount != value) {
                    _strCount = value;
                    OnPropertyChanged("StrCount");
                }
            }
        }

        [DbColumn(COUNTSTRING, AllowDbNull = false, Length = 128)]
        public string CountString {
            get { return _countString; }
            set {
                if (_countString != value) {
                    _countString = value;
                    OnPropertyChanged("CountString");
                }
            }
        }

        [DbColumn(CLMCOUNT, AllowDbNull = false)]
        public int ClmCount {
            get { return _clmCount; }
            set {
                if (_clmCount != value) {
                    _clmCount = value;
                    OnPropertyChanged("ClmCount");
                }
            }
        }

        [DbColumn(STRCLMCOUNT, AllowDbNull = false, Length = 128)]
        public string StrClmCount {
            get { return _strClmCount; }
            set {
                if (_strClmCount != value) {
                    _strClmCount = value;
                    OnPropertyChanged("StrClmCount");
                }
            }
        }

        [DbColumn(RUNNINGNUMBER, AllowDbNull = false, Length = 128)]
        public int RunningNumber {
            get { return _runningNumber; }
            set {
                if (_runningNumber != value) {
                    _runningNumber = value;
                    OnPropertyChanged("RunningNumber");
                }

            }
        }

        /// <summary>
        /// Loads the specified conn.
        /// </summary>
        /// <param name="conn">The conn.</param>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static TableInfo Load(IDatabase conn, int id) {
            return conn.DbMapping.Load<TableInfo>(id);
        }

        #endregion properties

        /*******************************************************************************/

    }
}
