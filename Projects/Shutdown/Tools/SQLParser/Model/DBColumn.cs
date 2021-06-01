using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQLParser2.Model
{
    public class DBColumn 
    {
        public string Name { get; set; }

        public bool IsIndexColumn { get; set; }
    }
    public class DBColumnCollection : List<DBColumn>
    {
        public new void Add(DBColumn column)
        {
            if (!Exists(c => c.Name.ToLower().Equals(column.Name.ToLower())))
            {
                base.Add(column);
            }
        }
    }
}
