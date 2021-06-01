// --------------------------------------------------------------------------------
// author: Benjamin Held / Mirko Dibbert
// since:  2011-07-14
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using DbAccess;
using eBalanceKitBusiness.Logs.DbMapping;

namespace eBalanceKitBusiness.Logs.Actions {
    internal class ActionAddDocument : ActionLog {
        public ActionAddDocument(int docId) { DocumentId = docId; }
        private int DocumentId { get; set; }

        #region ActionLog Members
        public void DoAction(IDatabase conn) {
            DbReportValueChangeLog.SetTableName(DocumentId, conn);
            conn.DbMapping.CreateTableIfNotExists<DbReportValueChangeLog>();

            DbReportLog.SetTableName(DocumentId, conn);
            conn.DbMapping.CreateTableIfNotExists<DbReportLog>();
        }
        #endregion
    }
}