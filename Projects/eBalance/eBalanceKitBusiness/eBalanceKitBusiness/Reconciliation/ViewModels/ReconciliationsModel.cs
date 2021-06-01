// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-03-08
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using Taxonomy;
using Utils;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Reconciliation.Interfaces;
using eBalanceKitBusiness.Reconciliation.Interfaces.ReconciliationTypes;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitResources.Localisation;

namespace eBalanceKitBusiness.Reconciliation.ViewModels {
    public class NavTreeNode : NotifyPropertyChangedBase {
        public NavTreeNode(string header, ObservableCollectionAsync<IReconciliation> reconciliations) {
            Header = header;
            _reconciliations = reconciliations;
            _reconciliations.CollectionChanged += ReconciliationsOnCollectionChanged;
            foreach (IReconciliation reconciliation in _reconciliations) {
                reconciliation.PropertyChanged += ReconciliationOnPropertyChanged;
            }
        }
        
        private void ReconciliationOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs) {
            if (propertyChangedEventArgs.PropertyName == "IsValid")
                OnPropertyChanged("IsValid");
            if (propertyChangedEventArgs.PropertyName == "HasTransaction")
                OnPropertyChanged("IsValid");
        }
        
        private void ReconciliationsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs) {
            OnPropertyChanged("IsValid");
            if (notifyCollectionChangedEventArgs.NewItems != null)
                foreach (var reconciliation in notifyCollectionChangedEventArgs.NewItems)
                    (reconciliation as IReconciliation).PropertyChanged += ReconciliationOnPropertyChanged;
            if (notifyCollectionChangedEventArgs.OldItems != null)
                foreach (var reconciliation in notifyCollectionChangedEventArgs.OldItems)
                    (reconciliation as IReconciliation).PropertyChanged -= ReconciliationOnPropertyChanged;
        }
        
        public string Header { get; private set; }
        public bool IsValid {
            get {
                if (_reconciliations.Any(r => !r.IsValid)) return false;
                else return true;
            }
        }
        
        private ObservableCollectionAsync<IReconciliation> _reconciliations { get; set; }

        private ICollectionView _reconciliationsView;

        public ICollectionView Reconciliations {
            get {
                if (_reconciliationsView == null) {
                    var source = new CollectionViewSource { Source = _reconciliations };
                    source.View.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
                    _reconciliationsView = source.View;
                }
                return _reconciliationsView;
            }
        }

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
        #endregion // IsSelected

        #region IsExpanded
        private bool _isExpanded;

        public bool IsExpanded {
            get { return _isExpanded; }
            set {
                if (_isExpanded != value) {
                    _isExpanded = value;
                    OnPropertyChanged("IsExpanded");
                }
            }
        }
        #endregion IsExpanded
    }

    public class ReconciliationsModel : NotifyPropertyChangedBase {

        public ReconciliationsModel(Document document) {
            Document = document;

            Dictionary<string, IElement> elements = Document.MainTaxonomy.Elements;
            TransferKind = elements["de-gaap-ci_hbst.transfer.kind"];

            #region init ReconciliationPropertyChanged event handler

            Document.ReconciliationManager.OnIsSelectedChanged += ReconciliationOnIsSelectedChanged;
            
            #endregion

            VisualOptions = new TreeViewVisualOptions();
            IReconciliationTreeNode node = null;

            PresentationTreeBalanceSheetTotalAssets =
                new ReconciliationTree(VisualOptions, Document.PresentationTreeBalanceSheetTotalAssets);

            node = GetSumNode(PresentationTreeBalanceSheetTotalAssets, e => e.IsBalanceSheetAssetsSumPosition);
            if (node != null) node.Value.ReconciliationInfo.PropertyChanged += TotalAssetsRootPropertyChanged;

            PresentationTreeBalanceSheetLiabilities =
                new ReconciliationTree(VisualOptions, Document.PresentationTreeBalanceSheetLiabilities);

            node = GetSumNode(PresentationTreeBalanceSheetLiabilities, e => e.IsBalanceSheetLiabilitiesSumPosition);
            if (node != null) node.Value.ReconciliationInfo.PropertyChanged += LiabilitiesRootPropertyChanged;

            PresentationTreeIncomeStatement =
                new ReconciliationTree(VisualOptions, Document.PresentationTreeIncomeStatement);

            node = GetSumNode(PresentationTreeIncomeStatement, e => e.IsIncomeStatementSumPosition);
            if (node != null) node.Value.ReconciliationInfo.PropertyChanged += IncomeStatementRootPropertyChanged;

            PresentationTreeBalanceSheetTotalAssets.ExpandSelectedCalled += (sender, args) => { SelectedTreeIndex = 0; };

            PresentationTreeBalanceSheetLiabilities.ExpandSelectedCalled += (sender, args) => { SelectedTreeIndex = 1; };

            PresentationTreeIncomeStatement.ExpandSelectedCalled += (sender, args) => { SelectedTreeIndex = 2; };

            ReconciliationList.CollectionChanged += OnReconciliationListChanged;
            InitReconciliationList();

            NavTreeRootReconciliations = new NavTreeNode(ResourcesReconciliation.Reconciliations, ReconciliationList) { IsExpanded = true };
            Document.ReconciliationManager.PreviousYearValues.Name = ResourcesReconciliation.PreviousYearValue;
            Document.ReconciliationManager.AuditCorrectionsPreviousYear.Name = ResourcesReconciliation.AuditCorrectionsPreviousYear;

            ObservableCollectionAsync<IReconciliation> previousYearValues = new ObservableCollectionAsync<IReconciliation>();
            NavTreeRootPreviousYearValues = new NavTreeNode(ResourcesReconciliation.ReconciliationPreviousYearValues, previousYearValues){IsExpanded = true};
            previousYearValues.Add(Document.ReconciliationManager.PreviousYearValues);
            //previousYearValues.Add(Document.ReconciliationManager.AuditCorrectionsPreviousYear);

            _navTreeRoots.Add(NavTreeRootReconciliations);
            if (Document.ReconciliationManager.PreviousYearValues.HasTransaction)
                _navTreeRoots.Add(NavTreeRootPreviousYearValues);
            Document.ReconciliationManager.PreviousYearValues.PropertyChanged += PreviousYearValuesPropertyChanged;
        }

        void PreviousYearValuesPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == "HasTransaction") {
                if (!Document.ReconciliationManager.PreviousYearValues.HasTransaction && _navTreeRoots.Contains(NavTreeRootPreviousYearValues)) {
                    _navTreeRoots.Remove(NavTreeRootPreviousYearValues);
                    NavTreeRootReconciliations.IsSelected = true;
                } else if (Document.ReconciliationManager.PreviousYearValues.HasTransaction && !_navTreeRoots.Contains(NavTreeRootPreviousYearValues)) {
                    _navTreeRoots.Add(NavTreeRootPreviousYearValues);
                    NavTreeRootPreviousYearValues.IsSelected = true;
                }
            }
        }
        
        public TreeViewVisualOptions VisualOptions { get; private set; }

        public Document Document { get; set; }

        public bool HasPreviousYearReports { get { return DocumentManager.Instance.HasPreviousYearReports; } }

        #region NavTreeRoots
        private readonly ObservableCollectionAsync<NavTreeNode> _navTreeRoots = new ObservableCollectionAsync<NavTreeNode>();
        public ObservableCollectionAsync<NavTreeNode> NavTreeRoots { get { return _navTreeRoots; } }
        #endregion // NavTreeRoots

        public NavTreeNode NavTreeRootReconciliations { get; private set; }
        public NavTreeNode NavTreeRootPreviousYearValues { get; private set; }
        
        void TotalAssetsRootPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == "STValueDisplayString") {
                OnPropertyChanged("TotalAssetsCaption");
            }
        }

        void LiabilitiesRootPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == "STValueDisplayString") {
                OnPropertyChanged("LiabilitiesCaption");
            }
        }

        void IncomeStatementRootPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == "STValueDisplayString") {
                OnPropertyChanged("IncomeStatementCaption");
            }
        }

        IReconciliationTreeNode GetSumNode(IReconciliationTree tree, Func<IElement, bool> isSumNodeSelector, IReconciliationTreeNode root = null) {
            if (root == null) {
                return tree.RootEntries.Select(
                    r => GetSumNode(tree, isSumNodeSelector, r)).FirstOrDefault(result => result != null);
            }

            return isSumNodeSelector(root.Element)
                       ? root
                       : root.Children.OfType<IReconciliationTreeNode>().Select(
                           node => GetSumNode(tree, isSumNodeSelector, node)).FirstOrDefault(
                               result => result != null);
        }

        #region SelectedTreeIndex
        private int _selectedTreeIndex;

        public int SelectedTreeIndex {
            get { return _selectedTreeIndex; }
            set {
                if (_selectedTreeIndex == value) return;
                _selectedTreeIndex = value;
                OnPropertyChanged("SelectedTreeIndex");
            }
        }
        #endregion // SelectedTreeIndex
       
        #region ReconciliationTypeInfos
        public static IDictionary<eBalanceKitBusiness.Reconciliation.Enums.ReconciliationTypes, IReconciliationTypeInfo> ReconciliationTypeInfos { get { return ReconciliationTypeInfo.ReconciliationTypeInfos; } }
        #endregion // ReconciliationTypeInfos

        public IElement TransferKind { get; private set; }

        #region DisplayedReconciliations
        public IEnumerable<IReconciliation> DisplayedReconciliations {
            get {

                switch (DisplayedReconciliationType) {
                    case eBalanceKitBusiness.Reconciliation.Enums.ReconciliationTypes.Reclassification:
                        return Reclassifications;

                    case eBalanceKitBusiness.Reconciliation.Enums.ReconciliationTypes.ValueChange:
                        return ValueChanges;

                    case eBalanceKitBusiness.Reconciliation.Enums.ReconciliationTypes.Delta:
                        return DeltaReconciliations;

                    case eBalanceKitBusiness.Reconciliation.Enums.ReconciliationTypes.PreviousYearValues:
                        return new[] { Document.ReconciliationManager.PreviousYearValues };

                    case eBalanceKitBusiness.Reconciliation.Enums.ReconciliationTypes.ImportedValues:
                        return ImportedValues;

                    case eBalanceKitBusiness.Reconciliation.Enums.ReconciliationTypes.AuditCorrection:
                        return AuditCorrections;

                    case eBalanceKitBusiness.Reconciliation.Enums.ReconciliationTypes.AuditCorrectionPreviousYear:
                        return new[] { Document.ReconciliationManager.AuditCorrectionsPreviousYear };

                    case eBalanceKitBusiness.Reconciliation.Enums.ReconciliationTypes.TaxBalanceValue:
                        return TaxBalanceValues;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        #endregion // DisplayedReconciliations

        #region ReconciliationList
        private ObservableCollectionAsync<IReconciliation> _reconciliationList = new ObservableCollectionAsync<IReconciliation>();
        public ObservableCollectionAsync<IReconciliation> ReconciliationList {
            get { return _reconciliationList; } 
            private set { _reconciliationList = value; }
        }

        private void OnReconciliationListChanged (object sender, NotifyCollectionChangedEventArgs args) {
            _hasTransactions = ReconciliationList.Any(r => r is IReclassification
                || r is IValueChange
                || r is IDeltaReconciliation
                || r is ITaxBalanceValue);
            OnPropertyChanged("HasTransactions");
            OnPropertyChanged("HasImportedValues");
        }

        #endregion

        #region [ HasTransactions ]

        private bool _hasTransactions = false;
        public bool HasTransactions {
            get { 
                return _hasTransactions;
            }
        }

        #endregion [ HasTransactions ]

        #region HasImportedValues
        public bool HasImportedValues {
            get { return ImportedValues.Any(); }
        }

        #endregion HasImportedValues

        #region DisplayedReconciliationType
        private eBalanceKitBusiness.Reconciliation.Enums.ReconciliationTypes _displayedReconciliationType = eBalanceKitBusiness.Reconciliation.Enums.ReconciliationTypes.Delta;

        public eBalanceKitBusiness.Reconciliation.Enums.ReconciliationTypes DisplayedReconciliationType {
            get { return _displayedReconciliationType; }
            set {
                if (_displayedReconciliationType == value) {
                    OnPropertyChanged("SelectedReconciliation");
                    return;
                }

                _displayedReconciliationType = value;
                OnPropertyChanged("DisplayedReconciliationType");
                OnPropertyChanged("DisplayedReconciliations");
                OnPropertyChanged("DeleteReconciliationAllowed");
                OnPropertyChanged("SelectedReconciliation");
            }
        }
        #endregion // DisplayedReconciliationType

        #region reconciliation lists

        #region Reclassifications
        public IEnumerable<IReclassification> Reclassifications {
            get {
                return Document.ReportRights.ReadTransferValuesAllowed
                           ? Document.ReconciliationManager.Reclassifications
                           : new List<IReclassification>();
            }
        }
        #endregion // Reclassifications

        #region ValueChanges
        public IEnumerable<IValueChange> ValueChanges {
            get {
                return Document.ReportRights.ReadTransferValuesAllowed
                           ? Document.ReconciliationManager.ValueChanges
                           : new List<IValueChange>();
            }
        }
        #endregion // ValueChanges

        #region DeltaReconciliations
        public IEnumerable<IDeltaReconciliation> DeltaReconciliations {
            get {
                return Document.ReportRights.ReadTransferValuesAllowed
                           ? Document.ReconciliationManager.DeltaReconciliations
                           : new List<IDeltaReconciliation>();
            }
        }
        #endregion // DeltaReconciliations

        #region ImportedValues
        public IEnumerable<IImportedValues> ImportedValues {
            get {
                return Document.ReportRights.ReadTransferValuesAllowed
                           ? Document.ReconciliationManager.ImportedValues
                           : new List<IImportedValues>();
            }
        }
        #endregion // ImportedValues

        #region AuditCorrections
        public IEnumerable<IDeltaReconciliation> AuditCorrections {
            get {
                return Document.ReportRights.ReadTransferValuesAllowed
                           ? Document.ReconciliationManager.AuditCorrections
                           : new List<IDeltaReconciliation>();
            }
        }
        #endregion // AuditCorrections

        #region TaxBalanceValues
        public IEnumerable<IDeltaReconciliation> TaxBalanceValues {
            get {
                return Document.ReportRights.ReadTransferValuesAllowed
                           ? Document.ReconciliationManager.TaxBalanceValues
                           : new List<IDeltaReconciliation>();
            }
        }
        #endregion // TaxBalanceValues

        #endregion // reconciliation lists

        //public bool AddReconciliationAllowed { get { return Document.ReportRights.WriteTransferValuesAllowed; } }

        public bool DeleteReconciliationAllowed {
            get {
                return Document.ReportRights.WriteTransferValuesAllowed &&
                       DisplayedReconciliations != null && DisplayedReconciliations.Any(reconciliation => reconciliation.IsSelected);
            }
        }

        /// <summary>
        /// Creates a new reconciliation of the specified type.
        /// </summary>
        /// <param name="type">Type of the new reconciliation.</param>
        public IReconciliation AddReconciliation(Enums.ReconciliationTypes type) {
            IReconciliation result = Document.ReconciliationManager.AddReconciliation(type);
            ReconciliationList.Add(result);
            return result;
        }

        /// <summary>
        /// Deletes the selected reconciliation.
        /// </summary>
        public void DeleteSelectedReconciliation() {
            Document.ReconciliationManager.DeleteReconciliation(SelectedReconciliation);
        }

        #region presentation trees
        public IReconciliationTree PresentationTreeBalanceSheetTotalAssets { get; private set; }
        public IReconciliationTree PresentationTreeBalanceSheetLiabilities { get; private set; }
        public IReconciliationTree PresentationTreeIncomeStatement { get; private set; }
        #endregion // presentation trees

        //#region Commands

        //public DelegateCommand AddDeltaReconciliationCommand { get; private set; }
        //public DelegateCommand AddReclassificationCommand { get; private set; }
        //public DelegateCommand AddValueChangeCommand { get; private set; }
        //public DelegateCommand AddAuditCorrectionCorrectionCommand { get; private set; }

        //#endregion Commands

        #region treeview sum detail display strings
        public string TotalAssetsCaption {
            get {
                return ResourcesReconciliation.BalanceListAssets + " " +
                       GetStValueSumDisplaString(PresentationTreeBalanceSheetTotalAssets, e => e.IsBalanceSheetAssetsSumPosition);
            }
        }

        public string LiabilitiesCaption {
            get {
                return ResourcesReconciliation.BalanceListLiabilities + " " +
                       GetStValueSumDisplaString(PresentationTreeBalanceSheetLiabilities, e => e.IsBalanceSheetLiabilitiesSumPosition);
            }
        }

        public string IncomeStatementCaption {
            get {
                return ResourcesReconciliation.IncomeStatement + " " +
                       GetStValueSumDisplaString(PresentationTreeIncomeStatement, e => e.IsIncomeStatementSumPosition);
            }
        }
        #endregion // treeview sum detail display strings

        private string GetStValueSumDisplaString(IReconciliationTree tree, Func<IElement, bool> isSumNodeSelector, IReconciliationTreeNode root = null) {
            if (root == null) {
                return tree.RootEntries.Select(
                    r => GetStValueSumDisplaString(tree, isSumNodeSelector, r)).FirstOrDefault(result => result != null);
            }

            return isSumNodeSelector(root.Element)
                       ? (root != null && root.Value != null && root.Value.ReconciliationInfo != null)
                             ? "(" + ResourcesReconciliation.TabHeaderSumCaption +
                               root.Value.ReconciliationInfo.STValueDisplayString + ")"
                             : null
                       : root.Children.OfType<IReconciliationTreeNode>().Select(
                           node => GetStValueSumDisplaString(tree, isSumNodeSelector, node)).FirstOrDefault(
                               result => result != null);
        }

        #region SelectedReconciliation
        public IReconciliation SelectedReconciliation {
            get {
                return Document.ReconciliationManager.SelectedReconciliation;
            }
        }
        #endregion // SelectedReconciliation

        public void DeleteAllPreviousYearValues() { Document.ReconciliationManager.DeleteAllPreviousYearValues(); }
        public void DeleteAllPreviousYearCorrectionValues() { Document.ReconciliationManager.DeleteAllPreviousYearCorrectionValues(); }

        /// <summary>
        /// Deletes all imported reconciliations.
        /// </summary>
        public void DeleteImportedReconciliations() {
            IImportedValues[] importedValues = ImportedValues.ToArray();
            foreach (IImportedValues importedValue in importedValues) {
                Document.ReconciliationManager.DeleteReconciliation(importedValue);
                ReconciliationList.Remove(importedValue);
            }
            OnPropertyChanged("DisplayedReconciliations");
            OnPropertyChanged("SelectedReconciliation");
            OnPropertyChanged("ImportedValues");
        }
        
        public void ReconciliationOnIsSelectedChanged(IReconciliation reconciliation, SelectedChangedEventArgs args) {
            // reconciliation should be null if nothing selected
            if (reconciliation != null)
                DisplayedReconciliationType = reconciliation.ReconciliationType;
            OnPropertyChanged("DeleteReconciliationAllowed");
            OnPropertyChanged("SelectedReconciliation");
        }

        public void DeletedReconciliations(IEnumerable<IReconciliation> deletedReconciliations) {
            foreach (IReconciliation reconciliation in deletedReconciliations) {
                ReconciliationList.Remove(reconciliation);
            }
        }

        private void InitReconciliationList() {
            List<IReconciliation> result = new List<IReconciliation>();
            //No PreviousYearValues and no AuditCorrectionsPreviousYear
            result.AddRange(Reclassifications);
            result.AddRange(ValueChanges);
            result.AddRange(DeltaReconciliations);
            result.AddRange(ImportedValues);
            result.AddRange(AuditCorrections);
            result.AddRange(TaxBalanceValues);

            ReconciliationList.Clear();
            foreach (IReconciliation item in result.OrderBy(r => r.Name)) {
                ReconciliationList.Add(item);
            }
        }

        public void AddedReconciliation(IReconciliation newReconciliation) {
            ReconciliationList.Add(newReconciliation);
        }

        internal void DeleteAllCurrentYearReconciliations() {
            List<IReconciliation> deletedReconciliations = new List<IReconciliation>();
            deletedReconciliations.AddRange(Reclassifications);
            deletedReconciliations.AddRange(ValueChanges);
            deletedReconciliations.AddRange(DeltaReconciliations);
            deletedReconciliations.AddRange(ImportedValues);
            deletedReconciliations.AddRange(AuditCorrections);
            DeletedReconciliations(deletedReconciliations);
            foreach (IReconciliation reconciliation in deletedReconciliations) {
                Document.ReconciliationManager.DeleteReconciliation(reconciliation);   
            }
        }

        internal void DeleteAllTaxBalanceSheetValueReconciliations() {
            List<IReconciliation> deletedReconciliations = new List<IReconciliation>();
            deletedReconciliations.AddRange(TaxBalanceValues);
            DeletedReconciliations(TaxBalanceValues);
            foreach (IReconciliation reconciliation in deletedReconciliations) {
                Document.ReconciliationManager.DeleteReconciliation(reconciliation);
            }
        }
    }
}