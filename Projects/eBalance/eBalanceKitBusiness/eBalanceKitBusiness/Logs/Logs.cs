using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using eBalanceKitBusiness.Logs;
using DbAccess;
using System.Data.Common;
using eBalanceKitBusiness.Logs.DbMapping;
using eBalanceKitBusiness.Logs.Types;
using eBalanceKitBusiness.Structures;

namespace eBalanceKitBusiness.Logs {
    public class Logs {
        #region Constructor
        public Logs() {
            LogList = new List<LogEntryBase>();
        }
        #endregion

        #region Properties

        #region LogList
        public List<LogEntryBase> LogList { get; private set; }
        #endregion

        #endregion Properties

        #region Methods

        public void ReadAdminLogs(LogFilter logFilter) {
            LogList = new List<LogEntryBase>();
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                List<DbAdminLog> dbAdminLogs = conn.DbMapping.Load<DbAdminLog>(logFilter.GetWhereClause(conn));
                foreach (DbAdminLog dbAdminLog in dbAdminLogs) {
                    AdminLog adminLog = new AdminLog(dbAdminLog);
                    LogList.Add(adminLog);
                }
            }
        }

        public void ReadReportLogs(LogFilter logFilter) {
            LogList = new List<LogEntryBase>();
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                int id = logFilter.ReportId;
                DbReportLog.SetTableName(id, conn);
                List<DbReportLog> dbReportLogs = conn.DbMapping.Load<DbReportLog>(logFilter.GetWhereClause(conn));
                foreach (DbReportLog dbReportLog in dbReportLogs)
                    LogList.Add(new ReportLog(id, dbReportLog));

                DbReportValueChangeLog.SetTableName(id, conn);
                List<DbReportValueChangeLog> dbReportValueChangeLogs = conn.DbMapping.Load<DbReportValueChangeLog>(logFilter.GetWhereClause(conn));
                foreach (DbReportValueChangeLog dbReportValueChangeLog in dbReportValueChangeLogs)
                    LogList.Add(new ReportValueChangeLog(id, dbReportValueChangeLog));

                LogList.Sort(
                    delegate(LogEntryBase p1, LogEntryBase p2) {
                        return p1.Timestamp.CompareTo(p2.Timestamp);
                    }
                );
            }
        }

        public void ReadSendLogs(LogFilter logFilter) {
            LogList = new List<LogEntryBase>();
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                List<DbSendLog> dbSendLogs = conn.DbMapping.Load<DbSendLog>(logFilter.GetWhereClause(conn));
                foreach (DbSendLog dbSendLog in dbSendLogs)
                    LogList.Add(new SendLog(dbSendLog));
            }
        }

        public void ClearLogs() {
            LogList.Clear();
        }

        #endregion Methods

    }
}
