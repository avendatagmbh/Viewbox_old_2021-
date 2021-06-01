using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQLParser2.Model
{
    public class SQLObjectTree
    {
        private DBTableCollection _tables = new DBTableCollection();
        private DBColumnCollection _columns = new DBColumnCollection();

        /// <summary>
        /// Store all columns in statement within nested statement or not.
        /// </summary>
        public DBColumnCollection Columns
        {
            get { return _columns; }
            set { _columns = value; }
        }
        public DBTableCollection Tables
        {
            get { return _tables; }
            set { _tables = value; }
        }
        public void AddColumn(DBColumn column, DBTable owningTable) 
        {
            if(owningTable != null)
                owningTable.Columns.Add(column);

            Columns.Add(column);
        }
        public void AddTable(DBTable table) 
        {
            _tables.Add(table);
        }
    }
}
