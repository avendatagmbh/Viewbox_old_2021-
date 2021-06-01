// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-09-09
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Taxonomy;
using Taxonomy.Enums;
using eBalanceKitBusiness.AuditCorrections;
using eBalanceKitBusiness.Interfaces.PresentationTree;
using eBalanceKitBusiness.Logs;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.MappingTemplate;
using eBalanceKitBusiness.Reconciliation;
using eBalanceKitBusiness.Rights;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Structures.DbMapping.ValueMappings;
using eBalanceKitBusiness.Structures.ValueTree;

namespace eBalanceKitBusiness.Structures.XbrlElementValue {
    public abstract class XbrlElementValueBase : INotifyPropertyChanged {
        #region constructor
        protected XbrlElementValueBase(IElement element, ValueTreeNode parent, IValueMapping dbValue) {
            Element = element;
            Parent = parent;
            DbValue = dbValue;

            if (element.ValueType == XbrlElementValueTypes.Monetary) {
                // todo move this to the constructor of xbrlElementValue_Monetary.cs
                MonetaryValue = new ComputedValue();
                MonetaryValue.PropertyChanged += MonetaryValuePropertyChanged;
            }
            
            if (dbValue != null)
                SetValue(dbValue.Value);
        }

        void MonetaryValuePropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case "Value": {
                    if (!SupressStateUpdate) {
                        SupressStateUpdate = true;
                        ForceValueComputationForParents();

                        if (!InitMode)
                            UpdateState();

                        SupressStateUpdate = false;
                    } else {
                        ForceValueComputationForParents();
                    }

                    OnPropertyChanged("DisplayString");
                    OnPropertyChanged("Value");
                    OnPropertyChanged("DecimalValue");
                    break;
                }

                case "ManualValue":
                    OnPropertyChanged("HasManualValue");
                    break;

                case "HasComputedValue":
                    OnPropertyChanged("HasComputedValue");
                    break;
            }
        }
        #endregion

        #region events
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string property) {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
        #endregion events

        #region properties

        #region MonetaryValue
        /// <summary>
        /// Detail values (manual value, computed value etc.) for monetary type.
        /// </summary>
        public ComputedValue MonetaryValue { get; private set; }
        #endregion MonetaryValue

        //--------------------------------------------------------------------------------        
        
        #region Value
        public virtual object Value {
            get { return GetValue(); }
            set {
                object oldValueObj = GetValue();
                string oldValue = (oldValueObj == null ? null : oldValueObj.ToString());
                SetValue(value);
                object newValueObj = GetValue();
                string newValue = (newValueObj == null ? null : newValueObj.ToString());

                if (Element.IsPersistant) {
                    // Save value, if it is no computed monetary value. For monetary values, only 
                    // manual entered values will be saved, which is done in the ManuelValue property.
                    DbValue.Value = (newValueObj != null ? newValueObj.ToString() : null);
                }

                OnPropertyChanged("DisplayString");
                OnPropertyChanged("Value");
                OnPropertyChanged("DecimalValue");

                // save value, if it has been changed
                if (DoDbUpdate && Element.IsPersistant && oldValue != newValue) {
                    SaveValueToDb("ValueChanged", oldValue);
                }

            }
        }
        #endregion Value

        #region ValueOther
        // TODO: integrate Property into Value Property
        public string ValueOther {
            get { return DbValue.ValueOther; }
            set {
                string oldValue = DbValue.ValueOther;
                DbValue.ValueOther = value;
                OnPropertyChanged("ValueOther");

                if (DoDbUpdate && oldValue != value)
                    SaveValueToDb("ValueOtherChanged", oldValue);
            }
        }
        #endregion

        #region Comment
        public virtual string Comment {
            get { return DbValue.Comment; }
            set {
                DbValue.Comment = value;
                if (DoDbUpdate) SaveValueToDb();
                OnPropertyChanged("Comment");
            }
        }
        #endregion Comment

        #region DecimalValue
        public decimal DecimalValue {
            get {
                if (Element.ValueType == XbrlElementValueTypes.Monetary)
                    return MonetaryValue.Value.HasValue ? MonetaryValue.Value.Value : 0;

                if (IsNumeric)
                    return HasValue ? Convert.ToDecimal(Value) : 0;

                return 0;
            }
        }
        #endregion

        static bool SupressStateUpdate { get; set; }
        public static bool InitMode { get; set; }

        public abstract bool HasValue { get; }
        public abstract bool IsNumeric { get; }
        public virtual bool IsReportable { get; set; }

        public IElement Element { get; private set; }
        public ValueTreeNode Parent { get; private set; }

        public ReportRights ReportRights {
            get { return Parent.ValueTree.Document != null ? Parent.ValueTree.Document.ReportRights : null; }
        }

        public bool IsCompanyEditAllowed {
            get {
                return (Parent.ValueTree.Company != null && RightManager.RightDeducer.CompanyEditable(Parent.ValueTree.Company)); 
            }
        }

        #region ValidationWarning
        private bool _validationWarning;

        public bool ValidationWarning {
            get { return _validationWarning && !Options.GlobalUserOptions.UserOptions.HideAllWarnings; }
            set {
                _validationWarning = value;
                OnPropertyChanged("ValidationWarning");
            }
        }
        #endregion

        #region ValidationWarningMessage
        private string _validationWarningMessage;

        public string ValidationWarningMessage {
            get { return _validationWarningMessage; }
            set {
                _validationWarningMessage = value;
                OnPropertyChanged("ValidationWarningMessage");
            }
        }
        #endregion

        #region ValidationError
        private bool _validationError;

        public bool ValidationError {
            get { return _validationError; }
            set {
                _validationError = value;
                OnPropertyChanged("ValidationError");
            }
        }
        #endregion

        #region ValidationErrorMessage
        private string _validationErrorMessage;

        public string ValidationErrorMessage {
            get { return _validationErrorMessage; }
            set {
                _validationErrorMessage = value;
                OnPropertyChanged("ValidationErrorMessage");
            }
        }
        #endregion

        #region ReconciliationInfo
        private ReconciliationInfo _reconciliationInfo;

        public ReconciliationInfo ReconciliationInfo {
            get { return _reconciliationInfo; }
            set {
                if (_reconciliationInfo != null) _reconciliationInfo.PropertyChanged -= ReconciliationInfoPropertyChanged;
                _reconciliationInfo = value;
                _reconciliationInfo.PropertyChanged += ReconciliationInfoPropertyChanged;
                OnPropertyChanged("ReconciliationInfo");
            }
        }

        private void ReconciliationInfoPropertyChanged(object sender, PropertyChangedEventArgs e) {

            if (e.PropertyName == "TransferValue" || e.PropertyName == "TransferValuePreviousYear" || e.PropertyName == "TransferValuePreviousYearCorrection") {
                if (!SupressStateUpdate) {
                    SupressStateUpdate = true;

                    UpdateComputedValue();
                    ForceValueComputationForParents();

                    if (!InitMode)
                        UpdateState();

                    SupressStateUpdate = false;
                } else {
                    ForceValueComputationForParents();
                }
                
                OnPropertyChanged("HasManualValue");
            }
           
            if (e.PropertyName == "HasComputedValue") {
                OnPropertyChanged("HasComputedValue");
                OnPropertyChanged("AutoComputeAllowed");
            }
        }
        #endregion ReconciliationInfo

        #region DbValue
        private IValueMapping _dbValue;

        public IValueMapping DbValue {
            get { return _dbValue; }
            private set {
                _dbValue = value;
                if (_dbValue != null) {
                    _autoComputeEnabled = _dbValue.AutoComputationEnabled;
                    _supressWarningMessages = _dbValue.SupressWarningMessages;
                    _sendAccountBalances = _dbValue.SendAccountBalances;
                }
            }
        }
        #endregion

        #region DoDbUpdate
        private static bool _doDbUpdate = true;

        internal static bool DoDbUpdate {
            get { return _doDbUpdate; }
            set { _doDbUpdate = value; }
        }
        #endregion

        #region SupressWarningMessages
        private bool _supressWarningMessages;
        // additional lokal storage for template tree view, which has no assigned DbValue

        public bool SupressWarningMessages {
            get { return _supressWarningMessages; }
            set {
                if (_supressWarningMessages != value) {
                    _supressWarningMessages = value;
                    OnPropertyChanged("HideWarning");
                    OnPropertyChanged("SupressWarningMessages");
                    if (DbValue != null) {
                        DbValue.SupressWarningMessages = value;
                        if (DoDbUpdate) SaveValueToDb("SupressWarningMessagesChanged", Convert.ToString(!value));
                    }
                }
            }
        }
        #endregion

        public void UserOptionChanged() {
            OnPropertyChanged("HideAllWarnings");
            OnPropertyChanged("HideWarning");
            OnPropertyChanged("ValidationWarning");
            OnPropertyChanged("ValidationWarningMessage");
        }

        #region HideWarning
        public bool HideWarning {
            get { return SupressWarningMessages && Options.GlobalUserOptions.UserOptions.HideChosenWarnings; }
        }
        #endregion HideWarning

        #region HideAllWarnings
        public bool HideAllWarnings {
            get { return Options.GlobalUserOptions.UserOptions.HideAllWarnings; }
        }
        #endregion HideAllWarnings

        #region SendAccountBalances
        private bool _sendAccountBalances;
        // additional lokal storage for template tree view, which has no assigned DbValue

        public bool SendAccountBalances {
            get { return _sendAccountBalances; }
            set {
                if (_sendAccountBalances != value) {
                    _sendAccountBalances = value;
                    OnPropertyChanged("SendAccountBalances");
                    OnPropertyChanged("SendAccountBalancesRecursive");
                    if (DbValue != null) {
                        DbValue.SendAccountBalances = value;
                        if (DoDbUpdate) SaveValueToDb("SendAccountBalancesChanged", Convert.ToString(!value));
                    }
                }
            }
        }
        #endregion

        #region SendAccountBalancesRecursive
        public bool SendAccountBalancesRecursive {
            get { return _sendAccountBalances; }
            set {
                var updateIds = new List<long>();
                SendAccountBalancesRecursiveGetIds(updateIds, value);
                ValueTree.ValueTree.SetSendAccountBalancesFromList(updateIds, value);
            }
        }

        /// <summary>
        /// Set the flag SendAccountBalancesRecursive without recursive call of the child flags
        /// </summary>
        public void SetSendAccountBalancesRecursive(bool setValue) { _sendAccountBalances = setValue; }

        internal void SendAccountBalancesRecursiveGetIds(List<long> updateIds, bool value) {
            foreach (var presentationTree in Element.PresentationTreeNodes) {
                foreach (var child in presentationTree.Children) {
                    if (child.Element == null) continue;
                    Debug.Assert(Parent != null, "XbrlElementValueBase.SendaccountBalancesRecursiveGetIds: Parent is not allowed to null!");
                    if (Parent.Values.ContainsKey(child.Element.Id)) {
                        var childElement = (XbrlElementValueBase) Parent.Values[child.Element.Id];
                        childElement.SendAccountBalancesRecursiveGetIds(updateIds, value);
                    } else {
                        Debug.WriteLine("SendAccountBalancesRecursiveGetIds: Id not found: " + child.Element.Id);
                    }
                }
            }
            
            if (_sendAccountBalances != value) {
                _sendAccountBalances = value;
                OnPropertyChanged("SendAccountBalances");
                OnPropertyChanged("SendAccountBalancesRecursive");
                if (DbValue != null) {
                    DbValue.SendAccountBalances = value;
                    updateIds.Add(((ValueMappingBase) DbValue).Id);
                }
            }
        }
        #endregion

        public bool IsValueVisible { get { return ReportRights != null && ReportRights.ReadRestAllowed; } }

        public bool IsEditAllowed { get { return IsEnabled && !HasComputedValue && ReportRights != null && ReportRights.WriteRestAllowed; } }
        
        public bool IsTransferValueVisible { get { return ReportRights != null && ReportRights.ReadTransferValuesAllowed; } }
        
        public bool IsEditTransferValueAllowed { get { return ReportRights != null && ReportRights.WriteTransferValuesAllowed; } }

        //--------------------------------------------------------------------------------
        #endregion properties

        #region methods
        //--------------------------------------------------------------------------------

        internal void SaveValueToDb(string change, string oldValue) {
            ValueManager.SaveValue(DbValue,
                                   new LogManager.AddValueInfo((object)Parent.ValueTree.Document ?? Parent.ValueTree.Company) {
                                                                   TaxonomyString = Element.Id,
                                                                   OldValue = oldValue,
                                                                   NewValue = DbValue,
                                                                   Change = change
                                                               });
        }

        internal void SaveValueToDb() {
            // used for value changes, which does not need a log message, e.g. user comments
            ValueManager.SaveValue(DbValue, null);
        }

        public override string ToString() {
            if (Element != null) return Element.Label;
            return "-";
        }

        protected abstract object GetValue();
        protected abstract void SetValue(object value);

        public void SetFlags(bool autoComputeEnabled, bool supressWarningMessages, bool sendAccountBalances) {
            _autoComputeEnabled = autoComputeEnabled;
            _supressWarningMessages = supressWarningMessages;
            _sendAccountBalances = sendAccountBalances;
        }

        public bool HasSummationSource(string id) { return _summationSources.Any(item => item.Value.Element.Id == id); }

        //--------------------------------------------------------------------------------
        #endregion methods

        #region Computation

        #region Class SummationItem
        internal class SummationItem {
            public IValueTreeEntry Value { get; set; }
            public decimal Weight { get; set; }

            public decimal WeightedValue {
                get { return Weight * Value.DecimalValue; }
            }

            public bool IsUserDefined { get; set; }
        }
        #endregion

        #region Class SummationSource
        public class SummationSource {
            public IValueTreeEntry Value { get; set; }
            public bool IsUserDefined { get; set; }
        }
        #endregion

        #region SummationTargets
        private readonly List<SummationItem> _summationTargets = new List<SummationItem>();
        internal List<SummationItem> SummationTargets { get { return _summationTargets; } }
        #endregion

        #region SummationSources
        private List<SummationSource> _summationSources = new List<SummationSource>();
        public List<SummationSource> SummationSources { get { return _summationSources; } }
        #endregion

        #region AddSummationTarget
        public void AddSummationTarget(IValueTreeEntry value, decimal weight) { _summationTargets.Add(new SummationItem { Value = value, Weight = weight }); }
        #endregion

        #region AddSummationSource
        public void AddSummationSource(IValueTreeEntry value) { _summationSources.Add(new SummationSource { Value = value }); }
        #endregion

        #region AddUserSummationTarget
        public void AddUserSummationTarget(IValueTreeEntry value, decimal weight) { _summationTargets.Add(new SummationItem { Value = value, Weight = weight, IsUserDefined = true, }); }
        #endregion

        #region AddUserSummationSource
        public void AddUserSummationSource(IValueTreeEntry value) {
            _summationSources.Add(new SummationSource {
                Value = value,
                IsUserDefined = true
            });
        }
        #endregion

        #region ForceValueComputationForParents
        internal void ForceValueComputationForParents() {
            foreach (var source in SummationSources) {
                ((XbrlElementValueBase)source.Value).UpdateComputedValue();
            }
        }
        #endregion

        #region UpdateComputedValue
        public void UpdateComputedValue(IPresentationTreeNode pTreeNode = null, bool sumAll = false) {
            if (!(this is XbrlElementValue_Monetary)) return;

            //System.Diagnostics.Debug.WriteLine("UpdateComputedValue: " + ToString());

            if (pTreeNode != null) {
                // compute balance list entry sum
                decimal balance = 0;
                bool foundValue = false;

                foreach (var child in pTreeNode.Children) {
                    if (child is IBalanceListEntry) {
                        // accounts, account groups and splitted accounts
                        foundValue = true;
                        var blEntry = child as IBalanceListEntry;
                        if (pTreeNode.Element.IsCreditBalance) balance -= blEntry.Amount;
                        else balance += blEntry.Amount;
                    
                    } else if (child is IAuditCorrectionTransaction) {                        
                        var transaction = child as IAuditCorrectionTransaction;
                        if (transaction.Value.HasValue) {
                            foundValue = true;
                            balance += transaction.Value.Value;
                        }
                    
                    } else if (child is MappingLineGui) {
                        // workaround for template tree views
                        foundValue = true;
                    }
                }

                if (foundValue) ((XbrlElementValueBase)pTreeNode.Value).MonetaryValue.BalanceListEntrySum = balance;
                else ((XbrlElementValueBase)pTreeNode.Value).MonetaryValue.BalanceListEntrySum = null;

            } else {
                // ComputedValue sum is computed using the taxonomy calculation rules
                {
                    decimal value = 0;
                    decimal transferValue = 0;
                    decimal transferValuePreviousYear = 0;
                    decimal transferValuePreviousYearCorrection = 0;
                    
                    bool foundValue = false;
                    bool foundTransferValue = false;
                    bool foundTransferValuePreviousYear = false;
                    bool foundTransferValuePreviousYearCorrection = false;

                    var computedValueByReconciliation = ReconciliationInfo != null
                                                    ? ReconciliationInfo.ComputedValueTransfer.
                                                          ComputedValueByReconciliation
                                                    : null;
                    if (computedValueByReconciliation != null) computedValueByReconciliation.Clear();

                    foreach (var sumItem in SummationTargets) {
                        if (!sumItem.IsUserDefined) {
                            if (sumItem.Value.HasValue) {
                                foundValue = true;
                                value += sumItem.WeightedValue;
                            }

                            if (ReconciliationInfo != null && sumItem.Value.ReconciliationInfo != null) {
                                if (sumItem.Value.ReconciliationInfo.TransferValue.HasValue) {
                                    foundTransferValue = true;
                                    transferValue += sumItem.Weight * sumItem.Value.ReconciliationInfo.TransferValue.Value;

                                    // add manual values by reconciliation sums
                                    foreach (var recValuePair in sumItem.Value.ReconciliationInfo.ComputedValueTransfer.ManualValueByReconciliation) {
                                        if (!computedValueByReconciliation.ContainsKey(recValuePair.Key))
                                            computedValueByReconciliation[recValuePair.Key] = 0;

                                        computedValueByReconciliation[recValuePair.Key] += recValuePair.Value;
                                    }
                                }

                                if (sumItem.Value.ReconciliationInfo.TransferValuePreviousYear.HasValue) {
                                    foundTransferValuePreviousYear = true;
                                    transferValuePreviousYear += sumItem.Weight *
                                                                 sumItem.Value.ReconciliationInfo.TransferValuePreviousYear.
                                                                     Value;
                                }

                                if (sumItem.Value.ReconciliationInfo.TransferValuePreviousYearCorrection.HasValue) {
                                    foundTransferValuePreviousYearCorrection = true;
                                    transferValuePreviousYearCorrection += sumItem.Weight *
                                                                 sumItem.Value.ReconciliationInfo.TransferValuePreviousYearCorrection.
                                                                     Value;
                                }
                            }
                        }
                    }

                    // update value
                    if (foundValue) MonetaryValue.TaxonomyComputedValue = value;
                    else MonetaryValue.TaxonomyComputedValue = null;

                    if (ReconciliationInfo != null) {
                        // update transfer value
                        if (foundTransferValue)
                            ReconciliationInfo.ComputedValueTransfer.TaxonomyComputedValue = transferValue;
                        else
                            ReconciliationInfo.ComputedValueTransfer.TaxonomyComputedValue = null;

                        // update transfer value previous year
                        if (foundTransferValuePreviousYear)
                            ReconciliationInfo.ComputedValueTransferPreviousYear.TaxonomyComputedValue =
                                transferValuePreviousYear;
                        else
                            ReconciliationInfo.ComputedValueTransferPreviousYear.TaxonomyComputedValue = null;

                        // update transfer value previous year correction
                        if (foundTransferValuePreviousYearCorrection)
                            ReconciliationInfo.ComputedValueTransferPreviousYearCorrection.TaxonomyComputedValue =
                                transferValuePreviousYearCorrection;
                        else
                            ReconciliationInfo.ComputedValueTransferPreviousYearCorrection.TaxonomyComputedValue = null;
                    }
                }

                // ManualComputedValue sum is computed using the manual calculation rules
                {
                    decimal value = 0;
                    decimal transferValue = 0;
                    decimal transferValuePreviousYear = 0;
                    decimal transferValuePreviousYearCorrection = 0;
                    
                    bool foundValue = false;
                    bool foundTransferValue = false;
                    bool foundTransferValuePreviousYear = false;
                    bool foundTransferValuePreviousYearCorrection = false;
                    
                    var computedValueByReconciliation = ReconciliationInfo != null
                                                    ? ReconciliationInfo.ComputedValueTransfer.
                                                          ComputedValueByReconciliation
                                                    : null;
                    if (computedValueByReconciliation != null) computedValueByReconciliation.Clear();

                    foreach (var sumItem in SummationTargets) {
                        var siValue = (XbrlElementValueBase)sumItem.Value;
                        if (sumItem.IsUserDefined && siValue.AutoComputeEnabled && siValue.AutoComputeAllowed) {
                            if (sumItem.Value.HasValue) {
                                foundValue = true;
                                value += sumItem.WeightedValue;
                            }

                            if (ReconciliationInfo != null && sumItem.Value.ReconciliationInfo != null) {

                                if (sumItem.Value.ReconciliationInfo.TransferValue.HasValue) {
                                    foundTransferValue = true;
                                    transferValue += sumItem.Weight * sumItem.Value.ReconciliationInfo.TransferValue.Value;

                                    // add manual values by reconciliation sums
                                    foreach (var recValuePair in sumItem.Value.ReconciliationInfo.ComputedValueTransfer.ManualValueByReconciliation) {
                                        if (!computedValueByReconciliation.ContainsKey(recValuePair.Key))
                                            computedValueByReconciliation[recValuePair.Key] = 0;

                                        computedValueByReconciliation[recValuePair.Key] += recValuePair.Value;
                                    }
                                }

                                if (sumItem.Value.ReconciliationInfo.TransferValuePreviousYear.HasValue) {
                                    foundTransferValuePreviousYear = true;
                                    transferValuePreviousYear += sumItem.Weight *
                                                                 sumItem.Value.ReconciliationInfo.TransferValuePreviousYear.
                                                                     Value;
                                }

                                if (sumItem.Value.ReconciliationInfo.TransferValuePreviousYearCorrection.HasValue) {
                                    foundTransferValuePreviousYearCorrection = true;
                                    transferValuePreviousYearCorrection += sumItem.Weight *
                                                                 sumItem.Value.ReconciliationInfo.TransferValuePreviousYearCorrection.
                                                                     Value;
                                }
                            }
                        }
                    }

                    // update value
                    if (foundValue) MonetaryValue.ManualComputedValue = value;
                    else MonetaryValue.ManualComputedValue = null;

                    if (ReconciliationInfo != null) {
                        // update transfer value
                        if (foundTransferValue)
                            ReconciliationInfo.ComputedValueTransfer.ManualComputedValue = transferValue;
                        else
                            ReconciliationInfo.ComputedValueTransfer.ManualComputedValue = null;

                        // update transfer value previous year
                        
                        if (foundTransferValuePreviousYear)
                            ReconciliationInfo.ComputedValueTransferPreviousYear.ManualComputedValue =
                                transferValuePreviousYear;
                        else
                            ReconciliationInfo.ComputedValueTransferPreviousYear.ManualComputedValue = null;

                        // update transfer value previous year correction
                        if (foundTransferValuePreviousYearCorrection)
                            ReconciliationInfo.ComputedValueTransferPreviousYearCorrection.ManualComputedValue =
                                transferValuePreviousYearCorrection;
                        else
                            ReconciliationInfo.ComputedValueTransferPreviousYearCorrection.ManualComputedValue = null;
                    }
                }
            }
        }
        #endregion UpdateComputedValue

        #endregion // Computation

        #region State

        #region AutoComputeAllowed
        /// <summary>
        /// True, iif. no parent is disabled or computed.
        /// </summary>
        public bool AutoComputeAllowed {
            get { return SummationSources.All(parent => parent.Value.IsEnabled && !parent.Value.HasComputedValue); }
        }
        #endregion

        #region AutoComputeEnabled
        /// <summary>
        /// User defined flag, to control if the value should be added to the parents values. 
        /// The value is only changeable, if SumToParentsAllowed == true.
        /// </summary>
        public bool AutoComputeEnabled {
            get { return _autoComputeEnabled; }
            set {
                // do not process updates, if the value has not be changed
                if (_autoComputeEnabled == value) return;

                // do not update the value, if SumToParentsAllowed == false (true iif. at least one parent node has a computed value).
                if (!AutoComputeAllowed) return;

                // update value
                _autoComputeEnabled = value;

                // update persistated value
                if (DbValue != null) {
                    DbValue.AutoComputationEnabled = value;
                    SaveValueToDb("AutoComputeEnabledChanged", Convert.ToString(!value));
                }

                // update depended values
                ForceValueComputationForParents();

                // update state
                UpdateState();

                OnPropertyChanged("HasComputedValue");
                OnPropertyChanged("IsEnabled");
            }
        }

        private bool _autoComputeEnabled;
        #endregion

        public bool HasComputedValue { get { return MonetaryValue != null && MonetaryValue.HasComputedValue; } }

        #region IsEnabled
        public bool IsEnabled {
            get {
                return _isEnabled;
            }
            private set {
                // do not process updates, if the value has not be changed
                if (_isEnabled == value) return;

                // update value
                _isEnabled = value;
                OnPropertyChanged("IsEnabled");
            }
        }
        private bool _isEnabled;
        #endregion

        #region conditions
        public bool IsComputationOrphanedNode { get { return !Element.HasComputationItems; } }
        public bool HasMonetaryParent {
            get {
                foreach (var parent in this.SummationSources) {
                    if (parent.Value.Element.ValueType == Taxonomy.Enums.XbrlElementValueTypes.Monetary) {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool HasManualValue { get { return (MonetaryValue.ManualValue.HasValue); } }

        private bool HasNonComputedValue {
            get {
                if (Element.ValueType != XbrlElementValueTypes.Monetary) return Value != null;

                if (MonetaryValue.ManualValue.HasValue)
                    return true;
                if (MonetaryValue.ManualComputedValue.HasValue)
                    return true;
                if (MonetaryValue.BalanceListEntrySum.HasValue)
                    return true;

                return false;
            }
        }
        #endregion

        #region InitStates
        /// <summary>
        /// Computes the initial state for all values in the specified value tree.
        /// </summary>
        /// <param name="vtreeRoot"></param>
        public static void InitStates(ValueTreeNode vtreeRoot) { InitStates(vtreeRoot.Values.Values); }

        /// <summary>
        /// Computes the initial state for all values in the specified value collection.
        /// </summary>
        public static void InitStates(IEnumerable<IValueTreeEntry> values) {
            foreach (XbrlElementValueBase value in values) {

                if (value.Element.ValueType != XbrlElementValueTypes.Monetary) {
                    // non monetary values
                    value.IsEnabled = true;
                } else {
                    // monetary values
                    SetState(value);
                }
            }
        }

        #endregion

        #region UpdateState

        private void UpdateState() {
            //System.Diagnostics.Debug.WriteLine("UpdateState: " + this);

            Dictionary<string, XbrlElementValueBase> valuesToUpdate = new Dictionary<string, XbrlElementValueBase>();
            valuesToUpdate.Add(Element.Id, this);

            Action<XbrlElementValueBase> getUpdatableParents = null;
            Action<XbrlElementValueBase> getUpdatableChildren = null;

            getUpdatableParents = root => {
                foreach (var sumSource in root.SummationSources) {
                    var bval = sumSource.Value as XbrlElementValueBase;
                    getUpdatableChildren(bval);
                    if (!valuesToUpdate.ContainsKey(bval.Element.Id)) {
                        valuesToUpdate.Add(bval.Element.Id, bval);
                        getUpdatableParents(bval);
                    }
                }
            };

            getUpdatableChildren = root => {
                foreach (var sumTarget in root.SummationTargets) {
                    var bval = sumTarget.Value as XbrlElementValueBase;
                    if (!valuesToUpdate.ContainsKey(bval.Element.Id)) {
                        valuesToUpdate.Add(bval.Element.Id, bval);
                        getUpdatableChildren(bval);
                    }
                }
            };

            getUpdatableChildren(this);
            getUpdatableParents(this);

            foreach (var value in valuesToUpdate.Values) {
                SetState1(value);
            }
        }
        #endregion

        #region SetState
        private static void SetState1(XbrlElementValueBase value) {

            value.IsEnabled = true;

            if (value.IsComputationOrphanedNode) {
                value.OnPropertyChanged("AutoComputeAllowed");
                value.OnPropertyChanged("AutoComputeEnabled");

                //SupressStateUpdate = true;
                //value.ForceValueComputationForParents();
                //SupressStateUpdate = false;

                return;
            }

            #region searchManualValue function
            /* 
             * Recursive search for manual values (breadth-fist search).
             * True if at least one child has a manually assigned value (including sums of assigned balance list entries).
             */
            Func<XbrlElementValueBase, bool> searchManualValue = null;
            searchManualValue = root => {
                return
                    root.SummationSources.Any(sumSource => ((XbrlElementValueBase)sumSource.Value).HasNonComputedValue) ||
                    root.SummationSources.Any(sumSource => searchManualValue((XbrlElementValueBase)sumSource.Value));
            };
            #endregion

            value.IsEnabled = !searchManualValue(value);
            //value.HasComputedValue = HasChildValues(value);
        }
        #endregion SetState

        #region SetState
        private static void SetState(XbrlElementValueBase value) {

            value.IsEnabled = true;

            if (value.IsComputationOrphanedNode) {
                value.OnPropertyChanged("AutoComputeAllowed");
                value.OnPropertyChanged("AutoComputeEnabled");

                SupressStateUpdate = true;
                value.ForceValueComputationForParents();
                SupressStateUpdate = false;

                return;
            }

            #region searchManualValue function
            /* 
             * Recursive search for manual values (breadth-fist search).
             * True if at least one child has a manually assigned value (including sums of assigned balance list entries).
             */
            Func<XbrlElementValueBase, bool> searchManualValue = null;
            searchManualValue = root => {
                return
                    root.SummationSources.Any(sumSource => ((XbrlElementValueBase)sumSource.Value).HasNonComputedValue) ||
                    root.SummationSources.Any(sumSource => searchManualValue((XbrlElementValueBase)sumSource.Value));
            };
            #endregion

            value.IsEnabled = !searchManualValue(value);
            //value.HasComputedValue = HasChildValues(value);

            value.ForceValueComputationForParents();
        }
        #endregion SetState

        #region HasChildValues
        private static bool HasChildValues(XbrlElementValueBase value) {

            if (value.SummationTargets.Count == 0) return false;

            foreach (var child in value.SummationTargets) {
                if (!child.IsUserDefined && (child.Value as XbrlElementValueBase).Value != null) {
                    return true;
                }
            }

            foreach (var child in value.SummationTargets) {
                if (!child.IsUserDefined && HasChildValues(child.Value as XbrlElementValueBase)) {
                    return true;
                }
            }

            return false;
        }
        #endregion

        #endregion // State

    }
}