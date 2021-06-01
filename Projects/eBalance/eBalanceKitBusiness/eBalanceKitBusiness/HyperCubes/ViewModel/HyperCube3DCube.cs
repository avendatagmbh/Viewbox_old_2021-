// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-02-22
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Utils;
using eBalanceKitBusiness.HyperCubes.Interfaces.Structure;
using eBalanceKitBusiness.HyperCubes.Interfaces.ViewModel;

namespace eBalanceKitBusiness.HyperCubes.ViewModel {
    internal class HyperCube3DCube : NotifyPropertyChangedBase, IHyperCube3DCube {
        private static bool _processPropertyChanged = true;
        
        private readonly Dictionary<IHyperCubeDimensionValue, List<IHyperCubeRow>> _rows =
            new Dictionary<IHyperCubeDimensionValue, List<IHyperCubeRow>>();

        private readonly Dictionary<IHyperCubeDimensionValue, List<IHyperCubeColumn>> _columns =
            new Dictionary<IHyperCubeDimensionValue, List<IHyperCubeColumn>>();

        internal HyperCube3DCube(IHyperCube cube, IHyperCubeDimension columns, IHyperCubeDimension rows,
                               IHyperCubeDimension thirdDimension) {

            Cube = cube;
            ThirdDimension = thirdDimension;
            
            TablesByDimensionValue = new Dictionary<IHyperCubeDimensionValue, IHyperCubeTable>();
            foreach (IHyperCubeDimensionValue dimValue in thirdDimension.Values) {
                TablesByDimensionValue[dimValue] = new HyperCubeTable(cube, columns, rows, new[] {dimValue});
            }

            foreach (var table in TablesByDimensionValue.Values) {

                table.PropertyChanged += TablePropertyChanged;

                foreach (var row in table.AllRows) {
                    if (!_rows.ContainsKey(row.DimensionValue))
                        _rows[row.DimensionValue] = new List<IHyperCubeRow>();

                    _rows[row.DimensionValue].Add(row);
                    row.PropertyChanged += RowPropertyChanged;
                }

                foreach (var col in table.AllColumns) {
                    if (!_columns.ContainsKey(col.DimensionValue))
                        _columns[col.DimensionValue] = new List<IHyperCubeColumn>();

                    _columns[col.DimensionValue].Add(col);
                    col.PropertyChanged += ColPropertyChanged;
                }
            }

            SelectedThirdDimensionValue = ThirdDimension.Values.First();
        }

        #region event handler
        
        #region TablePropertyChanged
        private void TablePropertyChanged(object sender, PropertyChangedEventArgs e) {
            // supress recursive calls
            if (!_processPropertyChanged) return;
            _processPropertyChanged = false;

            var changedTable = (IHyperCubeTable)sender;

            foreach (var table in Tables.Where(table => table != changedTable)) {
                switch (e.PropertyName) {
                    case "ColumnHeaderHeight":
                        table.ColumnHeaderHeight = SelectedTable.ColumnHeaderHeight;
                        break;

                    case "RowHeaderWidth":
                        table.RowHeaderWidth = SelectedTable.RowHeaderWidth;
                        break;
                }
            }

            _processPropertyChanged = true;
        }
        #endregion TablePropertyChanged
        
        #region ColPropertyChanged
        private void ColPropertyChanged(object sender, PropertyChangedEventArgs e) {

            // supress recursive calls
            if (!_processPropertyChanged) return;
            _processPropertyChanged = false;

            var changedCol = (IHyperCubeColumn)sender;
            switch (e.PropertyName) {
                case "IsExpanded":
                    foreach (var col in _columns[changedCol.DimensionValue].Where(col => col != changedCol)) {
                        col.IsExpanded = changedCol.IsExpanded;
                    }
                    break;

                case "IsSelected":
                    foreach (var col in _columns[changedCol.DimensionValue].Where(col => col != changedCol)) {
                        col.IsSelected = changedCol.IsSelected;
                    }
                    break;
            }

            _processPropertyChanged = true;
        }
        #endregion ColPropertyChanged

        #region RowPropertyChanged
        private void RowPropertyChanged(object sender, PropertyChangedEventArgs e) {

            // supress recursive calls
            if (!_processPropertyChanged) return;
            _processPropertyChanged = false;

            var changedRow = (IHyperCubeRow)sender;
            switch (e.PropertyName) {
                case "IsExpanded":
                    foreach (var row in _rows[changedRow.DimensionValue].Where(row => row != changedRow)) {
                        row.IsExpanded = changedRow.IsExpanded;
                    }
                    break;

                case "IsSelected":
                    foreach (var row in _rows[changedRow.DimensionValue].Where(row => row != changedRow)) {
                        row.IsSelected = changedRow.IsSelected;
                    }
                    break;
            }

            _processPropertyChanged = true;
        }
        #endregion RowPropertyChanged

        #endregion event handler

        private Dictionary<IHyperCubeDimensionValue, IHyperCubeTable> TablesByDimensionValue { get; set; }

        #region Tables
        public IEnumerable<IHyperCubeTable> Tables { get { return TablesByDimensionValue.Values; } }
        #endregion Tables

        public IHyperCube Cube { get; private set; }

        #region ThridDimension
        public IHyperCubeDimension ThirdDimension { get; private set; }
        #endregion ThridDimension

        #region SelectedThirdDimensionValue
        private IHyperCubeDimensionValue _selectedThirdDimensionValue;

        public IHyperCubeDimensionValue SelectedThirdDimensionValue {
            get { return _selectedThirdDimensionValue; }
            set {
                _selectedThirdDimensionValue = value;
                SelectedTable = value == null ? null : TablesByDimensionValue[value];
                OnPropertyChanged("SelectedThirdDimensionValue");
            }
        }
        #endregion SelectedThirdDimensionValue

        #region SelectedTable
        private IHyperCubeTable _selectedTable;

        public IHyperCubeTable SelectedTable {
            get { return _selectedTable; }
            private set {
                if (_selectedTable != null) _selectedTable.IsSelected = false;
                _selectedTable = value;
                if (value != null) value.IsSelected = true;
                OnPropertyChanged("SelectedTable");
            }
        }
        #endregion SelectedTable
    }
}