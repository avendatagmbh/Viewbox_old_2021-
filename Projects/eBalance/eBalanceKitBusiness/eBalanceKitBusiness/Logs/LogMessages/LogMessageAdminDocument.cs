// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-07-07
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Globalization;
using eBalanceKitBusiness.Logs.DbMapping;
using eBalanceKitBusiness.Logs.Enums;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitResources.Localisation;

namespace eBalanceKitBusiness.Logs.LogMessages {
    internal class LogMessageAdminDocument : LogMessageAdmin {
        public LogMessageAdminDocument(DbAdminLog dbAdminLog)
            : base(dbAdminLog) { }


        protected override string TranslateField(string type) {
            switch (type) {
                case "FinancialYearId":
                    return ResourcesMain.FinancialYear;
                case "CompanyId":
                    return ResourcesMain.Company;
                case "SystemId":
                    return ResourcesMain.System;
                default:
                    return base.TranslateField(type);
            }
        }

        protected override string ContentMessage() {
            if (DbAdminLog.ActionType == ActionTypes.Delete) {
                string name = LogEntryBase.GetAttribute(DbAdminLog.Info, "Name");
                return string.Format(ResourcesLogging.DocumentHasBeen, name);
            }
            if (DbAdminLog.ActionType == ActionTypes.Change) {
                try {
                    Document document = DocumentManager.Instance.GetDocument((int)DbAdminLog.ReferenceId);
                    string documentString = document == null ? string.Empty : document.Name;
                    string type = LogEntryBase.GetAttribute(DbAdminLog.Info, "Type");
                    return string.Format(ResourcesLogging.FieldOfDocumentHasBeen, TranslateField(type), documentString);
                } catch (Exception) {
                }
            }
            return ResourcesLogging.DocumentUnknownHasBeen;
        }

        //private Document _document;

        private string GetLogValue(string oldNewValue) {
            if (DbAdminLog.Info != null) {
                if (LogEntryBase.GetAttribute(DbAdminLog.Info, "Type") == "TaxonomyInfoId") {
                    //int documentId = (int) DbAdminLog.ReferenceId;
                    //_document = DocumentManager.Instance.GetDocument(documentId);
                    //_document.Init();
                    //_document.ReportRights = new ReportRights(_document);
                    //_document.LoadDetails(null);
                    //XbrlElementValue_SingleChoice singleChoice =
                    //    _document.ValueTreeGcd.Root.Values["de-gcd_genInfo.report.id.specialAccountingStandard"] as
                    //    XbrlElementValue_SingleChoice;
                    //Debug.Assert(singleChoice != null);
                    //return singleChoice.Elements[Int32.Parse(DbAdminLog.OldValue)].Label;
                    return TaxonomyManager.GetTaxonomyInfo(Int32.Parse(oldNewValue)).Name;
                }
                if (LogEntryBase.GetAttribute(DbAdminLog.Info, "Type") == "CompanyId") {
                    Company maybeCompany = CompanyManager.Instance.GetCompany(Int32.Parse(oldNewValue));
                    return  maybeCompany == null ? ResourcesLogging.DeletedCompany : maybeCompany.DisplayString;
                }
                if (LogEntryBase.GetAttribute(DbAdminLog.Info, "Type") == "FinancialYearId") {
                    using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                        FinancialYear financialYear = conn.DbMapping.Load<FinancialYear>(Int32.Parse(oldNewValue));
                        return financialYear.FYear.ToString(CultureInfo.InvariantCulture);
                    }
                }
                if (LogEntryBase.GetAttribute(DbAdminLog.Info, "Type") == "SystemId") {
                    var maybeSystem = SystemManager.Instance.GetSystem(Int32.Parse(oldNewValue));
                    return maybeSystem == null ? ResourcesLogging.DeletedSystem : maybeSystem.DisplayString;
                }
            }
            return null;
        }

        public override string GetOldValue() {
            return GetLogValue(DbAdminLog.OldValue) ?? base.GetOldValue();
        }

        public override string GetNewValue() {
            return GetLogValue(DbAdminLog.NewValue) ?? base.GetNewValue();
        }

        protected override string Name() { return ResourcesMain.Report; }
    }
}