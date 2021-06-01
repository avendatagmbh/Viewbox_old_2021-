using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;

namespace eBalanceKitBusiness.Logs {
    public class LogFilter {
        #region Constructor
        //Initialize filter so that all entries will be retrieved
        public LogFilter() {
            this.BeginTimestamp = new DateTime(1970, 1, 1, 0, 0, 0);
            this.EndTimestamp = DateTime.Now;
        }
        #endregion

        #region Properties
        public DateTime BeginTimestamp{get;set;}
        public DateTime EndTimestamp { get; set; }

        public bool HasWhereClause { get { return true; } }
        public bool HasOrderByClause { get { return true; } }
        public string OrderByClause { get { return "timestamp"; } }
        //#region ReportIds
        //private List<int> _reportIds;
        //public List<int> ReportIds {
        //    get { return _reportIds; }
        //    set {
        //        _reportIds = value; 
        //    }
        //}
        //#endregion
        public int ReportId { get; set; }

        #endregion

        #region Methods
        //public void BeginTimestamp(DateTime value) {
        //    AddWhereClause();
        //}
        //public void EndTimestamp(DateTime value) {
        //    AddWhereClause("timestamp <= " + value);
        //}
        public string GetWhereClause(IDatabase conn) {
            string whereClause = string.Empty;
            whereClause = (!HasWhereClause ? string.Empty : AddWhereClause(whereClause, "timestamp >= " + conn.GetSqlString(BeginTimestamp.ToString())));
            if (!Manager.UserManager.Instance.CurrentUser.IsAdmin) {
                whereClause = AddWhereClause(whereClause,
                                             conn.Enquote("user_id") + "= " + Manager.UserManager.Instance.CurrentUser.Id);
            }
            return whereClause;
        }
        private string AddWhereClause(string current, string where) {
            string whereClause = string.IsNullOrEmpty(current) ? "": current + " AND ";
            return whereClause + where;
        }
        #endregion
    }
}
