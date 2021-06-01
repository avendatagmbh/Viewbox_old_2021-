// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-12-17
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;

namespace federalGazetteBusiness.Structures.ValueTypes {
    public class FederalGazetteElementText : FederalGazetteElementInfoBase {
         
        public FederalGazetteElementText(string elementName, bool isAllowed = true) : base(string.Empty, elementName, isAllowed) {
        }

        public FederalGazetteElementText(string elementName, string taxonomyElementId, bool isAllowed = true)
            : base(string.Empty, elementName, taxonomyElementId, isAllowed) {
        }

        public FederalGazetteElementText(string elementName, IEnumerable<string> taxonomyElementIds, bool isAllowed = true)
            : base(string.Empty, elementName, taxonomyElementIds, isAllowed) {
        }

        #region Overrides of FederalGazetteElementInfoBase
        public override FederalGazetteElementType Type { get { return FederalGazetteElementType.Text; } }
        #endregion
    }
}