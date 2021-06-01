using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using eBalanceKitBusiness.Logs.DbMapping;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Manager;
using eBalanceKitResources.Localisation;

namespace eBalanceKitBusiness.Logs.LogMessages {
    class LogMessageAdminUserRight   : LogMessageAdmin{
        public LogMessageAdminUserRight(DbAdminLog dbAdminLog)
            : base(dbAdminLog) {

        }

        protected override string ContentMessage() {
            User user = UserManager.Instance.GetUser(Convert.ToInt32(DbAdminLog.NewValue));
            string userString = user == null ? string.Empty : user.FullName;
            Document document = DocumentManager.Instance.GetDocument((int)DbAdminLog.ReferenceId);
            string documentString = document == null ? string.Empty : document.Name;

            return string.Format(ResourcesLogging.DocumentHaveUserRight, documentString, userString);
        }

        protected override string Name() { return ResourcesLogging.UserRight; }
    }
}
