// --------------------------------------------------------------------------------
// author: Banjamin Held / Mirko Dibbert
// since:  2011-07-05
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using DbAccess;
using eBalanceKitBusiness.Logs.DbMapping;
using eBalanceKitBusiness.Logs.LogMessages;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness.Logs.Types {
    internal class AdminLog : LogEntryValueChangeBase {
        public AdminLog(DbAdminLog dbAdminLog = null) {
            DbAdminLog = dbAdminLog;
            _logMessageAdmin = LogMessageAdminFactory.CreateLogMessage(DbAdminLog);
        }

        private readonly LogMessageAdmin _logMessageAdmin;

        public DbAdminLog DbAdminLog { get; private set; }

        public override DateTime Timestamp { get { return DbAdminLog.TimeStamp; } }
        protected override User User { get { return UserManager.Instance.GetUser(DbAdminLog.UserId); } }
        public override int UserId() { return DbAdminLog.UserId; }

        //private string AccountMessage() { return ResourcesLogging.AccountAssignment; }

        protected override string GenerateMessage() { return _logMessageAdmin.GetMessage(); }


        public override void SaveToDb(IDatabase conn) { conn.DbMapping.Save(DbAdminLog); }

        public override string GetOldValue { get { return _logMessageAdmin.GetOldValue(); } }

        public override string GetNewValue { get { return _logMessageAdmin.GetNewValue(); } }
    }
}