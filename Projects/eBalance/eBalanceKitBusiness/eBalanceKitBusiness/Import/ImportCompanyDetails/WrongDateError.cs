using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eBalanceKitBusiness.Import.ImportCompanyDetails {
    public class WrongDateError {
        public WrongDateError(CompanyDetailsImporter owner) {
            _owner = owner;
            _selectedValue = DateTime.Today;
        }
        public string File { get; set; }
        public string TaxonomyName { get; set; }
        public int LineNumber { get; set; }
        public string Path { get; set; }
        public string Value { get; set; }
        private readonly CompanyDetailsImporter _owner;
        //public IEnumerable<KeyValuePair<string, string>> ValueList { get; set; }

        #region SelectedValue
        private DateTime _selectedValue;

        public DateTime SelectedValue {
            get { return _selectedValue; }
            set {
                _selectedValue = value;
                _owner.UpdateHasValueErrors();
            }
        }
        #endregion SelectedValueId
        public int Column { get; set; }
        //public Dictionary<string, string> SelectedValueId { get; set; }
    }
}
