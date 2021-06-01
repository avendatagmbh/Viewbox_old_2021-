// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-05-15
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using Utils;
using eBalanceKitBusiness.AuditCorrections.DbMapping;
using eBalanceKitBusiness.Exceptions;
using eBalanceKitBusiness.Interfaces.PresentationTree;
using eBalanceKitBusiness.Localisation;
using eBalanceKitBusiness.Logs;
using eBalanceKitBusiness.Reconciliation.Interfaces;
using eBalanceKitBusiness.Structures;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Structures.Presentation;
using eBalanceKitBusiness.Structures.XbrlElementValue;
using eBalanceKitResources.Localisation;

namespace eBalanceKitBusiness.AuditCorrections {
    internal class AuditCorrection : NotifyPropertyChangedBase, IAuditCorrection {

        private bool _doLog = true;

        internal AuditCorrection(Document document) {
            Document = document;
            DbEntity = new DbEntityAuditCorrection { Document = document };

            string baseName = ResourcesAuditCorrections.DefaultName + " ";

            HashSet<string> existingNames = new HashSet<string>();
            foreach (var positionCorrection in document.AuditCorrectionManager.PositionCorrections)
                existingNames.Add(positionCorrection.Name);

            int i = 1;
            string name = baseName + i;
            while (existingNames.Contains(name)) 
                name = baseName + i++;

            DbEntity.Name = name;
        }

        internal AuditCorrection(Document document, DbEntityAuditCorrection dbEntity) {
            Document = document;
            DbEntity = dbEntity;

            foreach (var transaction in dbEntity.Transactions) {

                var node = document.GaapPresentationTrees.Values.Select(
                    ptree => ptree.GetNode(transaction.ElementId)).FirstOrDefault(n => n != null);

                if (node != null)
                    AddTransaction(node, transaction);
            }
        }

        internal DbEntityAuditCorrection DbEntity { get; private set; }

        public Document Document { get; private set; }

        #region Name
        public string Name {
            get { return DbEntity.Name; }
            set {
                if (!Document.ReportRights.WriteTransferValuesAllowed)
                    throw new AccessDeniedException(ExceptionMessages.InsufficentWriteRights);

                if (DbEntity.Name == value) return;
                string oldValue = Name;
                DbEntity.Name = string.IsNullOrEmpty(value) ? null : StringUtils.Left(value, 512);
                if (_doLog) LogManager.Instance.UpdateAuditCorrectionName(this, oldValue);

                Save();

                OnPropertyChanged("Name");
            }
        }
        #endregion // Name
        
        #region Comment
        public string Comment {
            get { return DbEntity.Comment; }
            set {
                if (!Document.ReportRights.WriteTransferValuesAllowed)
                    throw new AccessDeniedException(ExceptionMessages.InsufficentWriteRights);

                if (DbEntity.Comment == value) return;
                string oldValue = Comment;
                DbEntity.Comment = string.IsNullOrEmpty(value) ? null : StringUtils.Left(value, 8192);
                if (_doLog) LogManager.Instance.UpdateAuditCorrectionComment(this, oldValue);

                Save();

                OnPropertyChanged("Comment");
            }
        }
        #endregion // Comment      

        #region IsSelected
        private bool _isSelected;

        public bool IsSelected {
            get { return _isSelected; }
            set {
                if (_isSelected == value) return;
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }
        #endregion IsSelected

        #region Transactions
        private readonly ObservableCollectionAsync<IAuditCorrectionTransaction> _transactions =
            new ObservableCollectionAsync<IAuditCorrectionTransaction>();

        public IEnumerable<IAuditCorrectionTransaction> Transactions { get { return _transactions; } }
        #endregion // Transactions

        #region AddTransaction
        public IAuditCorrectionTransaction AddTransaction(IPresentationTreeNode node) { return AddTransaction(node, null); }

        /// <summary>
        /// Adds a new transaction which will be assigned to the specified presentation tree node.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="transactionEntity"> </param>
        /// <returns></returns>
        public IAuditCorrectionTransaction AddTransaction(IPresentationTreeNode node, DbEntityAuditCorrectionTransaction transactionEntity) {
            var transaction = transactionEntity == null
                                  ? new AuditCorrectionTransaction(this, node.Element)
                                  : new AuditCorrectionTransaction(this, transactionEntity);

            _transactions.Add(transaction);

            node.AddChildren(transaction);
            transaction.AddParent(node);

            return transaction;
        }
        #endregion AddTransaction

        #region RemoveTransaction
        /// <summary>
        /// Removes the specified transaction and (in case of an assigned tree node) the assignment between the transaction and the assigned node.
        /// </summary>
        /// <param name="transaction"></param>
        public void RemoveTransaction(IAuditCorrectionTransaction transaction) {
            if (!Document.ReportRights.WriteTransferValuesAllowed)
                throw new AccessDeniedException(ExceptionMessages.InsufficentWriteRights);

            _transactions.Remove(transaction);

            // delete persistant representation
            ((AuditCorrectionTransaction)transaction).Delete();

            OnPropertyChanged("HasTransaction");
        }
        #endregion // RemoveTransaction

        #region Delete
        internal void Delete() {
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                try {
                    conn.DbMapping.Delete(DbEntity);
                } catch (Exception ex) {
                    throw new Exception(ResourcesReconciliation.DeleteException + ex.Message, ex);
                }
            }

            if (_doLog) LogManager.Instance.DeleteAuditCorrection(this);
        }
        #endregion // Delete

        #region Save
        internal void Save() {
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                try {
                    conn.DbMapping.Save(DbEntity);
                } catch (Exception ex) {
                    throw new Exception(ResourcesReconciliation.SaveException + ex.Message, ex);
                }
            }
        }
        #endregion // Save

        #region DisableLogging
        internal void DisableLogging() {
            _doLog = false;
            foreach (var transaction in Transactions)
                ((AuditCorrectionTransaction) transaction).DisableLogging();
        }
        #endregion // DisableLogging

        #region EnableLogging
        internal void EnableLogging() {
            _doLog = true;
            foreach (var transaction in Transactions)
                ((AuditCorrectionTransaction) transaction).EnableLogging();
        }
        #endregion // EnableLogging

    }
}