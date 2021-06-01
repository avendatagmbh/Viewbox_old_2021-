using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;
using DbAccess.Structures;
using eBalanceKitBusiness.Logs.DbMapping;
using eBalanceKitBusiness.Logs.Enums;

namespace eBalanceKitBusiness.Logs.Actions {
    class ActionDeleteDocument : ActionLog {
        private int DocumentId { get; set; }

        public ActionDeleteDocument(int docId) {
            this.DocumentId = docId;
        }

        public void DoAction(IDatabase conn) {
            conn.DropTableIfExists(DbReportValueChangeLog.GetTableName(DocumentId));
            conn.DropTableIfExists(DbReportLog.GetTableName(DocumentId));
            //Delete all value changes
            string sql = "DELETE FROM " + conn.Enquote("log_admin") +
                " WHERE " + conn.Enquote("content_type")+ "=" + Convert.ToInt16(AdminLogContentTypes.Document) +
                " AND " + conn.Enquote("reference_id")+"=" + DocumentId +
                " AND " + conn.Enquote("action_type")+"=" + Convert.ToInt16(ActionTypes.Change);
            conn.ExecuteNonQuery(sql);

            //Delete user rights
            sql = "DELETE FROM " + conn.Enquote("log_admin") +
                " WHERE " + conn.Enquote("content_type")+"=" + Convert.ToInt16(AdminLogContentTypes.UserRightDocument) +
                " AND " + conn.Enquote("reference_id")+"=" + DocumentId;
            conn.ExecuteNonQuery(sql);

            sql = "UPDATE " + conn.Enquote("log_send") +
                " SET " + conn.Enquote("document_id") + "=0" +
                " WHERE " + conn.Enquote("document_id") + "=" + DocumentId;
            conn.ExecuteNonQuery(sql);
            //Update Send reports which referenced this document
        }
    }
}
