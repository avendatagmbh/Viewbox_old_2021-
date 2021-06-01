// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-12-14
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;

namespace federalGazetteBusiness.Structures.ValueTypes
{
    public class FederalGazetteElementInfoString : FederalGazetteElementInfoBase
    {
        public FederalGazetteElementInfoString(string id, string caption, bool isAllowed = true) : base(id, caption, isAllowed) { Value = id; }

        public FederalGazetteElementInfoString(string id, string caption, string taxonomyElementId, bool isAllowed = true) : base(id, caption, taxonomyElementId, isAllowed)
        {
            Value = id;
        }

        public FederalGazetteElementInfoString(string id, string caption, IEnumerable<string> taxonomyElementIds, bool isAllowed = true)
            : base(id, caption, taxonomyElementIds, isAllowed)
        {
            Value = id;
        }

        public override FederalGazetteElementType Type { get { return FederalGazetteElementType.String; } }

        protected override void SaveValue() {
            //base.SaveValue();
        }

        public override string Caption { get { return ElementName; } }
    }
}