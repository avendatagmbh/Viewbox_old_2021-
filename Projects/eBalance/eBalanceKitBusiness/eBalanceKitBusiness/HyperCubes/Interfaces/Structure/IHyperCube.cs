// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-16
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using Taxonomy.Interfaces.PresentationTree;
using eBalanceKitBusiness.HyperCubes.Interfaces.ViewModels;
using eBalanceKitBusiness.HyperCubes.ViewModels;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness.HyperCubes.Interfaces.Structure {
    public interface IHyperCube {
        /// <summary>
        /// 
        /// </summary>
        string Comment { get; set; }

        /// <summary>
        /// Id of the assigned taxonomy.
        /// </summary>
        int TaxonomyId { get; }

        /// <summary>
        /// 
        /// </summary>
        Document Document { get; }
        
        /// <summary>
        /// 
        /// </summary>
        IHyperCubeItemCollection Items { get; }
        
        /// <summary>
        /// 
        /// </summary>
        IPresentationTreeNode Root { get; }
        
        /// <summary>
        /// 
        /// </summary>
        IHyperCubeDimensionSet Dimensions { get; }

        /// <summary>
        /// Returns a 2-dimensional subset of the hypercube.
        /// </summary>
        IHyperCubeTable GetTable(IHyperCubeDimension dim1, IHyperCubeDimension dim2, IEnumerable<IHyperCubeDimensionValue> fixedDimensionCoordinates = null);

        /// <summary>
        /// Returns the DimensionKey (long) for the given column/row id combination
        /// </summary>
        /// <param name="column">Unique ID to identify the column. (cubetable.column.DimensionValue.ElementId)</param>
        /// <param name="row">Unique ID to identify the row. (cubetable.row.Node.ElementId)</param>
        /// <returns>Unique Key in the cube. (eg. load an item with</returns>
        /// <author>Sebastian Vetter</author>
        long GetDimensionkey(long column, long row);

        /// <summary>
        /// 
        /// </summary>
        IHyperCube3DCube Get3DCube(IHyperCubeDimension dim1, IHyperCubeDimension dim2, IHyperCubeDimension dim3);

        /// <summary> 
        /// Returns all ScenarionContext elements, which are assigned to at least one element with a value.
        /// </summary>
        IEnumerable<IScenarioContext> GetScenarioContexts();

        void Save();

        /// <summary>
        /// Deletes the persistant representation of this entity.
        /// </summary>
        void Delete();

        /// <summary>
        /// Sets all Items.Items.Value to null.
        /// </summary>
        void Clear();

        /// <summary>
        /// Load the items collection (if not called the items will automatically be loaded of first access).
        /// </summary>
        void PreloadItems();
    }
}