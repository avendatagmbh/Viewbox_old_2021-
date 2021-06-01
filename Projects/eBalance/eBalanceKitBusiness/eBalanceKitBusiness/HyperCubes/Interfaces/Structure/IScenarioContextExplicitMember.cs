// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2012-01-16
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using Taxonomy;

namespace eBalanceKitBusiness.HyperCubes.Interfaces.Structure {

    /// <summary>
    /// This interface defines an explicit member for scenario contexts. Needed for xbrl instance document creation.
    /// </summary>
    public interface IScenarioContextExplicitMember {

        /// <summary>
        /// Assigned dimension id element.
        /// </summary>
        IElement Dimension { get; }

        /// <summary>
        /// Assigned dimension member element.
        /// </summary>
        IElement Member { get; }
    }
}