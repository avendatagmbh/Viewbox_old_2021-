using System;
using System.Collections.Generic;
using eBalanceKitBusiness.Structures.HyperCubes;

namespace eBalanceKitBusiness.Interfaces {
    
    /// <summary>
    /// Interface for hyper cubes
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <since>2011-05-02</since>
    public interface IHyperCube {

        /// <summary>
        /// Gets a 2d subtable of the hypercube.
        /// </summary>
        HyperCubeTable GetTable(int dim1, int dim2);

        /// <summary>
        /// Returns a collection of all existing items.
        /// </summary>
        /// <returns></returns>
        IEnumerable<IHyperCubeItem> GetItems();

        /// <summary>
        /// Returns a collection of all existing contexts.
        /// </summary>
        /// <returns></returns>
        IEnumerable<ScenarioContext> GetContexts();

        /// <summary>
        /// Gets the type of the implementing class.
        /// </summary>
        Type ItemType { get; }
    }
}
