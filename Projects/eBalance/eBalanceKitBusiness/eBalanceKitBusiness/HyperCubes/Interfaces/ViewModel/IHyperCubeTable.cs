// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-18
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using eBalanceKitBusiness.HyperCubes.Interfaces.Structure;

namespace eBalanceKitBusiness.HyperCubes.Interfaces.ViewModel {
    public interface IHyperCubeTable : INotifyPropertyChanged {

        /// <summary>
        /// Assigned hyper cube.
        /// </summary>
        IHyperCube Cube { get; }

        IEnumerable<IHyperCubeColumn> Columns { get; }
        IEnumerable<IHyperCubeColumn> AllColumns { get; }
        
        IEnumerable<IHyperCubeRow> Rows { get; }
        IEnumerable<IHyperCubeRow> AllRows { get; }

        bool IsSelected { get; set; }
        GridLength ColumnHeaderHeight { get; set; }
        GridLength RowHeaderWidth { get; set; }
    }
}