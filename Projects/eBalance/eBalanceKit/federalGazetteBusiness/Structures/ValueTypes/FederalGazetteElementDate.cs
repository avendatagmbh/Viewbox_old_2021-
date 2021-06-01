// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// --------------------------------------------------------------------------------
using System.Collections.Generic;

namespace federalGazetteBusiness.Structures.ValueTypes
{
    public class FederalGazetteElementDate : FederalGazetteElementInfoBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="caption"></param>
        /// <param name="isAllowed"></param>
        public FederalGazetteElementDate(string id, string caption, bool isAllowed = true) : base(id, caption, isAllowed)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="caption"></param>
        /// <param name="taxonomyElementId"></param>
        /// <param name="isAllowed"></param>
        public FederalGazetteElementDate(string id, string caption, string taxonomyElementId, bool isAllowed = true) : base(id, caption, taxonomyElementId, isAllowed)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="caption"></param>
        /// <param name="taxonomyElementIds"></param>
        /// <param name="isAllowed"></param>
        public FederalGazetteElementDate(string id, string caption, IEnumerable<string> taxonomyElementIds, bool isAllowed = true) : base(id, caption, taxonomyElementIds, isAllowed)
        {
        }

        public override FederalGazetteElementType Type { get { return FederalGazetteElementType.Date; } }
    }
}