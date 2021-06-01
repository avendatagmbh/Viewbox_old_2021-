// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-05-23
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Utils;
using eBalanceKitBase.Windows;
using eBalanceKitBusiness.AuditCorrections;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Reconciliation.Interfaces.ReconciliationTypes;
using eBalanceKitBusiness.Structures;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Windows.AuditMode.Models {
    public class TransferAuditCorrectionsModel : NotifyPropertyChangedBase {

        public TransferAuditCorrectionsModel(Window owner, IAuditCorrection correction, IReconciliation reconciliation) {
            Owner = owner;
            Correction = correction;                      
            Reconciliation = reconciliation;

            SelectNextYearTree = new SelectNextYearTree();

            if (DocumentManager.Instance.CurrentDocument != null) /* workaround due to designer exception */ {

                if (correction == null && reconciliation == null) {
                    AuditCorrections = new CheckableAuditCorrectionTree(
                        DocumentManager.Instance.CurrentDocument.AuditCorrectionManager.PositionCorrections);

                    AuditCorrectionsReconciliation = new CheckableReconciliationTree(
                        DocumentManager.Instance.CurrentDocument.ReconciliationManager.AuditCorrections);

                } else if (correction != null && reconciliation == null) {
                    AuditCorrections = new CheckableAuditCorrectionTree(new[] {correction});


                } else if (correction == null /*&& reconciliation != null*/) {
                    AuditCorrectionsReconciliation = new CheckableReconciliationTree(new[] { reconciliation });
                
                } else throw new NotImplementedException();
            }
        }

        private IAuditCorrection Correction { get; set; }
        private IReconciliation Reconciliation { get; set; }
        
        private Window Owner { get; set; }

        public SelectNextYearTree SelectNextYearTree { get; private set; }
        public CheckableAuditCorrectionTree AuditCorrections { get; private set; }
        public CheckableReconciliationTree AuditCorrectionsReconciliation { get; private set; }

        /// <summary>
        /// Transfering the AuditCorrectionValues and listing occured Exceptions in the <see cref="Problems"/>.
        /// If no Exceptions occured <see cref="Problems"/> is NULL.
        /// </summary>
        public void TransferAuditCorrectionValues() {
            var auditCorrectionTransactions = AuditCorrections == null ? null : AuditCorrections.GetCheckedTransactions();
            var reconciliationTransactions = AuditCorrectionsReconciliation == null ? null : AuditCorrectionsReconciliation.GetCheckedTransactions();
            
            var progress = new DlgProgress(Owner) {ProgressInfo = {IsIndeterminate = true}};
            var progressInfo = progress.ProgressInfo;
            bool success = true;

            Problems = null;

            progress.ExecuteModal(() => {
                success = DocumentManager.Instance.CurrentDocument.AuditCorrectionManager.TransferAuditCorrectionValues
                    (
                        progressInfo, SelectNextYearTree.GetCheckedDocuments(), auditCorrectionTransactions,
                        reconciliationTransactions);
            });
            
            //var newAuditCorrectionsDict = new Dictionary<Document, Dictionary<IAuditCorrection, IAuditCorrection>>();
            
            //progress.ExecuteModal(() => {
            //    foreach (var document in SelectNextYearTree.GetCheckedDocuments()) {
            //        Dictionary<IAuditCorrection, IAuditCorrection> newAuditCorrections;
            //        progressInfo.Caption = string.Format(ResourcesAuditCorrections.ProgressDocumentExecuting,document.Name);
            //        success &= DocumentManager.Instance.CurrentDocument.AuditCorrectionManager.TransferAuditCorrectionValues(
            //            progressInfo, document, auditCorrectionTransactions, reconciliationTransactions, out newAuditCorrections);

            //        newAuditCorrectionsDict[document] = newAuditCorrections;
            //    }
            //});

            //var results = new TransferAuditCorrectionResults(newAuditCorrectionsDict);

            if (!success) {
                Problems = new ProblemSummary();
                foreach (var error in DocumentManager.Instance.CurrentDocument.AuditCorrectionManager.Errors) {
                    Problems.AddCategory(new ProblemCategory(error.Key, new ProblemEntry(error.Value.Message)));
                }
            }
        }

        public void CheckAuditCorrectionValues() {

            var auditCorrectionTransactions = AuditCorrections == null ? null : AuditCorrections.GetCheckedTransactions();
            var reconciliationTransactions =  AuditCorrectionsReconciliation == null ? null : AuditCorrectionsReconciliation.GetCheckedTransactions();
            
            var progress = new DlgProgress(Owner) { ProgressInfo = { IsIndeterminate = true } };
            var progressInfo = progress.ProgressInfo;
            ProblemSummary problemList = new ProblemSummary(ResourcesAuditCorrections.CheckProblemPdfHead);
            if (TransactionSelected && SelectNextYearTree.CheckedDocumentsCount > 0) {

                progress.ExecuteModal(() => {
                    foreach (var document in SelectNextYearTree.GetCheckedDocuments()) {
                        progressInfo.Caption = string.Format(ResourcesAuditCorrections.ProgressDocumentChecking,
                                                             document.Name);
                        problemList.AddCategory(DocumentManager.Instance.CurrentDocument.AuditCorrectionManager.
                                                    CheckAuditCorrectionValues(
                                                        progressInfo, document, auditCorrectionTransactions,
                                                        reconciliationTransactions));
                    }
                });
            } else {
                var generalProblems = new ProblemCategory(ResourcesCommon.General);
                if (!TransactionSelected) {
                    generalProblems.AddProblem(new ProblemEntry(ResourcesAuditCorrections.TransferMessageNoTransactionSelected));
                }

                if (SelectNextYearTree.CheckedDocumentsCount <= 0) {
                    generalProblems.AddProblem(new ProblemEntry(ResourcesAuditCorrections.TransferMessageNoReportSelected));
                }
                problemList.AddCategory(generalProblems);
            }
            Problems = problemList;
        }

        #region Problems
        private ProblemSummary _problems;

        public ProblemSummary Problems {
            get { return _problems; }
            set {
                if (_problems != value) {
                    _problems = value;
                    OnPropertyChanged("Problems");
                }
            }
        }
        #endregion Problems

        public bool OkButtonEnabled {
            get {
                //return true;
                return SelectNextYearTree.CheckedDocumentsCount > 0 && TransactionSelected;

                // TODO: return if transfer is allowed:
                // at least one document selected
                // at least one allowed transaction selected:
                //  - at least one AuditCorrectionTransaction selected and WriteRestAllowed for at least one Document
                //  - at least one ReconciliationTransaction selected and WriteTransferValuesAllowed for at least one Document
            }
        }

        public bool TransactionSelected {
            get {
                return ((AuditCorrectionsReconciliation != null && AuditCorrectionsReconciliation.GetCheckedTransactions().Any()) ||
                        (AuditCorrections != null && AuditCorrections.GetCheckedTransactions().Any()));
            }
        }
    }
}