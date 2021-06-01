// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2012-01-16
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;

namespace eBalanceKitBusiness.HyperCubes.Interfaces.Structure {
    public interface IScenarioContext {
        /// <summary>
        /// Unique name to refer the context in xbrl instance documents.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Explicit members (specific dimension value) of the scenario.
        /// </summary>
        IEnumerable<IScenarioContextExplicitMember> Members { get; }
    }
}