// --------------------------------------------------------------------------------
// author: Benjamin Held / Mirko Dibbert
// since: 2011-07-06
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Diagnostics;
using System.Linq;
using DbAccess;
using eBalanceKitBusiness.Logs.DbMapping;
using eBalanceKitBusiness.Logs.Enums;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Reconciliation;
using eBalanceKitBusiness.Reconciliation.Enums;
using eBalanceKitBusiness.Reconciliation.Interfaces;
using eBalanceKitBusiness.Reconciliation.Interfaces.ReconciliationTypes;
using eBalanceKitBusiness.Rights;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Structures.XbrlElementValue;
using eBalanceKitResources.Localisation;

namespace eBalanceKitBusiness.Logs.Types {
    internal class ReportLog : LogEntryValueChangeBase {
        public ReportLog(int documentId, DbReportLog dbReportLog = null) {
            DocumentId = documentId;
            DbReportLog = dbReportLog;
        }

        public DbReportLog DbReportLog { get; private set; }
        private int DocumentId { get; set; }
        public override DateTime Timestamp { get { return DbReportLog.TimeStamp; } }

        protected override User User { get { return UserManager.Instance.GetUser(DbReportLog.UserId); } }
        
        public override void SaveToDb(IDatabase conn) {
            DbReportLog.SetTableName(DocumentId, conn);
            conn.DbMapping.Save(DbReportLog);
        }

        private string AccountContentMessage() {
            string contentMessage;
            if (DbReportLog.ReferenceId != 0) {
                switch (DbReportLog.ActionType) {
                    case ActionTypes.Delete:
                        contentMessage = ResourcesLogging.AccountFrom +
                                         TaxonomyStringFormatted(DocumentManager.Instance.GetDocument(DocumentId).TaxonomyIdManager,
                                                                 Convert.ToInt32(DbReportLog.OldValue));
                        break;
                    case ActionTypes.Change:
                        contentMessage = ResourcesLogging.AccountFrom +
                                         TaxonomyStringFormatted(DocumentManager.Instance.GetDocument(DocumentId).TaxonomyIdManager,
                                                                 Convert.ToInt32(DbReportLog.OldValue)) +
                                         ResourcesLogging.ToPosition +
                                         TaxonomyStringFormatted(DocumentManager.Instance.GetDocument(DocumentId).TaxonomyIdManager,
                                                                 Convert.ToInt32(DbReportLog.NewValue));
                        break;
                    default:
                        contentMessage = ResourcesLogging.AccountTo +
                                         TaxonomyStringFormatted(DocumentManager.Instance.GetDocument(DocumentId).TaxonomyIdManager,
                                                                 Convert.ToInt32(DbReportLog.NewValue));
                        break;
                }
            } else {
                contentMessage = ResourcesLogging.AccountsHasBeen;
            }
            return contentMessage;
        }

        protected override string GenerateMessage() {
            string contentMessage = null;
            switch (DbReportLog.ReportLogContentType) {
                case ReportLogContentTypes.Account:
                    contentMessage = AccountContentMessage();
                    break;
                case ReportLogContentTypes.BalanceList:
                    contentMessage = ResourcesLogging.BalanceListHasBeen;
                    break;
                case ReportLogContentTypes.Template:
                    contentMessage = ResourcesLogging.TemplateHasBeen;
                    break;
                case ReportLogContentTypes.TransferValue:
                    int taxId;
                    try {
                        taxId = Convert.ToInt32(GetAttribute(DbReportLog.Info, "tId"));
                    } catch (Exception) {
                        contentMessage = "Ein Transferwert wurde";
                        break;
                    }
                    string whichYear = GetAttribute(DbReportLog.Info, "Type") == "TVPY"
                                           ? ResourcesLogging.LastYear
                                           : ResourcesLogging.CurrentYear;
                    contentMessage = string.Format(ResourcesLogging.ReconciliationValueHasBeen, whichYear +
                                     TaxonomyStringFormatted(DocumentManager.Instance.GetDocument(DocumentId).TaxonomyIdManager,
                                                             taxId));
                    break;

                case ReportLogContentTypes.HyperCube:
                    contentMessage = ResourcesLogging.TableHasBeen;
                    break;
                case ReportLogContentTypes.Document:
                    contentMessage = ResourcesLogging.Taxonomy;
                    break;
                case ReportLogContentTypes.Reconciliation:
                    IReconciliationManagerManagement mgr = new ReconciliationManager();
                    string typeString = DbReportLog.Info == null ? null : GetAttribute(DbReportLog.Info, "Type");
                    string name = mgr.GetReconciliationNameManagement(DbReportLog.ReferenceId.ToString());
                    if (!string.IsNullOrEmpty(typeString)) {
                        ReconciliationTypes actualType =
                            (ReconciliationTypes) Enum.Parse(typeof (ReconciliationTypes), typeString);
                        switch (actualType) {
                            case ReconciliationTypes.Reclassification:
                                contentMessage = string.Format(ResourcesLogging.ReconciliationValueHasBeen, ResourcesLogging.Reclassification, name);
                                break;
                            
                            case ReconciliationTypes.PreviousYearValues:
                            case ReconciliationTypes.ValueChange:
                                contentMessage = string.Format(ResourcesLogging.ReconciliationValueHasBeen, ResourcesLogging.ValueChange, name);
                                break;

                            case ReconciliationTypes.AuditCorrection:
                            case ReconciliationTypes.AuditCorrectionPreviousYear:
                            case ReconciliationTypes.Delta:
                                contentMessage = string.Format(ResourcesLogging.ReconciliationValueHasBeen, actualType, name);
                                break;
                            
                            case ReconciliationTypes.ImportedValues:
                                contentMessage = string.Format(ResourcesLogging.ReconciliationValueHasBeen, ResourcesLogging.ImportedValues, name);
                                break;

                            case ReconciliationTypes.TaxBalanceValue:
                                contentMessage = string.Format(ResourcesLogging.ReconciliationValueHasBeen, ResourcesLogging.TaxBalanceValue,
                                                               name);
                                break;
                            
                            default:
                                // TODO: add a documentation somewhere what are the dependecies of extending a class. For example extending Reconciliation have affect in
                                // the logging here.
                                throw new NotImplementedException();
                        }
                    } else {
                        if (DbReportLog.ActionType == ActionTypes.Delete) {
                            contentMessage = string.Format(ResourcesLogging.ReconciliationValueHasBeen, ResourcesReconciliation.Reconciliation, name);
                            break;
                        }
                        string changedPropertyName = DbReportLog.Info == null
                                                         ? null
                                                         : GetAttribute(DbReportLog.Info, "ChangedProperty");
                        if (!string.IsNullOrEmpty(changedPropertyName)) {
                            contentMessage = string.Format(ResourcesLogging.ReconciliationPropertyHasBeen,
                                                           changedPropertyName, name);
                            break;
                        }
                        throw new NotImplementedException();
                    }
                    break;
                case ReportLogContentTypes.ReconciliationTransaction:
                    // TODO: show reconciliation transaction
                    contentMessage = "Reconciliation transaction";
                    break;
            }
            string actionType = null;
            switch (DbReportLog.ActionType) {
                case ActionTypes.New:
                    actionType = " " + ResourcesLogging.added.ToLower() + ".";
                    break;
                case ActionTypes.Delete:
                    actionType = " " + ResourcesLogging.removed.ToLower() + ".";
                    break;
                case ActionTypes.Change:
                    if (DbReportLog.ReportLogContentType == ReportLogContentTypes.Account)
                        actionType = " " + ResourcesLogging.moved.ToLower() + ".";
                    else
                        actionType = " " + ResourcesLogging.changed.ToLower() + ".";
                    break;
                case ActionTypes.Used:
                    actionType = " " + ResourcesLogging.used.ToLower() + ".";
                    break;

                case ActionTypes.Imported:
                    actionType = " " + ResourcesLogging.imported.ToLower() + ".";
                    break;
            }
            return contentMessage + actionType;
        }

        public override int UserId() { return DbReportLog.UserId; }

        public override string GetOldValue { get {
            if ((DbReportLog.ReportLogContentType == ReportLogContentTypes.Template && DbReportLog.ActionType == ActionTypes.New) ||
                DbReportLog.ReportLogContentType == ReportLogContentTypes.Account) {
                return string.Empty;
            }
            return DbReportLog.OldValue ?? string.Empty;
        } }

        public override string GetNewValue { get {
            if ((DbReportLog.ReportLogContentType == ReportLogContentTypes.Template && DbReportLog.ActionType == ActionTypes.New) ||
                DbReportLog.ReportLogContentType == ReportLogContentTypes.Account) {
                return string.Empty;
            }
            return DbReportLog.NewValue ?? string.Empty;
        } }
    }
}