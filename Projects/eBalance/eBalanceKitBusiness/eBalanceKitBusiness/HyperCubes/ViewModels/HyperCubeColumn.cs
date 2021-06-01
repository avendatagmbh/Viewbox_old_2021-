// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-18
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Taxonomy.Enums;
using Utils;
using eBalanceKitBusiness.HyperCubes.Interfaces.Structure;
using eBalanceKitBusiness.HyperCubes.Interfaces.ViewModels;

namespace eBalanceKitBusiness.HyperCubes.ViewModels {
    internal class HyperCubeColumn : NotifyPropertyChangedBase, IHyperCubeColumn {

        internal HyperCubeColumn(IHyperCubeDimensionValue dimension, HyperCubeColumn parent, HyperCubeTable table, string headerNumber, int level, bool isNegativeSumItem) {
            DimensionValue = dimension;
            Parent = parent;
            IsVisible = (Parent == null);
            Table = table;
            HeaderNumber = headerNumber;
            Level = level;
            IsNegativeSumItem = isNegativeSumItem;
        }
        
        #region Properties

        #region IsExpanded
        private bool _isExpanded;

        public bool IsExpanded {
            get { return _isExpanded; }
            set {
                if (_isExpanded != value) {
                    _isExpanded = value;
                    OnPropertyChanged("IsExpanded");

                    if (_isExpanded) {
                        foreach (var column in Children) ((HyperCubeColumn)column).IsVisible = true;
                        if (Parent != null) {
                            ((HyperCubeColumn)Parent).IsVisible = false;
                            foreach (var child in Parent.Children) ((HyperCubeColumn)child).IsVisible = false;
                        } else {
                            foreach (var root in Table.Columns) ((HyperCubeColumn)root).IsVisible = false;
                        }
                        IsVisible = true;
                    } else {
                        foreach (var column in Children) ((HyperCubeColumn)column).IsVisible = false;
                        if (Parent != null) {
                            ((HyperCubeColumn)Parent).IsVisible = true;
                            foreach (var child in Parent.Children) ((HyperCubeColumn)child).IsVisible = true;
                        } else {
                            foreach (var root in Table.Columns) ((HyperCubeColumn)root).IsVisible = true;
                        }
                    }
                }
            }
        }
        #endregion IsExpanded
        
        #region IsVisible
        private bool _isVisible;

        public bool IsVisible {
            get { return _isVisible; }
            private set {
                _isVisible = value;
                OnPropertyChanged("IsVisible");
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

        #region Node
        /// <summary>
        /// Assigned presentationTreeNode.
        /// </summary>
        public IHyperCubeDimensionValue DimensionValue { get; private set; }
        #endregion

        #region HeaderNumber
        public string HeaderNumber { get; private set; }
        #endregion

        #region Header
        public string Header { get { return HeaderNumber + ". " + DimensionValue.Element.MandatoryLabel; } }
        #endregion

        #region Parent
        public IHyperCubeColumn Parent { get; private set; }
        #endregion

        #region DataGridColumn
        /// <summary>
        /// Assigned DataGridColumn - used to update visibility.
        /// </summary>
        public DataGridColumn DataGridColumn { get; set; }
        #endregion

        #region Children
        private readonly List<IHyperCubeColumn> _children = new List<IHyperCubeColumn>();
        public IEnumerable<IHyperCubeColumn> Children { get { return _children; } }
        #endregion

        public HyperCubeTable Table { get; set; }

        /// <summary>
        /// Returns true if children count > 0, except there is only one child column of type string 
        /// (the last case is displayed as attached value for the parent column instead of a subcolumn).
        /// </summary>
        public bool HasChildren {
            get {
                return !(_children.Count == 0 ||
                         (_children.Count == 1 && Children.First().DimensionValue.Element.ValueType == XbrlElementValueTypes.String));
            }
        }
        #endregion

        internal void AddChildren(IHyperCubeColumn column) { _children.Add(column); }

        public IEnumerator GetEnumerator() { return _children.GetEnumerator(); }

        public override string ToString() { return Header; }

    }
}