// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-05
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using Taxonomy.Interfaces.PresentationTree;
using eBalanceKitBusiness.HyperCubes.Interfaces.Structure;

namespace eBalanceKitBusiness.HyperCubes.Structures {
    internal class HyperCubeDimension : IHyperCubeDimension {

        #region constructor
        internal HyperCubeDimension(HyperCube cube, IPresentationTreeNode root) {

            DimensionRoot = root;

            foreach (var pchild in root.Children.
                OfType<IPresentationTreeNode>().Where(pchild => !pchild.Element.IsHypercubeItem)) {
                
                var value = new HyperCubeDimensionValue(cube, this, pchild.Element);
                _rootValues.Add(value);
                _values.Add(value);
                InitNodesList(cube, pchild, value);
            }

            //if (root.IsHypercubeContainer) {
            //    // init primary key dimension (all children except the hypercube node belongs to this dimension)
            //    foreach (
            //        var pchild in
            //            root.Children.OfType<IPresentationTreeNode>().Where(pchild => !pchild.Element.IsHypercubeItem)) {
            //        var value = new HyperCubeDimensionValue(cube, this, pchild.Element);
            //        _rootValues.Add(value);
            //        _values.Add(value);
            //        InitNodesList(cube, pchild, value);
            //    }

            //} else {
            //    // TODO changesEquityStatement -> Use Children instead of root (first row + col are not allowed)
            //    // TODO Problem: data stored on DB seems to contain first row --> Exception in HyperCubeDimensionKeyManager

            //    foreach (var cRoot in root.Children) {
            //        var value = new HyperCubeDimensionValue(cube, this, cRoot.Element);
            //        _rootValues.Add(value);
            //        _values.Add(value);
            //        InitNodesList(cube, cRoot as IPresentationTreeNode, value);
            //    }

            //    // init regular dimension (all children belongs to this dimension and "root" is the only root node which exists)
            //    //var value = new HyperCubeDimensionValue(cube, this, root.Element);
            //    //_rootValues.Add(value);
            //    //_values.Add(value);
            //    //InitNodesList(cube, root, value);
                
            //}
        }
        #endregion constructor
        
        public int Ordinal { get; internal set; }
        public IPresentationTreeNode DimensionRoot { get; private set; }

        #region RootNodes
        private readonly List<IHyperCubeDimensionValue> _rootValues = new List<IHyperCubeDimensionValue>();
        public IEnumerable<IHyperCubeDimensionValue> RootValues { get { return _rootValues; } }
        #endregion

        #region Nodes
        private readonly List<IHyperCubeDimensionValue> _values = new List<IHyperCubeDimensionValue>();
        public IEnumerable<IHyperCubeDimensionValue> Values { get { return _values; } }
        #endregion

        #region InitNodesList
        private void InitNodesList(HyperCube cube, IPresentationTreeNode root, IHyperCubeDimensionValue parentDimensionValue) {

            Action<IPresentationTreeNode, IHyperCubeDimensionValue> getAllChildNodes = null;
            getAllChildNodes = (currentRoot, currentParentDimensionValue) => {
                foreach (var pchild in currentRoot.Children.OfType<IPresentationTreeNode>().Select(child => child)) {
                    var newParentDimensionValue =
                        new HyperCubeDimensionValue(cube, this, pchild.Element, currentParentDimensionValue);
                    
                    _values.Add(newParentDimensionValue);

                    // ReSharper disable PossibleNullReferenceException
                    // ReSharper disable AccessToModifiedClosure
                    getAllChildNodes(pchild, newParentDimensionValue);
                    // ReSharper restore AccessToModifiedClosure
                    // ReSharper restore PossibleNullReferenceException
                }
            };

            getAllChildNodes(root, parentDimensionValue);
        }
        #endregion InitNodesList

    }
}