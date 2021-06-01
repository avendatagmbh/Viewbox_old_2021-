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
using TransDATABusiness.Enums;
using DbAccess.Attributes;

namespace TransDATABusiness.ConfigDb.Tables {

    /// <summary>
    /// This class contains all profile dependent column properties.
    /// </summary> 
    [DbTable(TABLENAME)]
    public class ColumnInfo : TableBase, INotifyPropertyChanged {

        internal const string TABLENAME = "columninfo";
        internal const string NAME = "name";
        internal const string DESCRIPTION = "description";
        internal const string TYPE = "type";
        internal const string TYPE_NAME = "type_name";
        internal const string IS_PRIMARY_KEY = "isPrimaryKey";
        internal const string LENGTH = "length";
        internal const string NUMERICSCALE = "numericScale";
        internal const string NUMERICPRECISION = "numericPrecision";
        internal const string ORDINAL = "ordinal";

        ///<summary>
        /// Initialize a new instance of the <see cref="ColumnInfo"/> class.
        ///</summary>
        public ColumnInfo() {
            Description = "";
        }

        /*******************************************************************************/

        #region fields

        /// <summary>
        /// See propertry Name.
        /// </summary>
        private string _name;

        /// <summary>
        /// See property Description.
        /// </summary>
        private string _description;

        /// <summary>
        /// See property Type.
        /// </summary>
        private ExportType _type;

        /// <summary>
        /// See property TypeName.
        /// </summary>
        private string _typeName;

        /// <summary>
        /// See property IsPrimaryKey
        /// </summary>
        private bool _isPrimaryKey;

        /// <summary>
        /// See property Length.
        /// </summary>
        private int _length;

        /// <summary>
        /// See property NumericScale.
        /// </summary>
        private int _numericScale;

        /// <summary>
        /// See property NumericPrecision.
        /// </summary>
        private int _numericPrecision;

        /// <summary>
        /// See property Ordinal.
        /// </summary>
        private int _ordinal;

        #endregion fields

        /*******************************************************************************/

        #region properties

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

        /// <summary>
        /// Gets or sets the type number.
        /// </summary>
        /// <value>The type number.</value>
        [DbColumn(TYPE, AllowDbNull = false)]
        public ExportType Type {
            get { return _type; }
            set {
                if (_type != value) {
                    _type = value;
                    OnPropertyChanged("Type");
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of the type.
        /// </summary>
        /// <value>The name of the type.</value>
        [DbColumn(TYPE_NAME, AllowDbNull = false, Length = 32)]
        public string TypeName {
            get { return _typeName; }
            set {
                if (_typeName != value) {
                    _typeName = value;
                    OnPropertyChanged("TypeName");
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is primary key.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is primary key; otherwise, <c>false</c>.
        /// </value>
        [DbColumn(IS_PRIMARY_KEY, AllowDbNull = false)]
        public bool IsPrimaryKey {
            get { return _isPrimaryKey; }
            set {
                if (_isPrimaryKey != value) {
                    _isPrimaryKey = value;
                    OnPropertyChanged("IsPrimaryKey");
                }
            }
        }

        /// <summary>
        /// Gets or sets the length.
        /// </summary>
        /// <value>The length.</value>
        [DbColumn(LENGTH, AllowDbNull = false)]
        public int Length {
            get { return _length; }
            set {
                if (_length != value) {
                    _length = value;
                    OnPropertyChanged("Length");
                }
            }
        }


        /// <summary>
        /// Gets or sets the numeric scale.
        /// </summary>
        /// <value>The numeric scale.</value>
        [DbColumn(NUMERICSCALE, AllowDbNull = false)]
        public int NumericScale {
            get { return _numericScale; }
            set {
                if (_numericScale != value) {
                    _numericScale = value;
                    OnPropertyChanged("NumericScale");
                }
            }
        }


        /// <summary>
        /// Gets or sets the numeric precision.
        /// </summary>
        /// <value>The numeric precision.</value>
        [DbColumn(NUMERICPRECISION, AllowDbNull = false)]
        public int NumericPrecision {
            get { return _numericPrecision; }
            set {
                if (_numericPrecision != value) {
                    _numericPrecision = value;
                    OnPropertyChanged("NumericPrecision");
                }
            }
        }

        /// <summary>
        /// Gets or sets the ordinal.
        /// </summary>
        /// <value>The ordinal.</value>
        [DbColumn(ORDINAL, AllowDbNull = false)]
        public int Ordinal {
            get { return _ordinal; }
            set {
                if (_ordinal != value) {
                    _ordinal = value;
                    OnPropertyChanged("Ordinal");
                }
            }
        }

        #endregion properties

        /****************************************************************************/
        
        #region methods

        public ExportType GetFieldtype(string typename) {

                    switch (typename) {
                        case "System.Int16":
                            return ExportType.Numeric;
                            
                        case "System.UInt16":
                            return ExportType.Numeric;
                      
                        case "System.Int32":
                            return ExportType.Numeric;
                           
                        case "System.UInt32":
                            return ExportType.Numeric;
                          
                        case "System.Int64":
                            return ExportType.Numeric;
                          
                        case "System.UInt64":
                            return ExportType.Numeric;
             
                        case "System.Decimal":
                            return ExportType.Numeric;
                    
                        case "System.Double":
                            return ExportType.Numeric;
                       
                        case "System.Boolean":
                            return ExportType.Alphanumeric;
                        
                        case "System.String":
                            return ExportType.Alphanumeric;
                    
                        case "System.DateTime":
                            return ExportType.Date;
                     

                        default:
                            throw new Exception("Datentyp nicht definiert");
                    }
                }

        /// <summary>
        /// Loads the specified conn.
        /// </summary>
        /// <param name="conn">The conn.</param>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static ColumnInfo Load(IDatabase conn, int id) {
            return conn.DbMapping.Load<ColumnInfo>(id);
        }

        #endregion methods 
    }
}
