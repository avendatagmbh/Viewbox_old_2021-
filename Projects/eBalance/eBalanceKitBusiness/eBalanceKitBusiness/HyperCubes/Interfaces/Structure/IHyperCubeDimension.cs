// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-16
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using Taxonomy.Interfaces.PresentationTree;

namespace eBalanceKitBusiness.HyperCubes.Interfaces.Structure {
    
    /// <summary>
    /// This interface represents a dimension of a hypercube.
    /// </summary>
    public interface IHyperCubeDimension {

        /// <summary>
        /// Dimension root node.
        /// </summary>
        IPresentationTreeNode DimensionRoot { get; }

        /// <summary>
        /// Enumeration of all root values in this dimension.
        /// </summary>
        IEnumerable<IHyperCubeDimensionValue> RootValues { get; }
        
        /// <summary>
        /// Enumeration of all values in this dimension.
        /// </summary>
        IEnumerable<IHyperCubeDimensionValue> Values { get; }

        /// <summary>
        /// Ordinal number of the dimension.
        /// </summary>
        int Ordinal { get; }
    }
}
