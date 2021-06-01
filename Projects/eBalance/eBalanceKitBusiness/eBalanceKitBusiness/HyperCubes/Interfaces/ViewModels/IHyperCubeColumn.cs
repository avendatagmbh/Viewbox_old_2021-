// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-18
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using eBalanceKitBusiness.HyperCubes.Interfaces.Structure;

namespace eBalanceKitBusiness.HyperCubes.Interfaces.ViewModels {
    public interface IHyperCubeColumn : IEnumerable, INotifyPropertyChanged {
        bool IsExpanded { get; set; }
        bool IsSelected { get; set; }
        bool IsVisible { get; }
        int Level { get; }
        bool IsNegativeSumItem { get; }
        IHyperCubeDimensionValue DimensionValue { get; }
        string Header { get; }        
        IHyperCubeColumn Parent { get; }
        IEnumerable<IHyperCubeColumn> Children { get; }
        string HeaderNumber { get; } 
    }
}