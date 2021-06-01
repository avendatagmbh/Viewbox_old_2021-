// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-12-14
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using Utils;

namespace federalGazetteBusiness.Structures.ValueTypes {
    public class FederalGazetteElementSelectionList : FederalGazetteElementSelectionBase {
        public FederalGazetteElementSelectionList(string caption, ObservableCollectionAsync<IFederalGazetteElementInfo> options, IFederalGazetteElementInfo defaultOption = null, bool isNullable = false, bool isSelectable = true, string taxonomyElement = null) : base(caption, options, isNullable, defaultOption, isSelectable, taxonomyElement) { IsNullable = false; }


        #region Overrides of FederalGazetteElementInfoBase
        public override FederalGazetteElementType Type { get { return FederalGazetteElementType.SelectionList; } }
        #endregion

    }
}