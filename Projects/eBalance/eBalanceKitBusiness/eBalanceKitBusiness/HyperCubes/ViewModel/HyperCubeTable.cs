// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-18
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using eBalanceKitBusiness.HyperCubes.Interfaces.Structure;
using eBalanceKitBusiness.HyperCubes.Interfaces.ViewModel;

namespace eBalanceKitBusiness.HyperCubes.ViewModel {
    internal class HyperCubeTable : Utils.NotifyPropertyChangedBase, IHyperCubeTable {

        internal HyperCubeTable(IHyperCube cube, IHyperCubeDimension columns, IHyperCubeDimension rows, IEnumerable<IHyperCubeDimensionValue> fixedDimensionCoordinates) {
            Cube = cube;

            FixedDimensionCoordinates = fixedDimensionCoordinates;

            BuildColumns(columns);
            BuildRows(rows);
        }

        #region properties

        internal IEnumerable<IHyperCubeDimensionValue> FixedDimensionCoordinates { get; private set; }

        public IHyperCube Cube { get; private set; }

        #region columns
        private readonly List<IHyperCubeColumn> _columns = new List<IHyperCubeColumn>();
        public IEnumerable<IHyperCubeColumn> Columns { get { return _columns; } }

        public IEnumerable<IHyperCubeColumn> AllColumns {
            get {
                var result = new List<IHyperCubeColumn>();
                var enumerator = new HyperCubeColumnEnumerator(Columns);
                while (enumerator.MoveNext()) result.Add(enumerator.Current);
                return result;
            }
        }
        #endregion

        #region rows
        private readonly List<IHyperCubeRow> _rows = new List<IHyperCubeRow>();
        public IEnumerable<IHyperCubeRow> Rows { get { return _rows; } }

        public IEnumerable<IHyperCubeRow> AllRows {
            get {
                var result = new List<IHyperCubeRow>();
                var enumerator = new HyperCubeRowEnumerator(Rows);
                while (enumerator.MoveNext()) result.Add(enumerator.Current);
                return result;
            }
        }
        #endregion

        #region IsSelected
        private bool _isSelected;

        public bool IsSelected {
            get { return _isSelected; }
            set {
                if (_isSelected != value) {
                    _isSelected = value;
                    OnPropertyChanged("IsSelected");
                }
            }
        }
        #endregion

        #region RowHeaderWidth
        private GridLength _rowHeaderWidth = new GridLength(400);

        public GridLength RowHeaderWidth {
            get { return _rowHeaderWidth; }
            set {
                _rowHeaderWidth = value;
                OnPropertyChanged("RowHeaderWidth");
            }
        }
        #endregion RowHeaderWidth

        #region ColumnHeaderHeight
        private GridLength _columnHeaderHeight = new GridLength(200);

        public GridLength ColumnHeaderHeight {
            get { return _columnHeaderHeight; }
            set {
                _columnHeaderHeight = value;
                OnPropertyChanged("ColumnHeaderHeight");
            }
        }
        #endregion RowHeaderWidth

        #endregion properties

        #region methods

        #region BuildColumns
        private void BuildColumns(IHyperCubeDimension columns) {
            var ordinal = 1;
            foreach (var item in columns.RootValues) BuildColumns(item, ordinal++);
        }

        private void BuildColumns(IHyperCubeDimensionValue root, int ordinal, 
            string headerNumber = null, int level = 0, HyperCubeColumn parent = null) {

            var curHeaderNumber = (headerNumber == null
                                       ? ordinal.ToString(CultureInfo.InvariantCulture)
                                       : headerNumber + "." + ordinal);

            bool isNegativeSumItem = false;
            if (root.Parent != null) {
                foreach (var item in root.Parent.Element.SummationTargets.Where(
                    item => item.Element.Id == root.Element.Id)) {

                    isNegativeSumItem = item.Weight < 0;
                }
            }
            var hcc = new HyperCubeColumn(root, parent, this, curHeaderNumber, level, isNegativeSumItem);
            
            if (parent != null) parent.AddChildren(hcc);
            else _columns.Add(hcc);

            var subOrdinal = 1;
            foreach (var child in root.Children) {
                BuildColumns(child, subOrdinal++, curHeaderNumber, level + 1, hcc);
            }
        }
        #endregion

        #region BuildRows
        private void BuildRows(IHyperCubeDimension rows) {
            var ordinal = 1;
            foreach (var item in rows.RootValues) BuildRows(item, ordinal++);
        }

        private void BuildRows(IHyperCubeDimensionValue root, int ordinal, 
            string headerNumber = null, int level = 0, HyperCubeRow parent = null) {

            var curHeaderNumber = (headerNumber == null
                                          ? ordinal.ToString(CultureInfo.InvariantCulture)
                                          : headerNumber + "." + ordinal);

            bool isNegativeSumItem = false;
            if (root.Parent != null) {
                foreach (var item in root.Parent.Element.SummationTargets.Where(
                    item => item.Element.Id == root.Element.Id)) {

                    isNegativeSumItem = item.Weight < 0;
                }
            }
            var hcr = new HyperCubeRow(this, parent, root, curHeaderNumber, level, isNegativeSumItem);

            if (parent != null) parent.AddChildren(hcr);
            else _rows.Add(hcr);

            var subOrdinal = 1;
            foreach (var child in root.Children) {
                BuildRows(child, subOrdinal++, curHeaderNumber, level + 1, hcr);
            }
        }
        #endregion

        #endregion methods

    }
}