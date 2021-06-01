// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-26
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using Taxonomy;
using Utils;
using eBalanceKitBusiness.HyperCubes.Interfaces.Structure;
using eBalanceKitBusiness.HyperCubes.Structures.HyperCubeItem;

namespace eBalanceKitBusiness.HyperCubes.Structures {
    internal class HyperCubeDimensionValue : NotifyPropertyChangedBase, IHyperCubeDimensionValue {

        #region constructor
        internal HyperCubeDimensionValue(HyperCube cube, IHyperCubeDimension dimension, IElement element, IHyperCubeDimensionValue parent = null) {
            Dimension = dimension;
            Element = element;
            Parent = parent;
            if (parent != null) ((HyperCubeDimensionValue) parent)._children.Add(this);
            ElementId = cube.TaxonomyIdManager.GetId(element.Id);
        }
        #endregion constructor

        #region Children
        private readonly List<HyperCubeDimensionValue> _children = new List<HyperCubeDimensionValue>();
        public IEnumerable<IHyperCubeDimensionValue> Children { get { return _children; } }
        #endregion

        #region IsLocked
        private readonly Dictionary<HyperCubeMonetaryItemGroup, bool> _isLocked = new Dictionary<HyperCubeMonetaryItemGroup, bool>();
        internal Dictionary<HyperCubeMonetaryItemGroup, bool> IsLocked { get { return _isLocked; } }
        #endregion IsLocked

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

        public IHyperCubeDimension Dimension { get; private set; }
        public IHyperCubeDimensionValue Parent { get; private set; }
        public bool IsRoot { get { return Parent == null; } }
        public IElement Element { get; private set; }
        public long ElementId { get; private set; }
        public string Label { get { return Element.Label; } }                
        public override string ToString() { return Label; }
    }
}