// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2011-05-09
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using Taxonomy;
using eBalanceKitBusiness.HyperCubes.Interfaces.Structure;

namespace eBalanceKitBusiness.HyperCubes.Structures {
    internal class ScenarioContext : IScenarioContext {

        internal ScenarioContext(string name, int ordinal) {
            _members = new List<IScenarioContextExplicitMember>();
            Name = "dim_ctx_" + name + "_" + ordinal;
        }

        public string Name { get; private set; }

        #region Members
        private readonly List<IScenarioContextExplicitMember> _members;
        public IEnumerable<IScenarioContextExplicitMember> Members { get { return _members; } }
        #endregion
        
        internal void AddMember(IElement dimension, IElement member) { _members.Add(new ScenarioContextExplicitMember(dimension, member)); }
    }
}