using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQLParser2.Model
{
    public class DBTable
    {
        private DBColumnCollection _columns = new DBColumnCollection();
        public SQLStatements StatementType { get; set; }
        public DBColumnCollection Columns
        {
            get { return _columns; }
            set { _columns = value; }
        }
        public string Name { get; set; }
        public override string ToString()
        {
            return Name;
        }
    }

    public class DBTableCollection : List<DBTable>
    {
        public new void Add(DBTable table)
        {
            if (!Exists(table))
            {
                base.Add(table);
            }
            // update the statement type
            else if (this[table.Name].StatementType == SQLStatements.Unknown) 
            {
                this[table.Name].StatementType = table.StatementType;
            }
        }
        public bool Exists(DBTable table) 
        {
            return Exists(table.Name);
        }
        public bool Exists(string tableName)
        {
            return base.Exists(t => t.Name.ToLower().Equals(tableName.ToLower()));
        }
        public DBTable this[string tableName] 
        {
            get {
                return this.SingleOrDefault<DBTable>(t => t.Name.ToLower().Equals(tableName.ToLower()));
            }
        }
    }
}
