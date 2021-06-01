// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-02-22
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using eBalanceKitBusiness.HyperCubes.Interfaces.Structure;

namespace eBalanceKitBusiness.HyperCubes.Interfaces.ViewModels {
    public interface IHyperCube3DCube {
        IHyperCube Cube { get; }
        IHyperCubeDimension ThirdDimension { get; }
        IHyperCubeDimensionValue SelectedThirdDimensionValue { get; set; }
        IEnumerable<IHyperCubeTable> Tables { get; }
        IHyperCubeTable SelectedTable { get; }
    }
}