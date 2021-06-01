// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-16
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;

namespace eBalanceKitBusiness.HyperCubes.Interfaces.Structure {
    
    /// <summary>
    /// This interface represents a collection of all hypercube dimensions.
    /// </summary>
    public interface IHyperCubeDimensionSet {

        /// <summary>
        /// Primary dimension of the hypercube.
        /// </summary>
        IHyperCubeDimension Primary { get; }

        /// <summary>
        /// Enumeration of all secondary dimensions the hypercube.
        /// </summary>
        IEnumerable<IHyperCubeDimension> DimensionItems { get; }

        /// <summary>
        /// Enumeration of all dimensions, including the primary dimension.
        /// </summary>
        IEnumerable<IHyperCubeDimension> AllDimensionItems { get; }

        /// <summary>
        /// Returns the dimension with the specified index.
        /// </summary>
        IHyperCubeDimension GetDimension(int index);

        /// <summary>
        /// Returns the DimensionValue with the specified ElementId
        /// </summary>
        IHyperCubeDimensionValue GetDimensionValueById(long id);
    }
}