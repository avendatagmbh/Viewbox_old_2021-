using System;
using System.Diagnostics;
using eBalanceKitBusiness.Logs.DbMapping;
using eBalanceKitBusiness.Logs.Enums;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Manager;
using eBalanceKitResources.Localisation;

namespace eBalanceKitBusiness.Logs.LogMessages {
    class LogMessageAdminCompany  : LogMessageAdmin{
        public LogMessageAdminCompany(DbAdminLog dbAdminLog)
            : base(dbAdminLog) {

        }
        
        protected override string ContentMessage() {
            if (DbAdminLog.ActionType == ActionTypes.Delete) {
                string name = LogEntryBase.GetAttribute(DbAdminLog.Info, "Name");
                return string.Format(ResourcesLogging.CompanyHasBeen, name);
            }

            if (DbAdminLog.ActionType == ActionTypes.Change) {
                try {
                    Company company = CompanyManager.Instance.GetCompany((int)DbAdminLog.ReferenceId);
                    string companyString = company == null ? string.Empty : company.Name;
                    int taxId = Convert.ToInt32(LogEntryBase.GetAttribute(DbAdminLog.Info, "TId"));
                    
                    Debug.Assert(company != null, "company != null");

                    return string.Format(ResourcesLogging.FieldOfCompanyHasBeen, LogEntryBase.TaxonomyStringFormatted(company.TaxonomyIdManager, taxId), companyString);
                } catch (Exception) { }
            }
            return ResourcesLogging.CompanyUnknownHasBeen;
        }

        protected override string Name() { return ResourcesMain.Company; }
    }
}
