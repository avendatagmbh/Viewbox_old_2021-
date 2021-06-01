using System;

namespace DbSearchDatabase.DistinctDb.Events {
    public class DbDistincterFinishedEvent : EventArgs {

        public DbDistincterFinishedEvent(DateTime dtStart, DateTime dtEnd) {
            this.Start = dtStart;
            this.End = dtEnd;
        }

        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}