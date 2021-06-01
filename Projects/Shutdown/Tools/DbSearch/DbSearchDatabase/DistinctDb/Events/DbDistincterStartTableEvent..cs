using System;

namespace DbSearchDatabase.DistinctDb.Events {
    public class DbDistincterStartTableEvent : EventArgs {

        public DbDistincterStartTableEvent(string sTableName) {
            this.TableName = sTableName;
        }

        public string TableName { get; set; }
    }
}
