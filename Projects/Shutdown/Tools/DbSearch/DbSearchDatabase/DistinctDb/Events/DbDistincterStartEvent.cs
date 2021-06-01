using System;

namespace DbSearchDatabase.DistinctDb.Events {
    public class DbDistincterStartEvent : EventArgs{

        public DbDistincterStartEvent(int totalTableCount) {
            this.TotalTableCount = totalTableCount;
        }

        public int TotalTableCount { get; set; }
    }
}
