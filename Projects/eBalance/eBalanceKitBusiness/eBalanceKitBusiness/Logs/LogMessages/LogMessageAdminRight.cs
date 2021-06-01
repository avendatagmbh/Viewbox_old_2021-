// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-05-07
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Globalization;
using eBalanceKitBusiness.Logs.DbMapping;
using eBalanceKitBusiness.Logs.Enums;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Rights;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Structures.DbMapping.Rights;
using eBalanceKitResources.Localisation;

namespace eBalanceKitBusiness.Logs.LogMessages {
    class LogMessageAdminRight : LogMessageAdmin {
        public LogMessageAdminRight(DbAdminLog dbAdminLog) : base(dbAdminLog) { }

        #region Overrides of LogMessageAdmin

        protected override string ContentMessage() {
            return string.Format(ResourcesLogging.RightHasBeen, GetRole(), GetReferenceString());
        }
        
        protected override string Name() { return ResourcesLogging.RightName; }
        #endregion

        #region private methods

        private string GetRole() {
            int roleId = Convert.ToInt32(LogEntryBase.GetAttribute(DbAdminLog.Info, "RoleId"));

            Role role = RoleManager.GetRole(roleId);
            if (role != null) {
                if (role.DbRole.UserId.HasValue) {
                    return ResourcesCommon.User + " " + UserManager.Instance.GetUser(role.DbRole.UserId.Value).DisplayString;
                }
                return ResourcesLogging.RoleName + " " + role.DisplayString;
            }
            return ResourcesLogging.DeletedRole;
        }

        public override string GetOldValue() {
            return string.Empty;
        }

        public override string GetNewValue() {
            return string.Empty;
        }

        protected override string ActionMessage() {
            if (DbAdminLog.ActionType == ActionTypes.Delete) {
                return ResourcesLogging.SetToDefault;
            }
            DbRight oldRight = DbAdminLog.OldValue == null ? new DbRight(0) : new DbRight(Int32.Parse(DbAdminLog.OldValue));
            DbRight newRight = DbAdminLog.NewValue == null ? new DbRight(0) : new DbRight(Int32.Parse(DbAdminLog.NewValue));
            string grant = null;
            if (oldRight.Grant.HasValue != newRight.Grant.HasValue || oldRight.Grant != newRight.Grant) {
                string grantValue = ResourcesLogging.NotSet;
                if (newRight.Grant.HasValue) {
                    grantValue = newRight.Grant.Value ? ResourcesLogging.Granted : ResourcesLogging.Forbidden;
                }
                grant = string.Format(ResourcesLogging.GrantModified, grantValue);
            }
            string write = null;
            if (oldRight.Write.HasValue != newRight.Write.HasValue || oldRight.Write != newRight.Write) {
                string writeValue = ResourcesLogging.NotSet;
                if (newRight.Write.HasValue) {
                    writeValue = newRight.Write.Value ? ResourcesLogging.Granted : ResourcesLogging.Forbidden;
                }
                write = string.Format(ResourcesLogging.WriteModified, writeValue);
            }
            string read = null;
            if (oldRight.Read.HasValue != newRight.Read.HasValue || oldRight.Read != newRight.Read) {
                string readValue = ResourcesLogging.NotSet;
                if (newRight.Read.HasValue) {
                    readValue = newRight.Read.Value ? ResourcesLogging.Granted : ResourcesLogging.Forbidden;
                }
                read = string.Format(ResourcesLogging.ReadModified, readValue);
            }
            string export = null;
            if (oldRight.Export.HasValue != newRight.Export.HasValue || oldRight.Export != newRight.Export) {
                string exportValue = ResourcesLogging.NotSet;
                if (newRight.Export.HasValue) {
                    exportValue = newRight.Export.Value ? ResourcesLogging.Granted : ResourcesLogging.Forbidden;
                }
                export = string.Format(ResourcesLogging.ExportModified, exportValue);
            }
            string send = null;
            if (oldRight.Send.HasValue != newRight.Send.HasValue || oldRight.Send != newRight.Send) {
                string sendValue = ResourcesLogging.NotSet;
                if (newRight.Send.HasValue) {
                    sendValue = newRight.Send.Value ? ResourcesLogging.Granted : ResourcesLogging.Forbidden;
                }
                send = string.Format(ResourcesLogging.SendModified, sendValue);
            }
            return ResourcesLogging.ChangedTo + write + read + grant + export + send + ")";
        }

        private string GetReferenceString() {

            var contentTypeString = LogEntryBase.GetAttribute(DbAdminLog.Info, "ContentType");
            var contentIdString = LogEntryBase.GetAttribute(DbAdminLog.Info, "ReferenceId");

            if (string.IsNullOrEmpty(contentTypeString)) {
                return ResourcesLogging.UnknownObject;
            }

            var contentType = (DbRight.ContentTypes) Convert.ToInt32(contentTypeString);
            var contentId = Convert.ToInt32(contentIdString);
            string name = string.Empty;
            string replacementForDeletedObject = "<" + ResourcesLogging.removed + ">";

            switch (contentType) {
                case DbRight.ContentTypes.All:
                    name = ResourcesLogging.Everything;
                    break;
                case DbRight.ContentTypes.System:
                    name = ResourcesMain.System + " ";
                    var system = Manager.SystemManager.Instance.GetSystem(contentId);
                    name += system == null ? replacementForDeletedObject : system.Name;
                    break;
                case DbRight.ContentTypes.Company:
                    name = ResourcesMain.Company + " ";
                    var company = Manager.CompanyManager.Instance.GetCompany(contentId);
                    name += company == null ? replacementForDeletedObject : company.Name;
                    break;
                case DbRight.ContentTypes.FinancialYear:
                    string companyName = null;
                    FinancialYear financialYear = null;
                    foreach (Company comp in CompanyManager.Instance.Companies) {
                        financialYear = comp.GetMaybeFinancialYear(contentId);
                        if (financialYear == null) continue;
                        companyName = comp.DisplayString;
                        break;
                    }
                    if (companyName == null) {
                        name = ResourcesLogging.DeletedFinancialYearOrCompany;
                        break;
                    }
                    name = ResourcesMain.FinancialYear + " " + financialYear.FYear.ToString(CultureInfo.InvariantCulture) +
                        " of company \"" + companyName + "\"";
                    break;
                case DbRight.ContentTypes.Document:
                    name = ResourcesMain.Report + " ";
                    var doc = Manager.DocumentManager.Instance.GetDocument(contentId);
                    name += doc == null ? replacementForDeletedObject : doc.Name;
                    break;
                case DbRight.ContentTypes.DocumentSpecialRight_TransferValueCalculation:
                    name = ResourcesLogging.TransferValueCalculation;
                    break;
                case DbRight.ContentTypes.DocumentSpecialRight_Rest:
                    name = ResourcesLogging.OtherReportParts;
                    break;
            }
            return name;
        }

        #endregion

    }
}