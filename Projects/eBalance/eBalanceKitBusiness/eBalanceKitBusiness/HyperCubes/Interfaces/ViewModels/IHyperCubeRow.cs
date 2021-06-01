// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-18
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.ComponentModel;
using eBalanceKitBusiness.HyperCubes.Interfaces.Structure;

namespace eBalanceKitBusiness.HyperCubes.Interfaces.ViewModels {
    public interface IHyperCubeRow : INotifyPropertyChanged {
        bool IsExpanded { get; set; }
        bool IsSelected { get; set; }
        bool IsVisible { get; }
        int Level { get; }
        bool IsNegativeSumItem { get; }
        IHyperCubeDimensionValue DimensionValue { get; }
        string Header { get; }
        IHyperCubeRow Parent { get; }
        IEnumerable<IHyperCubeRow> Children { get; }
        string HeaderNumber { get; }
        IHyperCubeTableEntry this[string name] { get; }
        IHyperCubeTableEntry this[IHyperCubeColumn name] { get; }
    }
}