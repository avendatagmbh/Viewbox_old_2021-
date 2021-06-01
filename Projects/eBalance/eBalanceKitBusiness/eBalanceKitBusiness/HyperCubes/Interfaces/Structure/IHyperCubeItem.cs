// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-16
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using Taxonomy;

namespace eBalanceKitBusiness.HyperCubes.Interfaces.Structure {
    public interface IHyperCubeItem {        
        /// <summary>
        /// Gets the value of the hypercube item.
        /// </summary>
        /// <value>The value.</value>
        object Value { get; set; }

        /// <summary>
        /// Gets the primary dimension value of the hypercube item.
        /// </summary>
        IElement PrimaryDimensionValue { get; }

        /// <summary>
        /// Gets the context of the item, which is used in xbrl export to assign the respective context.
        /// </summary>
        IScenarioContext Context { get; }

        /// <summary>
        /// True, iif. this item has an assigned value.
        /// </summary>
        bool HasValue { get; }

        /// <summary>
        /// Returns true, if the value of this item is editable.
        /// </summary>
        bool IsEditable { get; }

        /// <summary>
        /// Only used for monetary values. True, iif. this item is computed.
        /// </summary>
        bool IsComputed { get; }

        /// <summary>
        /// Only used for monetary values. True, iif. this item is locked or is computed.
        /// </summary>
        bool IsLocked { get; }
    }
}