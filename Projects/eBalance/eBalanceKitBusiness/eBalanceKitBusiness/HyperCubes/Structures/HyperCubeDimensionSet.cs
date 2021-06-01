// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-16
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Taxonomy.Interfaces.PresentationTree;
using eBalanceKitBusiness.HyperCubes.Interfaces.Structure;
using eBalanceKitBusiness.HyperCubes.Manager;

namespace eBalanceKitBusiness.HyperCubes.Structures {
    internal class HyperCubeDimensionSet : IHyperCubeDimensionSet {
        
        #region constructor
        public HyperCubeDimensionSet(HyperCube cube) {

            Debug.Assert(cube != null && cube.Root != null && cube.Root.IsHypercubeContainer);
            
            Primary = new HyperCubeDimension(cube, cube.Root);

            // process all dimension nodes
            var dimNodes =
                (
                    from child in cube.Root.Children
                    where child.Element.IsHypercubeItem
                    from dimNode in ((IPresentationTreeNode) child)
                    where dimNode.Element.IsDimensionItem
                    select dimNode)
                    .Cast<IPresentationTreeNode>()
                    .ToList();

            var dimensionItemsTmp = new IHyperCubeDimension[dimNodes.Count];
            for (int ordinal = 1; ordinal <= dimNodes.Count; ordinal++) {
                var dimNode = dimNodes[ordinal - 1];
                dimensionItemsTmp[ordinal - 1] = new HyperCubeDimension(cube, dimNode);
            }

            HyperCubeOrdinalManager.SetDimensionOrdinals(cube, dimensionItemsTmp);
            _dimensionItems = (from dim in dimensionItemsTmp orderby dim.Ordinal select dim).ToArray();

            // private dict with ElementId/DimensionValue
            _cubeDimensionValues = new Dictionary<long, IHyperCubeDimensionValue>();

            foreach (IHyperCubeDimension cubeDimension in _dimensionItems) {
                foreach (IHyperCubeDimensionValue dimensionValue in cubeDimension.Values) {
                    _cubeDimensionValues.Add(dimensionValue.ElementId, dimensionValue);
                }
                //_cubeDimensionValues.Add(cubeDimension.Ordinal, cubeDimension.Values.First());
            }
            
            AllDimensionItems = DimensionItems.Union(new[] {Primary}).ToList();
        }
        #endregion constructor

        private readonly IHyperCubeDimension[] _dimensionItems;
        private Dictionary<long, IHyperCubeDimensionValue> _cubeDimensionValues;
        
        public IHyperCubeDimension Primary { get; private set; }
        public IEnumerable<IHyperCubeDimension> DimensionItems { get { return _dimensionItems; } }
        public IEnumerable<IHyperCubeDimension> AllDimensionItems { get; private set; }

        /// <summary>
        /// Find the DimensionValue for the given Id.
        /// </summary>
        /// <param name="id">ElementId</param>
        public IHyperCubeDimensionValue GetDimensionValueById(long id) {
            IHyperCubeDimensionValue result;
            if (_cubeDimensionValues.TryGetValue(id, out result)) {
                return result;
            }
            throw new Exception("Id unknown");
        }

        public IHyperCubeDimension GetDimension(int index) {
            if (index < 0 || index >= _dimensionItems.Length) 
                throw new IndexOutOfRangeException("Hypercube dimension index out of bound exception.");
            
            return _dimensionItems[index];
        }
    }
}