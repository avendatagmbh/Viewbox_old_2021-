// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-18
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using Utils;
using eBalanceKitBusiness.HyperCubes.Interfaces.Structure;
using eBalanceKitBusiness.HyperCubes.Interfaces.ViewModels;
using eBalanceKitBusiness.HyperCubes.Structures;

namespace eBalanceKitBusiness.HyperCubes.ViewModels {
    internal class HyperCubeRow : NotifyPropertyChangedBase, IHyperCubeRow, IEnumerable {

        private readonly Dictionary<string, IHyperCubeTableEntry> _itemsByName =
            new Dictionary<string, IHyperCubeTableEntry>();

        private readonly Dictionary<IHyperCubeColumn, IHyperCubeTableEntry> _itemsByColumn =
            new Dictionary<IHyperCubeColumn, IHyperCubeTableEntry>();

        #region constructor
        internal HyperCubeRow(HyperCubeTable table, IHyperCubeRow parent, IHyperCubeDimensionValue rowDimensionValue, string headerNumber, int level, bool isNegativeSumItem) {
            Table = table;
            Parent = parent;
            DimensionValue = rowDimensionValue;
            HeaderNumber = headerNumber; 
            Level = level;
            IsNegativeSumItem = isNegativeSumItem;

            var items = (HyperCubeItemCollection) ((HyperCube) table.Cube).Items;
            foreach (var col in table.AllColumns) {
                var item = new HyperCubeTableEntry(
                    table.Cube, this, col, col.DimensionValue,
                    items.GetItem(rowDimensionValue, col.DimensionValue, table.FixedDimensionCoordinates));

                _itemsByColumn[col] = item;
                _itemsByName[col.Header] = item;
            }
        }

        #endregion

        #region Properties

        #region IsExpanded
        private bool _isExpanded;

        public bool IsExpanded {
            get { return _isExpanded; }
            set {
                _isExpanded = value;
                UpdateVisibility(this);
                OnPropertyChanged("IsExpanded");
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

        #region IsVisible
        public bool IsVisible { get { return Parent == null || (Parent.IsVisible && Parent.IsExpanded); } }
        #endregion

        #region Height
        private double _height;

        public double Height {
            get { return _height; }
            set {
                if (_height != value) {
                    _height = value;
                    OnPropertyChanged("Height");
                }
            }
        }
        #endregion

        #region Table
        public IHyperCubeTable Table { get; private set; }
        #endregion

        #region DimensionValue
        private IHyperCubeDimensionValue _dimensionValue;

        /// <summary>
        /// Assigned row dimension coordinate.
        /// </summary>
        public IHyperCubeDimensionValue DimensionValue { get { return _dimensionValue; } private set { _dimensionValue = value; } }
        #endregion

        #region Parent
        public IHyperCubeRow Parent { get; private set; }
        #endregion

        #region Children
        private readonly List<IHyperCubeRow> _children = new List<IHyperCubeRow>();
        public IEnumerable<IHyperCubeRow> Children { get { return _children; } }
        #endregion

        #region HasChildren
        public bool HasChildren { get { return _children.Count > 0; } }
        #endregion

        #region Level
        private int _level;

        public int Level {
            get { return _level; }
            private set {
                if (_level == value) return;
                _level = value;
                OnPropertyChanged("Level");
            }
        }
        #endregion

        #region IsNegativeSumItem
        public bool IsNegativeSumItem { get; private set; }
        #endregion IsNegativeSumItem

        #region HeaderNumber
        public string HeaderNumber { get; set; }
        #endregion

        #region Header
        public string Header { get { return HeaderNumber + ". " + DimensionValue.Element.MandatoryLabel; } }
        #endregion

        #endregion

        internal void AddChildren(IHyperCubeRow row) { _children.Add(row); }
        
        private static void UpdateVisibility(IHyperCubeRow root) {
            foreach (var child in root.Children) {
                ((HyperCubeRow)child).OnPropertyChanged("IsVisible");
                UpdateVisibility(child);
            }
        }

        public IEnumerator GetEnumerator() { return _itemsByColumn.Values.GetEnumerator(); }
        public IHyperCubeTableEntry this[string name] { get { return _itemsByName[name]; } }
        public IHyperCubeTableEntry this[IHyperCubeColumn name] { get { return _itemsByColumn[name]; } }
    }
}