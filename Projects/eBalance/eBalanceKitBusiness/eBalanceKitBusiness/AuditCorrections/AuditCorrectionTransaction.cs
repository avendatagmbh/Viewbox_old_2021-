// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-05-15
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Taxonomy;
using Taxonomy.Interfaces;
using Taxonomy.Interfaces.PresentationTree;
using Utils;
using eBalanceKitBase.Structures;
using eBalanceKitBusiness.AuditCorrections.DbMapping;
using eBalanceKitBusiness.Exceptions;
using eBalanceKitBusiness.Localisation;
using eBalanceKitBusiness.Logs;
using eBalanceKitBusiness.Structures;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Structures.Presentation;
using eBalanceKitResources.Localisation;

namespace eBalanceKitBusiness.AuditCorrections {
    internal class AuditCorrectionTransaction : NotifyPropertyChangedBase, IAuditCorrectionTransaction, IPresentationTreeEntry {

        private bool _doLog = true;

        internal AuditCorrectionTransaction(IAuditCorrection parent, IElement element) {
            Parent = parent;
            Element = element;
            DbEntity = new DbEntityAuditCorrectionTransaction {
                Document = parent.Document, 
                ElementId = Document.TaxonomyIdManager.GetId(element),
                DbEntityAuditCorrection = ((AuditCorrection)parent).DbEntity
            };
            Save();

            if (_doLog) LogManager.Instance.NewAuditCorrectionTransaction(this);
        }

        internal AuditCorrectionTransaction(IAuditCorrection parent, DbEntityAuditCorrectionTransaction dbEntity) {            
            Parent = parent;
            DbEntity = dbEntity;
            Element = dbEntity.Document.TaxonomyIdManager.GetElement(dbEntity.ElementId);
        }

        internal DbEntityAuditCorrectionTransaction DbEntity { get; set; }

        #region Value
        public virtual decimal? Value {
            get {
                if (!Document.ReportRights.ReadRestAllowed)
                    throw new AccessDeniedException(ExceptionMessages.InsufficentReadRights);
                return DbEntity.Value;
            }
            set {
                if (!Document.ReportRights.WriteRestAllowed)
                    throw new AccessDeniedException(ExceptionMessages.InsufficentWriteRights);

                string oldValue = DbEntity.Value.ToString(CultureInfo.InvariantCulture);
                DbEntity.Value = value ?? 0;
                if (_doLog) LogManager.Instance.UpdateAuditCorrectionTransactionValue(this, oldValue);
                OnPropertyChanged("Value");
                OnPropertyChanged("ValueDisplayString");
                Save();

                foreach (var parent in Parents) {
                    var node = (PresentationTreeNode)parent;
                    var val = node.Value;
                    if (val != null) val.UpdateComputedValue(node);
                }
            }
        }

        public string ValueDisplayString { get { return LocalisationUtils.CurrencyToString(Value); } }

        #endregion // Value
        
        #region Parent
        private IAuditCorrection _parent;

        public IAuditCorrection Parent {
            get { return _parent; }
            private set {
                Debug.Assert(value != null);

                _parent = value;
                
                _parent.PropertyChanged += ParentOnPropertyChanged;
                
                OnPropertyChanged("Parent");
                OnPropertyChanged("Label");
            }
        }

        private void ParentOnPropertyChanged(object sender, PropertyChangedEventArgs e) { if (e.PropertyName == "Name") OnPropertyChanged("Label"); }
        #endregion // Parent

        #region Element
        private IElement _element;

        /// <summary>
        /// Assigned taxonomy element.
        /// </summary>
        public IElement Element {
            get { return _element; }
            private set {
                if (_element == value) return;
                _element = value;
                OnPropertyChanged("Element");
            }
        }
        #endregion // Element

        public Document Document { get { return Parent.Document; } }

        public string Label { get { return Parent.Name; } }
        
        #region IsSelected
        private bool _isSelected;

        public virtual bool IsSelected {
            get { return _isSelected; }
            set {
                if (_isSelected == value) return;

                if (value) ShowInTreeview();

                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }
        #endregion IsSelected

        #region IsVisible
        private bool _isVisible = true;

        public bool IsVisible {
            get { return _isVisible; }
            set {
                if (_isVisible == value) return;
                _isVisible = value;
                OnPropertyChanged("IsVisible");
            }
        }
        #endregion // IsVisible

        /// <summary>
        /// Saves this entity to database (called on value change).
        /// </summary>
        private void Save() {
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                try {
                    conn.DbMapping.Save(DbEntity);
                } catch (Exception ex) {
                    throw new Exception(ResourcesReconciliation.SaveTransactionException + ex.Message, ex);
                }
            }
        }

        /// <summary>
        /// Deletes the database representation of this entity (called from assigned audit correction in remove method).
        /// </summary>
        internal virtual void Delete() {
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                try {
                    conn.DbMapping.Delete(DbEntity);
                } catch (Exception ex) {
                    throw new Exception(ResourcesReconciliation.DeleteTransactionException + ex.Message, ex);
                }
            }
            if (_doLog) LogManager.Instance.DeleteAuditCorrectionTransaction(this);

            foreach (var parent in Parents.ToList()) // should always contain exact one item
                parent.RemoveChildren(this);
            _parents.Clear();
        }

        internal void DisableLogging() { _doLog = false; }
        internal void EnableLogging() { _doLog = true; }

        /// <summary>
        /// Removes this transaction from the parent AuditCorrection object.
        /// </summary>
        public void Remove() {
            Debug.Assert(Parent != null);
            Parent.RemoveTransaction(this);
        }

        #region Parents
        private readonly ObservableCollectionAsync<IPresentationTreeNode> _parents =
            new ObservableCollectionAsync<IPresentationTreeNode>();

        public virtual IEnumerable<IPresentationTreeNode> Parents { get { return _parents; } }
        #endregion // Parents

        public virtual bool IsRoot { get { return _parents.Count == 0; } }
        public double Order { get { return -1; } }
        public bool IsHypercubeContainer { get { return false; } }

        public virtual void AddParent(IPresentationTreeNode parent) { _parents.Add(parent); }
        public virtual void RemoveParent(IPresentationTreeNode parent) { _parents.Remove(parent); }

        #region RemoveFromParents
        public virtual void RemoveFromParents() {
            var tmpParents = new List<IPresentationTreeNode>(Parents);
            foreach (IPresentationTreeNode node in tmpParents) node.RemoveChildren(this);
            IsSelected = false;
        }
        #endregion // RemoveFromParents

        public event ScrollIntoViewRequestedEventHandler ScrollIntoViewRequested;
        public event ScrollIntoViewRequestedEventHandler SearchLeaveFocusRequested;

        public void ScrollIntoView(IList<ISearchableNode> path) {
            if (ScrollIntoViewRequested != null)
                ScrollIntoViewRequested(path);
        }

        public void SearchLeaveFocus(IList<ISearchableNode> path) {
            if (SearchLeaveFocusRequested != null)
                SearchLeaveFocusRequested(path);
        }

        // dummy properties to avoid binding exceptions
        public bool IsEditable { get { return false; } }
        public bool IsExpanded { get; set; }

        public void ShowInTreeview() {
            var parent = Parents.FirstOrDefault() as Interfaces.PresentationTree.IPresentationTreeNode;
            Debug.Assert(parent != null);
            ((PresentationTree)parent.PresentationTree).ExpandSelected(parent);
        }

    }
}