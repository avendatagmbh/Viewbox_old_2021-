using System;

namespace DbSearchDatabase.DistinctDb.Events {
    public class DbDistincterPerformActionEvent : EventArgs {

        public DbDistincterPerformActionEvent(string action) {
            this.PerfomedAction = action;
        }

        public string PerfomedAction { get; set; }
    }
}
