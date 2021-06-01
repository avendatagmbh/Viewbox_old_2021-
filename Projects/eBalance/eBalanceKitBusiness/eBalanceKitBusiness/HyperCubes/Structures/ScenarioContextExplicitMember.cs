// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2012-01-16
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using Taxonomy;
using eBalanceKitBusiness.HyperCubes.Interfaces.Structure;

namespace eBalanceKitBusiness.HyperCubes.Structures {
    internal class ScenarioContextExplicitMember : IScenarioContextExplicitMember {

        internal ScenarioContextExplicitMember(IElement dimension, IElement member) {
            Dimension = dimension;
            Member = member;
        }

        public IElement Dimension { get; private set; }
        public IElement Member { get; private set; }
    }
}