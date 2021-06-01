// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-18
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;

namespace eBalanceKitBusiness.HyperCubes.Interfaces.Structure {
    public interface IHyperCubeItemCollection {
        IHyperCubeItem GetItem(long dimensionKey);
        IHyperCubeItem GetItemByID(int id);
        long GetItemDimensionKey(int id);
        IEnumerable<IHyperCubeItem> Items { get; }

        IHyperCubeItem GetItem(IEnumerable<IHyperCubeDimensionValue> dimensionCoordinates);
        
        /// <summary>
        /// Returns the unique dimension key of the specified HyperCubeItem. Used for logs.
        /// </summary>
        /// <Author>Sebastian Vetter</Author>
        long GetItemDimensionKey(IHyperCubeItem cubeItem);
    }
}