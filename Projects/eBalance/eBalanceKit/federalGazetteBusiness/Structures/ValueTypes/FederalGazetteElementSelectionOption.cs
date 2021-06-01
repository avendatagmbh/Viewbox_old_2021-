// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-12-14
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using Utils;

namespace federalGazetteBusiness.Structures.ValueTypes {
    public class FederalGazetteElementSelectionOption : FederalGazetteElementSelectionBase {
        public FederalGazetteElementSelectionOption(string caption, ObservableCollectionAsync<IFederalGazetteElementInfo> options, IFederalGazetteElementInfo defaultOption = null, bool isSelectable = true, string taxonomyElement = null) : base(caption, options, false, defaultOption, isSelectable, taxonomyElement) {
        }

        #region Overrides of FederalGazetteElementInfoBase
        public override FederalGazetteElementType Type { get { return FederalGazetteElementType.SelectionOption; } }
        #endregion
    }
}