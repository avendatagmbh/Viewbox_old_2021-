using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;
using eBalanceKitBusiness.Logs.DbMapping;
using eBalanceKitBusiness.Logs.Enums;

namespace eBalanceKitBusiness.Logs.Actions {
    class ActionDeleteCompany : ActionLog {
        private int CompanyId;

        public ActionDeleteCompany(int companyId) {
            CompanyId = companyId;
        }

        public void DoAction(IDatabase conn) {
            string sql = "DELETE FROM " + conn.Enquote("log_admin") +
                " WHERE " +conn.Enquote("content_type")+"=" + Convert.ToInt16(AdminLogContentTypes.Company) +
                " AND " +conn.Enquote("reference_id")+"=" + CompanyId + 
                " AND " +conn.Enquote("action_type")+"=" + Convert.ToInt16(ActionTypes.Change);
            conn.ExecuteNonQuery(sql);
        }
    }
}
