using System;

namespace DbSearchDatabase.DistinctDb.Events {
    public class DbDistincterStartColumnEvent : EventArgs{

        public DbDistincterStartColumnEvent(string sColumnName) {
            this.ColumnName = sColumnName;
        }

        public string ColumnName { get; set; }
    }
}
