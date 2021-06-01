using Taxonomy.PresentationTree;

namespace eBalanceKitBusiness.Interfaces {

    /// <summary>
    /// Internal interface for Hyper cubes.
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <since>2011-05-02</since>
    internal interface IHyperCubeInternal : IHyperCube {
        
        /// <summary>
        /// Gets the number of dimensions for the hypercube.
        /// </summary>
        int Dimensions { get; }
        
        /// <summary>
        /// Gets the specified hypercube item.
        /// </summary>
        /// <param name="dimensionKeys">XBRL-Name of all neccesary dimensions. Unused dimensions could be specified as null values.</param>
        /// <returns></returns>
        IHyperCubeItem GetItem(params string[] dimensionKeys);
        
        /// <summary>
        /// Gets the root nodes for the dimensions of the hypercube.
        /// </summary>
        Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode[] DimensionRoots { get; }
    }
}
