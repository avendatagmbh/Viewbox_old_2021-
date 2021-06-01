using DbAccess;
using System;
using eBalanceKitBusiness.Manager;
using Utils;
using System.Collections.Generic;

namespace eBalanceKitBusiness.Structures.DbMapping.BalanceList {
    
    /// <summary>
    /// (non peristent part)
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <since>2011-06-13</since>
    internal partial class SplittedAccount : BalanceListEntryBase, ISplittedAccount {

        public SplittedAccount() {
            Validate();
        }

        #region Baseaccount
        private IAccount _baseAccount;

        public IAccount BaseAccount {
            get { return _baseAccount; }
            set { 
                _baseAccount = value;
                ComputeCorrectionValue();
            }
        }
        #endregion Baseaccount

        #region SplitAccountGroup
        ISplitAccountGroup _splitAccountGroup;

        public ISplitAccountGroup SplitAccountGroup {
            get { return _splitAccountGroup; }
            set {
                _splitAccountGroup = value;
                var sag = _splitAccountGroup as SplitAccountGroup;
                if (sag.Id == 0) sag.PropertyChanged += new global::System.ComponentModel.PropertyChangedEventHandler(sag_PropertyChanged);
                else SplitAccountGroupId = sag.Id;

                Validate();
            }
        }

        void sag_PropertyChanged(object sender, global::System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == "Id") SplitAccountGroupId = (sender as SplitAccountGroup).Id;
        }
        #endregion

        #region ValidationErrorMessages
        private ObservableCollectionAsync<string> _validationErrorMessages = new ObservableCollectionAsync<string>();
        public IEnumerable<string> ValidationErrorMessages { get { return _validationErrorMessages; } }
        #endregion

        #region IsValid
        private bool? _isValid;
        public bool IsValid {
            get {
                if (!_isValid.HasValue) Validate();
                return _isValid.Value; 
            }
            private set {
                _isValid = value;
                OnPropertyChanged("IsValid");
            }
        }
        #endregion

        public void Validate() {
            _validationErrorMessages.Clear();
            // validate number
            if (BalanceList != null) {
                if (string.IsNullOrEmpty(Number)) {
                    _validationErrorMessages.Add("Es wurde keine Kontonummer angegeben.");
                
                } else if (BalanceList.IsNumberDefined(Number, BaseAccount)) {
                    _validationErrorMessages.Add("Die angegebene Kontonummer ist bereits vergeben.");
                
                }

                foreach (var splitAccount in SplitAccountGroup.Items) {
                    if (splitAccount != this && splitAccount.Number == this.Number) {
                        _validationErrorMessages.Add("Die angegebene Kontonummer wurde in mindestens zwei Teilkonten vergeben.");
                        break;
                    }
                }
            }

            // validate name
            if (string.IsNullOrEmpty(Name)) {
                _validationErrorMessages.Add("Kontoname wurde nicht angegeben.");
            } 

            // validate percent
            if (SplitAccountGroup != null && SplitAccountGroup.ValueInputMode == ValueInputMode.Relative && !AmountPercent.HasValue) {
                _validationErrorMessages.Add("Prozentwert wurde nicht angegeben.");
            }
            IsValid = _validationErrorMessages.Count == 0;
        }

        #region CorrectionValue
        private int? _correctionValue;

        public int? CorrectionValue {
            get { return _correctionValue; }
            set { 
                _correctionValue = value;
                ComputeAmount();
                OnPropertyChanged("CorrectionValue");
            }
        }
        #endregion

        private void ComputeAmount() {
            if (BaseAccount != null && _amountPercent.HasValue) {

                // compute relative amount value
                var tmp = BaseAccount.Amount * _amountPercent.Value / 100;
                decimal amount = Math.Round(tmp, 2, MidpointRounding.AwayFromZero);

                if (CorrectionValue.HasValue) amount += (decimal)CorrectionValue.Value / 100;

                this.Amount = amount;

                Validate();
                OnPropertyChanged("Amount");
            }
        }

        private void ComputeCorrectionValue() {
            if (BaseAccount != null && AmountPercent.HasValue) {
                var tmp = BaseAccount.Amount * _amountPercent.Value / 100;
                decimal amount = Math.Round(tmp, 2, MidpointRounding.AwayFromZero);
                int tmpCorrectionValue = (int)((this.Amount - amount) * 100);
                if (tmpCorrectionValue != 0) _correctionValue = tmpCorrectionValue;
                OnPropertyChanged("CorrectionValue");
            }
        }

        public bool IsAssignedToReferenceList { get; set; }
    }
}
