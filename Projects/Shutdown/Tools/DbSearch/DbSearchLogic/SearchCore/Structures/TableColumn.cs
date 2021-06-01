using System;
using DbAccess.Structures;

namespace DbSearchLogic.SearchCore.Structures
{
    /// <summary>
    /// TableField stores the column name and type of a database table 
    /// </summary>
    public class TableColumn
    {
        /*
         * Variables
         */

        private string _columnName;
        private DbColumnTypes _columnTypeEnum;

        #region Constructor
        public TableColumn(string sName, DbColumnTypes type)
        {
            // Initialize the variables
            _columnName = sName;
            _columnTypeEnum = type;
        }
        #endregion Constructor


        /*
         * Properties
         */

        /// <summary>
        /// Gets or sets the column name
        /// </summary>
        public string Name
        {
            get { return _columnName; }
            set { _columnName = value; }
        }

        public DbColumnTypes Type
        {
            get { return _columnTypeEnum; }
            set { 
                _columnTypeEnum = value;
                }
        }
    }
}