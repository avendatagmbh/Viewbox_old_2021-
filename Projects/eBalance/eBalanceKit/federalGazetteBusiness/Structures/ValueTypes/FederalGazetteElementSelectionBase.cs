// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-12-14
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Linq;
using Utils;

namespace federalGazetteBusiness.Structures.ValueTypes
{
    public abstract class FederalGazetteElementSelectionBase : FederalGazetteElementInfoBase {
        protected FederalGazetteElementSelectionBase(string caption, ObservableCollectionAsync<IFederalGazetteElementInfo> options, bool isNullable = false, IFederalGazetteElementInfo defaultOption = null, bool isSelectable = true, string taxonomyElement = null) : base(string.Empty, caption, taxonomyElement, isSelectable) {
            Options = options;
            IsNullable = isNullable;
            SelectedOption = IsNullable ? defaultOption : (defaultOption ?? options.First());
            //IsSelectable = isSelectable;
            IsAllowed = isSelectable;
            TaxonomyElement = taxonomyElement;
        }

        public ObservableCollectionAsync<IFederalGazetteElementInfo> Options { get; set; }
        
        #region SelectedOption
        private IFederalGazetteElementInfo _selectedOption;

        public IFederalGazetteElementInfo SelectedOption {
            get { return _selectedOption; }
            set {
                if (_selectedOption != value) {
                    if (value == null && !IsNullable) {
                        return;
                    }
                    var valueToSet = value as IFederalGazetteElementInfo;
                    if (valueToSet == null && Options.Any(o => o.Value == value)) {
                        valueToSet = Options.First(o => o.Value == value);
                    }
                    _selectedOption = valueToSet;
                    OnPropertyChanged("SelectedOption");
                    Value = _selectedOption == null ? null : _selectedOption.Value;
                }
            }
        }
        #endregion // SelectedOption

        //public bool IsSelectable { get; set; }
        public string TaxonomyElement { get; set; }

        #region Overrides of FederalGazetteElementInfo

        //public new object Value { get { return _selectedOption.Value; } }
        #endregion
    }
}