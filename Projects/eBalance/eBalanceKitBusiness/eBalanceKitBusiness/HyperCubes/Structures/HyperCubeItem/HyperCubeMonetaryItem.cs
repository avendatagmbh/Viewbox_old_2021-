// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-25
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Taxonomy;
using eBalanceKitBase.Structures;
using eBalanceKitBusiness.Exceptions;
using eBalanceKitBusiness.HyperCubes.DbMapping;
using eBalanceKitBusiness.HyperCubes.Interfaces.Structure;

namespace eBalanceKitBusiness.HyperCubes.Structures.HyperCubeItem {   
    
    internal class HyperCubeMonetaryItem : HyperCubeItemBase {

        #region Class SummationItem
        private class SummationItem {

            public SummationItem(HyperCubeMonetaryItem value, decimal weight) {
                Value = value;
                Weight = weight;
            }

            public HyperCubeMonetaryItem Value { get; set; }
            public decimal Weight { get; set; }

            public decimal? WeightedValue {
                get { return Value._monetaryValue.HasValue ? Weight*Value._monetaryValue : null; }
            }

            public bool IsUserDefined { get; set; }
        }
        #endregion

        private decimal? _monetaryValue;

        private readonly List<HyperCubeMonetaryItem> _parents = new List<HyperCubeMonetaryItem>();
        private HyperCubeMonetaryItemGroup _relatives;

        private List<HyperCubeMonetaryItem>[] _siblingsByDimIndex;
        private HyperCubeMonetaryItem[] _parentsByDimIndex;
        private List<SummationItem>[] _childrenByDimIndex;
        private List<HyperCubeMonetaryItem>[] _ancestorsByDimIndex;

        internal HyperCubeMonetaryItem(HyperCubeItemCollection items, DbEntityHyperCubeItem dbEntity, IElement primaryDimensionValue)
            : base(items, dbEntity, primaryDimensionValue) { }

        #region properties
        #region private/internal

        #region IsComputedFromMultipleSources
        /// <summary>
        /// Help property, used in UpdateIsLocked method.
        /// </summary>
        private bool IsComputedFromMultipleSources {
            get {
                var count = 0;
                foreach (var childList in _childrenByDimIndex) {
                    if (childList.Any(child => child.WeightedValue.HasValue)) count++;
                    if (count == 2) return true;
                }

                return false;
            }
        }
        #endregion IsComputedFromMultipleSources

        #region AssignedDimensionValues
        internal IHyperCubeDimensionValue[] AssignedDimensionValues { get; set; }
        #endregion AssignedDimensionValues
        
        #endregion private/internal

        #region public

        #region Value
        /// <summary>
        /// Returns the monetary value, which is ether the computed or the manual value.
        /// </summary>
        public override sealed object Value {
            get {
                if (!Items.Cube.Document.ReportRights.ReadAllowed)
                    throw new AccessDeniedException(Localisation.ExceptionMessages.InsufficentReadRights);
                
                return _monetaryValue.HasValue ? LocalisationUtils.DecimalToString(_monetaryValue.Value) : string.Empty;
            }
            set {
                if (!Items.Cube.Document.ReportRights.WriteAllowed)
                    throw new AccessDeniedException(Localisation.ExceptionMessages.InsufficentWriteRights); 
                
                if (value == null || value.ToString().Length == 0) ManualValue = null;
                else
                {
                    //ManualValue = Math.Round(decValue, 2, MidpointRounding.AwayFromZero);
                    
                    decimal providerValue;
                    NumberFormatInfo numProvider = new NumberFormatInfo();
                    numProvider.NumberDecimalSeparator = ",";
                    numProvider.NumberGroupSeparator = ".";
                    numProvider.NumberGroupSizes = new int[] { 3 };

                    try
                    {
                        providerValue = Convert.ToDecimal(value, numProvider);
                        ManualValue = Math.Round(providerValue, 2, MidpointRounding.AwayFromZero);
                    }
                    catch(FormatException)
                    {
                        //do nothing, no value set
                    }
                    catch (OverflowException)
                    {
                        //do nothing, no value set
                    }
                }
            }
        }
        #endregion Value

        #region ComputedValue
        private decimal? _computedValue;

        public decimal? ComputedValue {
            get { return _computedValue; }
            private set {
                _computedValue = value;
                //OnPropertyChanged("IsComputed");
            }
        }
        #endregion

        #region ManualValue
        private decimal? _manualValue;
        public decimal? ManualValue {
            get {
                if (!Items.Cube.Document.ReportRights.ReadAllowed)
                    throw new AccessDeniedException(Localisation.ExceptionMessages.InsufficentReadRights); 
                
                return _manualValue;
            }
            set {
                if (_monetaryValue == value) return;

                if (!Items.Cube.Document.ReportRights.WriteAllowed)
                    throw new AccessDeniedException(Localisation.ExceptionMessages.InsufficentWriteRights); 
                
                bool hasValueChanged = _manualValue.HasValue != value.HasValue;
                _manualValue = value;

                HashSet<IHyperCubeItem> changedItems = new HashSet<IHyperCubeItem>();
                UpdateMonetaryValue(changedItems);

                foreach (var changedItem in changedItems.OfType<HyperCubeMonetaryItem>()) {
                    changedItem.OnPropertyChanged("");
                }

                base.Value = value;

                if (hasValueChanged) UpdateIsLocked();
            }
        }
        #endregion ManualValue

        #region HasValue
        public override bool HasValue { get { return _monetaryValue.HasValue; } }
        #endregion

        #region IsComputed
        public override bool IsComputed { get { return ComputedValue.HasValue; } }
        #endregion

        #region IsLocked
        private bool _isLocked;

        /// <summary>
        /// True, if any assigned dimension is locked (set in method UpdateIsLocked) or if
        /// any anchestor in any dimension is computed from multiple sources.
        /// </summary>
        public override bool IsLocked {
            get { return _isLocked; }
            protected set {
                if (_isLocked != value) {
                    _isLocked = value;
                    OnPropertyChanged("IsLocked");
                }
            }
        }
        #endregion

        #endregion public
        #endregion properties

        #region methods

        #region UpdateMonetaryValue

        private void UpdateMonetaryValue(HashSet<IHyperCubeItem> changedItems) {
            Debug.Assert(!(ComputedValue.HasValue && ManualValue.HasValue), "ComputedValue or ManualValue must be null!");

            var oldValue = _monetaryValue;

            decimal val = 0;

            if (ComputedValue.HasValue || ManualValue.HasValue) {
                if (ComputedValue.HasValue) val += ComputedValue.Value;
                if (ManualValue.HasValue) val += ManualValue.Value;
                _monetaryValue = val;
            } else {
                _monetaryValue = null;
            }

            if (oldValue != _monetaryValue) 
                changedItems.Add(this);

            // recompute parent values
            foreach (var item in _parents)
                item.UpdateComputedValue(changedItems);
        }
        #endregion UpdateMonetaryValue

        #region UpdateIsLocked
        private void UpdateIsLocked() {

            // compute isLockedValues array
            bool[] isLockedParentValues = new bool[AssignedDimensionValues.Length];
            bool[] isLockedChildValues = new bool[AssignedDimensionValues.Length];
            if (ManualValue.HasValue) {
                for (int i = 0; i < AssignedDimensionValues.Length; i++) {
                    isLockedChildValues[i] = true;
                    isLockedParentValues[i] = true;
                }
            } else {
                for (int i = 0; i < AssignedDimensionValues.Length; i++) {
                    foreach (var item in _relatives)
                    {
                        if (item.ManualValue.HasValue && item.AssignedDimensionValues[i] == AssignedDimensionValues[i]) {
                            isLockedChildValues[i] = true;
                        }
                    }

                    var parent = _parentsByDimIndex[i];
                    if (parent == null) continue;
                    foreach (var item in _relatives) {
                        if (item.ManualValue.HasValue) {
                            isLockedParentValues[i] |= item._parentsByDimIndex[i] != null &&
                                                       item._parentsByDimIndex[i].AssignedDimensionValues[i] ==
                                                       parent.AssignedDimensionValues[i];
                        }
                    }
                }
            }

            // update isLocked for parent/child dimensions
            for (int i = 0; i < AssignedDimensionValues.Length; i++) {
                var dimValue = AssignedDimensionValues[i];
                UpdateIsLockedForChildren(dimValue, isLockedChildValues[i]);
                UpdateIsLockedForParents(dimValue, isLockedParentValues[i]);
            }
            
            // update isLocked for all computational dependent values
            foreach (var item in _relatives) {
                item.IsLocked = !item.IsComputed &&
                                item.AssignedDimensionValues.OfType<HyperCubeDimensionValue>().Any(
                                    dimValue => dimValue.IsLocked[_relatives]);
            }
        }

        private void UpdateIsLockedForParents(IHyperCubeDimensionValue dimValue, bool isLockedValue) {
            if (dimValue.Parent == null) return;
            ((HyperCubeDimensionValue) dimValue.Parent).IsLocked[_relatives] = isLockedValue;
            UpdateIsLockedForParents(dimValue.Parent, isLockedValue);
        }

        private void UpdateIsLockedForChildren(IHyperCubeDimensionValue dimValue, bool isLockedValue) {
            foreach (var dimChild in dimValue.Children.OfType<HyperCubeDimensionValue>()) {
                dimChild.IsLocked[_relatives] = isLockedValue;
                UpdateIsLockedForChildren(dimChild, isLockedValue);
            }
        }
        #endregion UpdateIsLocked

        #region UpdateComputedValue
        private void UpdateComputedValue(HashSet<IHyperCubeItem> changedItems) {
            var oldValue = ComputedValue;
            bool foundValue = false;
            foreach (var childList in _childrenByDimIndex) {
                decimal? computedValue = null;
                foreach (var child in childList.Where(child => child.WeightedValue.HasValue)) {
                    Debug.Assert(child.WeightedValue != null, "child.WeightedValue != null");
                    if (!computedValue.HasValue) computedValue = child.WeightedValue.Value;
                    else computedValue += child.WeightedValue.Value;
                }

                if (computedValue.HasValue) {
                    ComputedValue = computedValue;
                    foundValue = true;
                }
            }

            if (!foundValue)
                ComputedValue = null;

            if (oldValue != ComputedValue)
                changedItems.Add(this);
            
            UpdateMonetaryValue(changedItems);
        }
        #endregion UpdateComputedValue

        #region SetDirectDependencies
        /// <summary>
        /// Computed the direct dependencies (parents and children).
        /// </summary>
        internal void SetDirectDependencies() {
                        
            _parentsByDimIndex = new HyperCubeMonetaryItem[AssignedDimensionValues.Length];
            _childrenByDimIndex = new List<SummationItem>[AssignedDimensionValues.Length];

            for (int i = 0; i < AssignedDimensionValues.Length; i++) {
                var value = AssignedDimensionValues[i];
                if (!value.IsRoot) {
                    var key = AssignedDimensionValues.ToArray();
                    key[i] = value.Parent;
                    // TODO: get respective value for value.Element.SummationSources (should be unique?)
                    IHyperCubeItem item = Items.GetItem(key);

                    if (item is HyperCubeMonetaryItem) {
                        _parentsByDimIndex[i] = item as HyperCubeMonetaryItem;
                        _parents.Add(_parentsByDimIndex[i]);
                    }
                }

                _childrenByDimIndex[i] = new List<SummationItem>();

                // assertion:
                // presentation tree structure equals computation tree structure
                // see "Technischer Leitfaden zur Verwendung der HGB-Taxonomie 5.0" V0.92 / 2011-12-19 / page 14:
                // "Der Aufbau der Positionen in der Definition Linkbase stimmt mit der Calutaltion Linkbase 
                // überein, lediglich die Nicht-monetären Positionen entfallen in der Calculation Linkbase."
                //
                // no calculations defined for "Eigenkapitalspiegel"?

                foreach (var child in value.Children) {
                    var key = AssignedDimensionValues.ToArray();
                    key[i] = child;
                    IHyperCubeItem item = Items.GetItem(key);

                    if (item is HyperCubeMonetaryItem) {
                        Taxonomy.SummationItem summationItem = value.Element.SummationTargets.FirstOrDefault(
                            summationTarget => summationTarget.Element == child.Element);

                        _childrenByDimIndex[i].Add(
                            summationItem != null
                                ? new SummationItem(item as HyperCubeMonetaryItem, (decimal)summationItem.Weight)
                                : new SummationItem(item as HyperCubeMonetaryItem, 1));
                    }
                }
                // ****************************************
            }
        }
        #endregion SetDirectDependencies

        #region SetIndirectDependencies
        /// <summary>
        /// Computed the indirect dependencies (siblings, ancestors).
        /// </summary>
        internal void SetIndirectDependencies() {

            // compute siblings
            _siblingsByDimIndex = new List<HyperCubeMonetaryItem>[AssignedDimensionValues.Length];
            for (int i = 0; i < AssignedDimensionValues.Length; i++) {
                _siblingsByDimIndex[i] = new List<HyperCubeMonetaryItem>();
                var parent = _parentsByDimIndex[i];
                if (parent == null) continue;
                foreach (var child in parent._childrenByDimIndex[i].Where(child => child.Value != this)) {
                    _siblingsByDimIndex[i].Add(child.Value);
                }
            }

            // compute _ancestorsByDimIndex
            _ancestorsByDimIndex = new List<HyperCubeMonetaryItem>[AssignedDimensionValues.Length];
            for (int i = 0; i < AssignedDimensionValues.Length; i++) {
                _ancestorsByDimIndex[i] = new List<HyperCubeMonetaryItem>();
                GetAncestors(i, this);
            }
        }

        /// <summary>
        /// Help method for SetIndirectDependencies
        /// </summary>
        private void GetAncestors(int dimIndex, HyperCubeMonetaryItem root) {
            var parent = root._parentsByDimIndex[dimIndex];
            if (parent == null) return;
            _ancestorsByDimIndex[dimIndex].Add(parent);
            GetAncestors(dimIndex, parent);
        }
        #endregion SetIndirectDependencies

        #region SetRelatives
        /// <summary>
        /// Computes the relatives collection (all items which are computational depending on each other).
        /// </summary>
        internal HyperCubeMonetaryItemGroup SetRelatives() {
            _relatives = new HyperCubeMonetaryItemGroup();

            foreach (
                var dimValue in
                    Items.Cube.Dimensions.AllDimensionItems.SelectMany(
                        dim => dim.Values.OfType<HyperCubeDimensionValue>())) {
                dimValue.IsLocked[_relatives] = false;
            }

            AddChildRelatives(GetTopMostParent(this));
            foreach (var item in _relatives) item._relatives = _relatives;
            return _relatives;
        }

        /// <summary>
        /// Help method for SetRelatives.
        /// </summary>
        private static HyperCubeMonetaryItem GetTopMostParent(HyperCubeMonetaryItem root, int dimIndex = 0) {
            if (dimIndex == root._parentsByDimIndex.Length) return root;
            return root._parentsByDimIndex[dimIndex] == null
                       ? (GetTopMostParent(root, dimIndex + 1))
                       : (GetTopMostParent(root._parentsByDimIndex[dimIndex], dimIndex));
        }

        /// <summary>
        /// Help method for SetRelatives.
        /// </summary>
        private void AddChildRelatives(HyperCubeMonetaryItem root) {
            if (_relatives.Contains(root)) return;
            _relatives.Add(root);

            foreach (var childList in root._childrenByDimIndex) {
                foreach (var child in childList) {
                    AddChildRelatives(child.Value);
                }
            }
        }
        #endregion

        #region InitManualValue
        /// <summary>
        /// Inits the ManualValue property. This method should not not be called before the method
        /// SetDirectDependencies, SetIndirectDependencies and SetRelatives has been called, otherwhise
        /// the IsComputed and the IsLocked properties would not be correct initialized.
        /// </summary>
        internal void InitManualValue() {
            if (base.Value == null || base.Value.ToString().Length == 0) _manualValue = null;
            else {
                _manualValue = Math.Round(Convert.ToDecimal(base.Value), 2, MidpointRounding.AwayFromZero);
                HashSet<IHyperCubeItem> changedItems = new HashSet<IHyperCubeItem>();
                UpdateMonetaryValue(changedItems);
                UpdateIsLocked();
            }
        }
        #endregion InitManualValue

        internal void SetAssignedDimensionValues(IHyperCubeDimensionValue[] dimItems, IHyperCubeDimensionValue primaryDimensionValue) {
            AssignedDimensionValues = new IHyperCubeDimensionValue[dimItems.Length + 1];
            Array.Copy(dimItems, AssignedDimensionValues, dimItems.Length);
            AssignedDimensionValues[dimItems.Length] = primaryDimensionValue;
        }

        #endregion methods

        /*
                // ComputedValue sum is computed using the taxonomy calculation rules
                {
                    decimal value = 0;
                    decimal transferValue = 0;
                    decimal transferValuePreviousYear = 0;
                    bool foundValue = false;
                    bool foundTransferValue = false;
                    bool foundTransferValuePreviousYear = false;
                    foreach (var sumItem in SummationTargets) {
                        if (!sumItem.IsUserDefined) {
                            if (sumItem.Value.HasValue) {
                                foundValue = true;
                                value += sumItem.WeightedValue;
                            }
                            if (.ReconciliationInfo != null && sumItem.Value.ReconciliationInfo != null) {
                                if (sumItem.Value.ReconciliationInfo.TransferValue.HasValue) {
                                    foundTransferValue = true;
                                    transferValue += sumItem.Weight*sumItem.Value.ReconciliationInfo.TransferValue.Value;
                                }

                                if (sumItem.Value.ReconciliationInfo.TransferValuePreviousYear.HasValue) {
                                    foundTransferValuePreviousYear = true;
                                    transferValuePreviousYear += sumItem.Weight*
                                                                 sumItem.Value.ReconciliationInfo.TransferValuePreviousYear.
                                                                     Value;
                                }
                            }
                        }
                    }

                    // update value
                    if (foundValue) MonetaryValue.TaxonomyComputedValue = value;
                    else MonetaryValue.TaxonomyComputedValue = null;

                    if (.ReconciliationInfo != null) {
                        // update transfer value
                        if (foundTransferValue)
                            .ReconciliationInfo.ComputedValueTransfer.TaxonomyComputedValue = transferValue;
                        else
                            .ReconciliationInfo.ComputedValueTransfer.TaxonomyComputedValue = null;

                        // update transfer value previous year
                        if (foundTransferValuePreviousYear)
                            .ReconciliationInfo.ComputedValueTransferPreviousYear.TaxonomyComputedValue =
                                transferValuePreviousYear;
                        else
                            .ReconciliationInfo.ComputedValueTransferPreviousYear.TaxonomyComputedValue = null;
                    }
         */
    }
}