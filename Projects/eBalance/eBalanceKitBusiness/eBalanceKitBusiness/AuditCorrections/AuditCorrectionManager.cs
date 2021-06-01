// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-05-15
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Utils;
using eBalanceKitBase.Structures;
using eBalanceKitBusiness.AuditCorrections.DbMapping;
using eBalanceKitBusiness.Exceptions;
using eBalanceKitBusiness.Localisation;
using eBalanceKitBusiness.Logs;
using eBalanceKitBusiness.Reconciliation;
using eBalanceKitBusiness.Reconciliation.DbMapping;
using eBalanceKitBusiness.Reconciliation.Enums;
using eBalanceKitBusiness.Reconciliation.Interfaces;
using eBalanceKitBusiness.Reconciliation.ReconciliationTypes;
using eBalanceKitBusiness.Rights;
using eBalanceKitBusiness.Structures;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitResources.Localisation;

namespace eBalanceKitBusiness.AuditCorrections {
    internal class AuditCorrectionManager : NotifyPropertyChangedBase, IAuditCorrectionManager {

        internal AuditCorrectionManager(Document document) {
            Document = document;
            LoadDbEntities();
            Errors = new Dictionary<string, Exception>();
        }

        private Document Document { get; set; }

        private void LoadDbEntities() {
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                try {
                    conn.DbMapping.Load<DbEntityAuditCorrection>(conn.Enquote("document_id") + "=" + Document.Id).
                        ForEach(dbEntity => {
                            dbEntity.Document = Document;
                            dbEntity.Transactions.ForEach(t => t.Document = Document);

                            _positionCorrections.Add(new AuditCorrection(Document, dbEntity));
                        });
                } catch (Exception ex) {
                    throw new Exception(ResourcesReconciliation.LoadException + ex.Message, ex);
                }
            }
        }

        #region PositionCorrections
        private readonly ObservableCollectionAsync<IAuditCorrection> _positionCorrections =
            new ObservableCollectionAsync<IAuditCorrection>();

        public IEnumerable<IAuditCorrection> PositionCorrections { get { return _positionCorrections; } }
        #endregion // PositionCorrections

        #region SelectedPositionCorrection
        private IAuditCorrection _selectedPositionCorrection;

        public IAuditCorrection SelectedPositionCorrection {
            get { return _selectedPositionCorrection; }
            set {
                if (_selectedPositionCorrection == value) return;
                _selectedPositionCorrection = value;
                OnPropertyChanged("SelectedPositionCorrection");
                OnPropertyChanged("DeletePositionsCorrectionValueAllowed");
                OnPropertyChanged("ShowAssignButton");
            }
        }
        #endregion // SelectedPositionCorrection

        public bool AddPositionsCorrectionValueAllowed { get { return Document.ReportRights.WriteRestAllowed && PositionCorrections != null; } }

        public bool DeletePositionsCorrectionValueAllowed { get { return Document.ReportRights.WriteRestAllowed && SelectedPositionCorrection != null; } }

        public bool ShowAssignButton { get { return SelectedPositionCorrection != null; } }

        #region AddPositionCorrection
        public IAuditCorrection AddPositionCorrection() {
            if (!Document.ReportRights.WriteRestAllowed)
                throw new AccessDeniedException(ExceptionMessages.InsufficentWriteRights);

            var result = new AuditCorrection(Document);
            _positionCorrections.Add(result);
            result.Save();
            SelectedPositionCorrection = result;
            LogManager.Instance.NewAuditCorrection(result);
            return result;
        }
        #endregion // AddPositionCorrection

        #region DeleteSelectedPositionCorrection
        public void DeleteSelectedPositionCorrection() {
            if (!Document.ReportRights.WriteRestAllowed)
                throw new AccessDeniedException(ExceptionMessages.InsufficentWriteRights);

            if (SelectedPositionCorrection != null)
                DeletePositionCorrection(SelectedPositionCorrection);

            SelectedPositionCorrection = null;
        }
        #endregion // DeleteSelectedPositionCorrection

        #region DeletePositionCorrection
        public void DeletePositionCorrection(IAuditCorrection correction) {
            if (!Document.ReportRights.WriteRestAllowed)
                throw new AccessDeniedException(ExceptionMessages.InsufficentWriteRights);

            foreach (var transaction in correction.Transactions.ToList()) transaction.Remove();
            ((AuditCorrection) correction).Delete();
            _positionCorrections.Remove(correction);
        }
        #endregion // DeletePositionCorrection

        /// <summary>
        /// Deletes all corrections.
        /// </summary>
        public void DeleteAllCorrections() { DeleteAllCorrections(true); }

        /// <summary>
        /// Deletes all corrections whereas logging could be optionally disabled when called from another internal methods which logs the superior action.
        /// </summary>
        /// <param name="doLog"></param>
        internal void DeleteAllCorrections(bool doLog) {
            if (!Document.ReportRights.WriteRestAllowed)
                throw new AccessDeniedException(ExceptionMessages.InsufficentWriteRights);

            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                try {
                    conn.ExecuteNonQuery("DELETE FROM " + conn.Enquote("audit_correction") + " WHERE " + conn.Enquote("document_id") + "=" + Document.Id);
                    conn.ExecuteNonQuery("DELETE FROM " + conn.Enquote("audit_correction_transaction") + " WHERE " + conn.Enquote("document_id") + "=" + Document.Id);
                } catch (Exception ex) {
                    throw new Exception(ResourcesReconciliation.DeleteException + ex.Message, ex);
                }
            }

            _positionCorrections.Clear();

            if (doLog) LogManager.Instance.DeleteAllAuditCorrections(Document);
        }

        #region TransferAuditCorrectionValues
        /// <summary>
        /// Transfers the specified audit corrections to the specified target document. This process replaces all existing audit corrections in the target document.
        /// </summary>
        /// <param name="progressInfo"> </param>
        /// <param name="targetDocuments"> </param>
        /// <param name="auditCorrectionTransactions"></param>
        /// <param name="reconciliationTransactions"></param>
        public bool TransferAuditCorrectionValues(
            ProgressInfo progressInfo,
            IEnumerable<Document> targetDocuments,
            IEnumerable<IAuditCorrectionTransaction> auditCorrectionTransactions,
            IEnumerable<IReconciliationTransaction> reconciliationTransactions) {

            var targetDocumentList = targetDocuments.ToList();
            var auditCorrectionTransactionList = auditCorrectionTransactions.OrderBy(t => t.Parent);

            // TODO: check rights

            Errors.Clear();
            bool success = true;

            try {
                // load document details
                foreach (var document in targetDocumentList) {
                    try {
                        progressInfo.Caption = string.Format(
                            ResourcesAuditCorrections.ProgressDocumentLoading, document.Name);

                        document.ReportRights = new ReportRights(document);
                        document.LoadDetails(progressInfo);

                        // DEBUG
                        (document.AuditCorrectionManager).DeleteAllCorrections();

                    } catch (Exception ex) {
                        Errors.Add(document.Name, ex);
                        success = false;
                    }
                }

                // transfer audit corrections
                if (success)
                    foreach (var correction in auditCorrectionTransactionList.Select(t => t.Parent).Distinct()) {
                        AuditCorrection correction1 = (AuditCorrection)correction;

                        AuditCorrectionSet correctionSet;
                        // create new audit correction set
                        correctionSet = new AuditCorrectionSet(correction1.DbEntity.Id);

                        var transactions = auditCorrectionTransactionList.Where(t => t.Parent == correction1).ToList();

                        // add original correction to correction set
                        correctionSet.AddCorrection(correction);

                        // transfer corrections to all target documents and add the newly created corrections to the correction set.
                        foreach (var document in targetDocumentList) {
                            correctionSet.AddCorrection(TransferAuditCorrections(progressInfo, document, transactions));
                        }

                    }
            } catch (Exception ex) {
                Errors.Add(ex.Message, ex);
                success = false;

            } finally {
                // clear document details
                foreach (var document in targetDocumentList) {
                    document.ClearDetails();
                }
            }

            // TODO: transfer reconciliation corrections
            // var reconciliationTransactionList = reconciliationTransactions.OrderBy(t => t.Reconciliation);

            return success;
        }

        ///// <summary>
        ///// Transfers the specified audit corrections to the specified target document. This process replaces all existing audit corrections in the target document.
        ///// </summary>
        ///// <param name="progressInfo"> </param>
        ///// <param name="targetDocument"> </param>
        ///// <param name="auditCorrectionTransactions"></param>
        ///// <param name="reconciliationTransactions"></param>
        //public bool TransferAuditCorrectionValues(
        //    ProgressInfo progressInfo,
        //    Document targetDocument,
        //    IEnumerable<IAuditCorrectionTransaction> auditCorrectionTransactions,
        //    IEnumerable<IReconciliationTransaction> reconciliationTransactions) {
                
        //    bool success;
        //    try {
        //        targetDocument.ReportRights = new ReportRights(targetDocument);
        //        targetDocument.LoadDetails(progressInfo);
        //        if (auditCorrectionTransactions != null && targetDocument.ReportRights.WriteRestAllowed)
        //            TransferAuditCorrections(progressInfo, targetDocument, auditCorrectionTransactions);

        //        if (reconciliationTransactions != null && targetDocument.ReportRights.WriteTransferValuesAllowed)
        //            TransferAuditCorrectionsReconciliation(progressInfo, targetDocument, reconciliationTransactions);

        //        success = true;
            
        //    } catch (Exception e) {
        //        Errors.Add(targetDocument.Name, e);               
        //        success = false;
            
        //    } finally {
        //        targetDocument.ClearDetails();
        //    }

        //    return success;
        //}

        /// <summary>
        /// Dictionary that contains Category.Name (=Document.Name) as Key and occured Exception as Value
        /// </summary>
        public Dictionary<string, Exception> Errors { get; set; }

        /// <summary>
        /// Checks if the specified audit corrections is compatible with the specified target document.
        /// </summary>
        /// <param name="progressInfo"></param>
        /// <param name="targetDocument"></param>
        /// <param name="auditCorrectionTransactions"></param>
        /// <param name="reconciliationTransactions"></param>
        /// <returns>A list of problems</returns>
        public ProblemCategory CheckAuditCorrectionValues(
            ProgressInfo progressInfo,
            Document targetDocument,
            List<IAuditCorrectionTransaction> auditCorrectionTransactions,
            List<IReconciliationTransaction> reconciliationTransactions) {

            ProblemCategory problems = new ProblemCategory(targetDocument.Name);

            targetDocument.ReportRights = new ReportRights(targetDocument);
            targetDocument.LoadDetails(progressInfo);

            if (auditCorrectionTransactions != null && auditCorrectionTransactions.Any()) {
                if (targetDocument.ReportRights.WriteRestAllowed && auditCorrectionTransactions != null) {
                    problems.AddProblem(CheckTransferAuditCorrections(progressInfo, targetDocument,
                                                                    auditCorrectionTransactions));
                } else {
                    problems.AddProblem(new ProblemEntry(targetDocument.Name,
                                                  ExceptionMessages.InsufficentWriteRights + " (" +
                                                  ResourcesCommon.RightTreeNodeOtherReportsCaption + ")"));
                }
            }

            if (reconciliationTransactions != null && reconciliationTransactions.Any()) {
                if (targetDocument.ReportRights.WriteTransferValuesAllowed && reconciliationTransactions != null) {
                    problems.AddProblem(CheckTransferAuditCorrectionsReconciliation(progressInfo, targetDocument, reconciliationTransactions));
                } else {
                    problems.AddProblem(new ProblemEntry(targetDocument.Name,
                                                  ExceptionMessages.InsufficentWriteRights + " (" +
                                                  ResourcesCommon.RightTreeNodeReconciliationCaption + ")"));
                }
            }
            
            targetDocument.ClearDetails();

            return problems.Entries.Count > 0 ? problems : null;
        }


        private IEnumerable<ProblemEntry> CheckTransferAuditCorrections(
            ProgressInfo progressInfo,
            Document targetDocument,
            IEnumerable<IAuditCorrectionTransaction> transactions) {

            List<ProblemEntry> problems = new List<ProblemEntry>();
            
            foreach (var transaction in transactions.OfType<AuditCorrectionTransaction>()) {
                    if (progressInfo != null) {
                        progressInfo.Caption = string.Format(ResourcesAuditCorrections.ProgressCorrectionChecking, transaction.Parent.Name);
                    }
                
                var problem = CheckTransactionElement(transaction, targetDocument);
                if (problem != null) {
                    problems.Add(problem);
                }

            }

            return problems;
        }

        private IEnumerable<ProblemEntry> CheckTransferAuditCorrectionsReconciliation(
            ProgressInfo progressInfo,
            Document targetDocument,
            IEnumerable<IReconciliationTransaction> transactions) {

            List<ProblemEntry> problems = new List<ProblemEntry>();


            // check each transaction
            foreach (var transaction in transactions.OfType<ReconciliationTransaction>()) {
                
                var problem = CheckTransactionElement(transaction, targetDocument);
                if (problem != null) {
                    problems.Add(problem);
                }
                
                Debug.Assert(transaction.Value != null, "transaction.Value != null");
            }
            return problems;
        }

        /// <summary>
        /// Checks if tranaction.Element would have any problems if transfered from an other year.
        /// </summary>
        /// <returns>NULL if no problem is existing.</returns>
        private ProblemEntry CheckTransactionElement(AuditCorrectionTransaction transaction, Document targetDocument) {

            var gtree = targetDocument.GaapPresentationTrees.Values.FirstOrDefault(
                tree => tree.Nodes.Any(node => node.Element.Id == transaction.Element.Id));
            if (gtree != null && gtree.Nodes != null) {
                var elemNode = gtree.Nodes.OfType<Structures.Presentation.PresentationTreeNode>().FirstOrDefault(
                    node => node.Element != null && node.Element.Id == transaction.Element.Id);
                if (elemNode != null) {
                    // Check if elemNode.Value.IsEnabled is not enough
                    if (!elemNode.IsValueEditingAllowed) {
                        return new ProblemEntry(transaction.Parent.Name, string.Format(ResourcesAuditCorrections.ElementIsDisabledOrComputed, transaction.Element.Label));
                    } else {
                        return null;
                    }
                }
                else {
#if DEBUG
                    throw new Exception("This is not possible!!!");
#else
                    return new ProblemEntry("elemnode is null");
#endif
                }
            }
            else {
                return new ProblemEntry(transaction.Parent.Name, string.Format(ResourcesAuditCorrections.ProblemEntryNoGaapPresentationTree, transaction.Element.Label));
            }
        }

        private ProblemEntry CheckTransactionElement(ReconciliationTransaction reconciliationTransaction, Document targetDocument) {
            var element = Document.TaxonomyIdManager.GetElement(reconciliationTransaction.DbEntity.ElementId);
            var gtree = targetDocument.GaapPresentationTrees.Values.FirstOrDefault(
                tree => tree.Nodes.Any(node => node.Element.Id == element.Id));
            if (gtree != null && gtree.Nodes != null) {
                var elemNode = gtree.Nodes.OfType<Structures.Presentation.PresentationTreeNode>().FirstOrDefault(
                    node => node.Element != null && node.Element.Id == element.Id);

                if (elemNode != null) {
                    // ReconciliationTransactions are possible even if the element is disabled

                //    if (!elemNode.Value.IsEnabled) {
                //        return new ProblemEntry(reconciliationTransaction.Label, string.Format(ResourcesAuditCorrections.ElementIsDisabledOrComputed, elemNode.Element.Label));
                //    }
                //    else {
                        return null;
                    //}
                }
                else {
#if DEBUG
                    throw new Exception("This is not possible!!!");
#else
                    return new ProblemEntry(reconciliationTransaction.Label, string.Format(ResourcesAuditCorrections.ProblemEntryNoGaapPresentationTree, element.Label));
#endif
                }
            }
            else {
                return new ProblemEntry(reconciliationTransaction.Label, string.Format(ResourcesAuditCorrections.ProblemEntryNoGaapPresentationTree, element.Label));
            }
        }

        private IAuditCorrection TransferAuditCorrections(
            ProgressInfo progressInfo,
            Document targetDocument,
            List<IAuditCorrectionTransaction> transactions) {

            // assert that at least one transaction exists and all transactions belong to the same audit correction
            Debug.Assert(transactions.Any() && transactions.All(t => t.Parent == transactions.First().Parent));

            var origCorrection = transactions.First().Parent;

            // create new values
            var correctionEntity = new DbEntityAuditCorrection {
                Document = targetDocument,
                Name = origCorrection.Name,
                Comment = origCorrection.Comment
            };

            foreach (var transaction in transactions.OfType<AuditCorrectionTransaction>()) {
                correctionEntity.Transactions.Add(new DbEntityAuditCorrectionTransaction {
                    DbEntityAuditCorrection = correctionEntity,
                    Document = targetDocument,
                    Value = transaction.DbEntity.Value,
                    ElementId = transaction.DbEntity.ElementId,
                    TransactionType = transaction.DbEntity.TransactionType,
                });
            }

            // save new values
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                try {
                    conn.DbMapping.Save(correctionEntity);
                    conn.DbMapping.Save(typeof(DbEntityAuditCorrectionTransaction), correctionEntity.Transactions);
                        LogManager.Instance.CopyAuditCorrection(correctionEntity, Document, true);
                } catch (Exception ex) {
                    // TODO: add meaningful exception message
                    throw new Exception(ex.Message, ex);
                }
            }

            return new AuditCorrection(targetDocument, correctionEntity);
        }

        private void TransferAuditCorrectionsReconciliation(
            ProgressInfo progressInfo, 
            Document targetDocument, 
            IEnumerable<IReconciliationTransaction> transactions) {

            ((ReconciliationManager)targetDocument.ReconciliationManager).DeleteAllPreviousYearCorrectionValues(false);

            // Key: element id, value: sum of all transactions for that element
            var values = new Dictionary<int, decimal>();

            // compute sums
            foreach (var transaction in transactions.OfType<ReconciliationTransaction>()) {
                
                // There is a problem
                if (CheckTransactionElement(transaction, targetDocument) != null) {
                    // so we skip this transaction
                    continue;
                }

                if (!values.ContainsKey(transaction.DbEntity.ElementId))
                    values[transaction.DbEntity.ElementId] = 0;

                Debug.Assert(transaction.Value != null, "transaction.Value != null");
                values[transaction.DbEntity.ElementId] += transaction.Value.Value;
            }

            // create and save new transactions
            var reconciliation = (AuditCorrectionPreviousYear)targetDocument.ReconciliationManager.AuditCorrectionsPreviousYear;
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                try {
                    foreach (var pair in values) {
                        var elementId = pair.Key;
                        var value = pair.Value;

                        var transaction = new DbEntityReconciliationTransaction(
                            targetDocument, reconciliation.DbEntity, TransactionTypes.Unspecified, elementId) { Value = value };

                        conn.DbMapping.Save(transaction);

                        LogManager.Instance.CopyReconciliationTransaction(transaction, Document);
                    }
                } catch (Exception ex) {
                    
                    // TODO: add meaningful exception message
                    throw new Exception(ex.Message, ex);
                }
            }

        }

        #endregion // TransferAuditCorrectionValues

    }
}