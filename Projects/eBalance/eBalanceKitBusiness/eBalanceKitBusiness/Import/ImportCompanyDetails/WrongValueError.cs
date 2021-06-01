using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;

namespace eBalanceKitBusiness.Import.ImportCompanyDetails {
    public class WrongValueError : NotifyPropertyChangedBase {

        public WrongValueError(CompanyDetailsImporter owner) { _owner = owner; }
        #region Properties
        public int RowNumber { get; set; }
        public string File { get; set; }
        public string Path { get; set; }
        public string TaxonomyName { get; set; }
        public string TaxonomyId { get; set; }
        public string Value { get; set; }
        private readonly CompanyDetailsImporter _owner;
        public IEnumerable<KeyValuePair<string, string>> ValueList { get; set; }
        //public List<string> ValueList { get; set; }
        //public string SelectedValueId { get; set; }

        #region SelectedValueId
        private KeyValuePair<string, string> _selectedValueId;

        public KeyValuePair<string, string> SelectedValueId {
            get { return _selectedValueId; }
            set {
                foreach (WrongValueError wrongValueError in _owner.ValueErrors) {
                    if (wrongValueError.Value == Value) {
                        wrongValueError.ChangeSelectedId(value);
                    }
                }
                _owner.UpdateHasValueErrors();
            }
        }
        #endregion SelectedValueId
        public int Column { get; set; }
        //public Dictionary<string, string> SelectedValueId { get; set; }
        #endregion Properties
        #region Methods
        public void ChangeSelectedId(KeyValuePair<string, string> to) { _selectedValueId = to; OnPropertyChanged("SelectedValueId"); }
        #endregion
    }
}
