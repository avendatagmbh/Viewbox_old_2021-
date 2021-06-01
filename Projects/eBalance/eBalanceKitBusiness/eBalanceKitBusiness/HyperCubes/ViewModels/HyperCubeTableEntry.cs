// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-23
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using Utils;
using eBalanceKitBusiness.HyperCubes.Interfaces.Structure;
using eBalanceKitBusiness.HyperCubes.Interfaces.ViewModels;

namespace eBalanceKitBusiness.HyperCubes.ViewModels {
    internal class HyperCubeTableEntry : NotifyPropertyChangedBase, IHyperCubeTableEntry {

        internal HyperCubeTableEntry(IHyperCube cube, IHyperCubeRow row, IHyperCubeColumn column, IHyperCubeDimensionValue dimValue, IHyperCubeItem item) {
            Cube = cube;
            Row = row;
            Column = column;
            Column.PropertyChanged += (sender, e) => OnPropertyChanged(e.PropertyName);

            DimensionValue = dimValue;
            Item = item;
        }

        public IHyperCube Cube { get; private set; }
        public IHyperCubeRow Row { get; private set; }
        public IHyperCubeColumn Column { get; private set; }

        #region IsSelected
        private bool _isSelected;

        public bool IsSelected {
            get { return _isSelected; }
            set {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }
        #endregion

        public bool IsVisible { get { return Column.IsVisible; } }

        #region DimensionValue
        private IHyperCubeDimensionValue DimensionValue { get; set; }
        #endregion

        #region Header
        public string Header { get { return DimensionValue.Element.Label; } }
        #endregion

        #region Item
        public IHyperCubeItem Item { get; private set; }
        #endregion

    }
}