// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-12-16
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using Utils;
using System.Linq;

namespace federalGazetteBusiness.Structures.ValueTypes {
    public class FederalGazetteElementBool : FederalGazetteElementSelectionBase
    {

        public FederalGazetteElementBool(string caption, bool isNullable = false) :base(caption, isNullable ? _optionsNullable : _optionsNotNullable, isNullable) {  }

        public override FederalGazetteElementType Type { get { return FederalGazetteElementType.Bool; } }


        private static ObservableCollectionAsync<IFederalGazetteElementInfo> _optionsNullable = new ObservableCollectionAsync<IFederalGazetteElementInfo> {
                                                                                    new FederalGazetteElementInfoString(null, null),
                                                                                    new FederalGazetteElementInfoString(true.ToString(), true.ToString()),
                                                                                    new FederalGazetteElementInfoString(false.ToString(), false.ToString())
                                                                                };

        private static ObservableCollectionAsync<IFederalGazetteElementInfo> _optionsNotNullable = new ObservableCollectionAsync<IFederalGazetteElementInfo> {
                                                                                    new FederalGazetteElementInfoString(true.ToString(), true.ToString()),
                                                                                    new FederalGazetteElementInfoString(false.ToString(), false.ToString())
                                                                                };

        #region Value
        public new object Value {
            get { return SelectedOption == null ? null : base.SelectedOption.Value; }
            set {
                SelectedOption =
                    Options.FirstOrDefault(
                        o => o.Value == value || (o.Value != null && value != null && o.Value.ToString() == value.ToString()));
                OnPropertyChanged("BoolValue");
            }
        }
        #endregion // Value

        #region BoolValue

        public bool BoolValue {
            get {
                bool result;
                if (SelectedOption == null || SelectedOption.Value == null || !bool.TryParse(SelectedOption.Value.ToString(), out result)) {
                    return false;
                }
                
                return result;
            }
            set { Value = value; }
        }
        #endregion // BoolValue

    }
}