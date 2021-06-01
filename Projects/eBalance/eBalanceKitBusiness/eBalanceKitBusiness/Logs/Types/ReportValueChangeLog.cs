using System;
using DbAccess;
using Taxonomy.Enums;
using Taxonomy.Interfaces;
using eBalanceKitBusiness.Logs.DbMapping;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Rights;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitResources.Localisation;

namespace eBalanceKitBusiness.Logs.Types {
    internal class ReportValueChangeLog : LogEntryValueChangeBase {
        public ReportValueChangeLog(int documentId, DbReportValueChangeLog dbReportValueChangeLog = null) {
            DocumentId = documentId;
            DbReportValueChangeLog = dbReportValueChangeLog;
        }

        public DbReportValueChangeLog DbReportValueChangeLog { get; private set; }
        private int DocumentId { get; set; }
        public override DateTime Timestamp { get { return DbReportValueChangeLog.TimeStamp; } }
        protected override User User { get { return UserManager.Instance.GetUser(DbReportValueChangeLog.UserId); } }


        public override void SaveToDb(IDatabase conn) {
            DbReportValueChangeLog.SetTableName(DocumentId, conn);
            conn.DbMapping.Save(DbReportValueChangeLog);
        }

        protected override string GenerateMessage() {
            var taxonomyIdManager = DocumentManager.Instance.GetDocument(DocumentId).TaxonomyIdManager;
            string taxString = "\"" +
                               taxonomyIdManager.GetElement(DbReportValueChangeLog.TaxonomyId).MandatoryLabel + "\" (" +
                               taxonomyIdManager.GetElement(DbReportValueChangeLog.TaxonomyId).Id + ")";
            switch (DbReportValueChangeLog.ValueType) {
                case DbReportValueChangeLog.ValueTypes.AddToList:
                    return string.Format(ResourcesLogging.ListEntryNew, taxString);
                case DbReportValueChangeLog.ValueTypes.DeleteFromList:
                    return string.Format(ResourcesLogging.ListEntryRemoved, taxString);
                case DbReportValueChangeLog.ValueTypes.IsManualValue:
                    if (DbReportValueChangeLog.NewValue == null)
                        return string.Format(ResourcesLogging.ChangeManualValue, taxString, ResourcesLogging.changed);
                    else
                        return string.Format(ResourcesLogging.ChangeManualValue, taxString,
                               (DbReportValueChangeLog.NewValue == "True" ? ResourcesLogging.added : ResourcesLogging.removed));
                case DbReportValueChangeLog.ValueTypes.OtherValue:
                case DbReportValueChangeLog.ValueTypes.Value:
                case DbReportValueChangeLog.ValueTypes.ManualValue:
                    return string.Format(ResourcesLogging.ChangedValue, taxString);
                default:
                    return ResourcesLogging.LogEntryLoadingNotFinished;
            }
        }

        public override int UserId() { return DbReportValueChangeLog.UserId; }

        //public override void Read(IDatabase conn) {
        //    List<DbReportValueChangeLog> entry = conn.DbMapping.Load<DbReportValueChangeLog>(conn.Enquote("id") + "=" + MainLog.ActionId);
        //    if (entry.Count == 1) DbReportValueChangeLog = entry[0];
        //    else throw new Exception("Konnte den Log-Datensatz nicht lesen.");
        //}

        //public override void EraseUndo(IDatabase conn) {
        //    base.EraseUndo(conn);
        //    DbAdminLog.OldValue = null;
        //    DbAdminLog.NewValue = null;
        //    conn.DbMapping.Save(DbAdminLog);
        //}

        public override string GetOldValue {
            get {
                //return
                //    DocumentManager.Instance.CurrentDocument.TaxonomyIdManager.GetElement(
                //        DbReportValueChangeLog.OldValue).Label;
                return DbReportValueChangeLog.OldValue ?? string.Empty;
            }
        }

        public override string GetNewValue {
            get {
                return DbReportValueChangeLog.NewValue ?? string.Empty;
            }
        }
    }
}