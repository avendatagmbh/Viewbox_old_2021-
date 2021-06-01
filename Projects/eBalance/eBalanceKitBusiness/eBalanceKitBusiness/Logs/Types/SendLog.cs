using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;
using System.ComponentModel;
using eBalanceKitBusiness.Logs;
using eBalanceKitBusiness.Logs.DbMapping;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitResources.Localisation;

namespace eBalanceKitBusiness.Logs.Types {

    public class SendLog : LogEntryBase {
        internal DbSendLog DbSendLog {get; set;}

        internal SendLog(DbSendLog dbSendLog = null) {
            DbSendLog = dbSendLog;
        }

        protected override string GenerateMessage() {
            Document document = DocumentManager.Instance.GetDocument((int)DbSendLog.DocumentId);
            if(document == null) return ResourcesLogging.SentDeletedDocument;
            string errorString = "";
            if (DbSendLog.SendError == DbSendLog.SendErrorType.VerificationError)
                errorString = ResourcesLogging.VerificationError;
            if (DbSendLog.SendError == DbSendLog.SendErrorType.SendError)
                errorString = ResourcesLogging.SendError;
            if (DbSendLog.SendError == DbSendLog.SendErrorType.UnknownError)
                errorString = ResourcesLogging.UnknownError;
            return string.Format(ResourcesLogging.DocumentSent, document.Name) + " " + errorString;
        }

        public override void SaveToDb(IDatabase conn) {
            conn.DbMapping.Save(DbSendLog);
        }

        public override DateTime Timestamp {
            get {
                return DbSendLog.TimeStamp;
            }
        }

        public override int UserId() {
            return DbSendLog.UserId;
        }

        protected override User User { get { return UserManager.Instance.GetUser(DbSendLog.UserId); } }
    }
}
