// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-23
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using eBalanceKitBusiness.HyperCubes.Interfaces.ViewModels;

namespace eBalanceKitBusiness.HyperCubes.Interfaces.Structure {
    public interface IHyperCubeTableEntry {
        IHyperCube Cube { get; }
        IHyperCubeColumn Column { get; }
        bool IsSelected { get; set; }
        bool IsVisible { get; }
        string Header { get; }
        IHyperCubeItem Item { get; }
    }
}