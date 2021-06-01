// --------------------------------------------------------------------------------
// author: Benjamin Held / Mirko Dibbert
// since:  2011-07-01
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using DbAccess;
using eBalanceKitBusiness.Logs.Enums;

namespace eBalanceKitBusiness.Logs.DbMapping {
    [DbTable("log_report_1", ForceInnoDb = true)]
    internal class DbReportLog : DbLogBase {
        #region Methods

        #region SetTableName
        public static void SetTableName(int id, IDatabase conn) { conn.DbMapping.SetTableName<DbReportLog>(GetTableName(id)); }
        #endregion

        public static string GetTableName(int id) { return "log_report_" + id; }
        #endregion

        #region Properties

        #region ContentEnum
        [DbColumn("content_type", AllowDbNull = false)]
        public ReportLogContentTypes ReportLogContentType { get; set; }
        #endregion

        #region ActionEnum
        [DbColumn("action_type", AllowDbNull = false)]
        public ActionTypes ActionType { get; set; }
        #endregion

        #region ReferenceId
        [DbColumn("reference_id", AllowDbNull = false)]
        public long ReferenceId { get; set; }
        #endregion

        #region OldValue
        [DbColumn("old_value", AllowDbNull = true, Length = 100000)]
        public string OldValue { get; set; }
        #endregion

        #region NewValue
        [DbColumn("new_value", AllowDbNull = true, Length = 100000)]
        public string NewValue { get; set; }
        #endregion

        #region InfoValue
        [DbColumn("info", AllowDbNull = true, Length = 4096)]
        public string Info { get; set; }
        #endregion

        #endregion
    }
}