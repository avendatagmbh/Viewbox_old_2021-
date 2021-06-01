// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2012-01-16
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Taxonomy;
using Taxonomy.Enums;
using eBalanceKitBusiness.HyperCubes.DbMapping;
using eBalanceKitBusiness.HyperCubes.Interfaces.Structure;
using eBalanceKitBusiness.HyperCubes.Manager;
using eBalanceKitBusiness.HyperCubes.Structures.HyperCubeItem;
using eBalanceKitBusiness.Structures;

namespace eBalanceKitBusiness.HyperCubes.Structures {
    internal class HyperCubeItemCollection : IHyperCubeItemCollection {
        internal HyperCubeItemCollection(HyperCube cube) {
            Cube = cube;
            DimensionKeyManager = new HyperCubeDimensionKeyManager(Cube);
            InitItems();
            InitItemDependencies();
            InitItemAncestors();
            InitItemRelatives();
            InitValues();
        }

        protected HyperCubeDimensionKeyManager DimensionKeyManager { get; set; }

        #region private

        internal HyperCube Cube { get; set; }

        private readonly Dictionary<long, IHyperCubeItem> _items = new Dictionary<long, IHyperCubeItem>();

        #region AssignExistingItems
        private void AssignExistingItems() {
            foreach (var dbEntity in Cube.DbEntityHyperCube.Items) {
                if (_items.ContainsKey(dbEntity.DimensionId))
                    throw new Exception("Database error: found multiple Hypercube elements withs the same key.");

                _items[dbEntity.DimensionId] = CreateHypercubeItem(dbEntity);
            }
        }
        #endregion AssignExistingItems

        #region InitItems
        private void InitItems() {
            
            if (Cube.DbEntityHyperCube.Items.Count > 0)
                AssignExistingItems();

            int contextOrdinal = 1;

            var dimItems = new IHyperCubeDimensionValue[Cube.Dimensions.DimensionItems.Count()];

            var newEntities = new List<DbEntityHyperCubeItem>();
            InitItems(newEntities, ref contextOrdinal, dimItems);

            // save new entities
            if (newEntities.Count > 0) {
                using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                    try {
                        conn.BeginTransaction();
                        conn.DbMapping.Save(typeof (DbEntityHyperCubeItem), newEntities);
                        conn.CommitTransaction();
                    } catch {
                        conn.RollbackTransaction();
                        throw;
                    }
                }
            }
        }

        private void InitItems(List<DbEntityHyperCubeItem> newEntities,
                               ref int contextOrdinal, IHyperCubeDimensionValue[] dimItems, int index = 0) {

            if (index < Cube.Dimensions.DimensionItems.Count()) {
                foreach (var item in Cube.Dimensions.GetDimension(index).Values) {
                    dimItems[index] = item;
                    InitItems(newEntities, ref contextOrdinal, dimItems, index + 1);
                }
            } else {
                var context = new ScenarioContext(Cube.Root.Element.Id, contextOrdinal++);
                for (int i = 0; i < dimItems.Length; i++) {
                    var dimItem = dimItems[i];
                    context.AddMember(Cube.Dimensions.GetDimension(i).DimensionRoot.Element, dimItem.Element);
                }

                foreach (var item in Cube.Dimensions.Primary.Values) {
                    var dimId = DimensionKeyManager.GetDimensionKey(dimItems.Union(new[] { item }));
                    if (!_items.ContainsKey(dimId)) {
                        var dbEntity = new DbEntityHyperCubeItem {
                            HyperCube = Cube.DbEntityHyperCube,
                            Document = Cube.Document,
                            DimensionId = dimId
                        };

                        newEntities.Add(dbEntity);
                        _items[dimId] = CreateHypercubeItem(dbEntity, context, item.Element);

                    } else {
                        ((HyperCubeItemBase) _items[dimId]).Context = context;
                    }

                    if (_items[dimId] is HyperCubeMonetaryItem) {
                        var monetaryItem = _items[dimId] as HyperCubeMonetaryItem;
                        Debug.Assert(monetaryItem != null);
                        monetaryItem.SetAssignedDimensionValues(dimItems, item);
                    }
                }
            }
        }

        #endregion InitItems

        #region InitItemDependencies
        private void InitItemDependencies() {
            foreach (HyperCubeMonetaryItem item in _items.Values.OfType<HyperCubeMonetaryItem>()) {
                item.SetDirectDependencies();
            }
        }
        #endregion

        #region InitItemAncestors
        private void InitItemAncestors() {
            foreach (HyperCubeMonetaryItem item in _items.Values.OfType<HyperCubeMonetaryItem>()) {
                item.SetIndirectDependencies();
            }
        }
        #endregion

        #region SetItemRelatives
        private void InitItemRelatives() {
            var tmp = new HashSet<HyperCubeMonetaryItem>(_items.Values.OfType<HyperCubeMonetaryItem>());
            while (tmp.Count > 0) {
                var tmp1 = tmp.First().SetRelatives();
                foreach (var item in tmp1) tmp.Remove(item);
            }
        }
        #endregion

        #region InitValues
        private void InitValues() {
            foreach (HyperCubeMonetaryItem item in _items.Values.OfType<HyperCubeMonetaryItem>()) {
                item.InitManualValue();
            }
        }
        #endregion

        private IHyperCubeItem CreateHypercubeItem(
            DbEntityHyperCubeItem dbEntity,
            ScenarioContext context = null,
            IElement primaryDimensionElement = null) {

                if (primaryDimensionElement == null)
                    primaryDimensionElement =
                        DimensionKeyManager.GetPrimaryDimensionElement(dbEntity.DimensionId);

            switch (primaryDimensionElement.ValueType) {
                case XbrlElementValueTypes.Monetary:
                    return new HyperCubeMonetaryItem(this, dbEntity, primaryDimensionElement) {Context = context};

                case XbrlElementValueTypes.String:
                    return new HyperCubeStringItem(this, dbEntity, primaryDimensionElement) {Context = context};

                default:
                    return new HyperCubeAbstractItem(this, primaryDimensionElement);
            }            
        }


        internal IHyperCubeItem GetItem(
            IHyperCubeDimensionValue element1,
            IHyperCubeDimensionValue element2,
            IEnumerable<IHyperCubeDimensionValue> fixedDimensionCoordinates = null) {

            var id = DimensionKeyManager.GetDimensionKey(
                fixedDimensionCoordinates == null
                    ? new[] {element1, element2}
                    : new[] {element1, element2}.Union(fixedDimensionCoordinates));
            return GetItem(id);
        }
        #endregion private
            
        #region public

        public IHyperCubeItem GetItem(IEnumerable<IHyperCubeDimensionValue> dimensionCoordinates) {
            var id = DimensionKeyManager.GetDimensionKey(dimensionCoordinates);
            return GetItem(id);
        }

        public IHyperCubeItem GetItem(long dimensionKey) {
            if (!_items.ContainsKey(dimensionKey))
                throw new Exception("The requested item could not be found (dimension key = " + dimensionKey + ")");
            return _items[dimensionKey];
        }

        public IHyperCubeItem GetItemByID(int id) { 
            return GetItem(_items.ToList()[id].Key);
        }

        public long GetItemDimensionKey(int id) { 
            return _items.ToList()[id].Key;
        }

        public long GetItemDimensionKey(IHyperCubeItem cubeItem) {
            if (_items.ContainsValue(cubeItem)) {
                return (from item in _items where item.Value == cubeItem select item.Key).First();
            }
            throw new Exception("HyperCubeItem doesn't exist.");
            //return 0;
        }

        public IEnumerable<IHyperCubeItem> Items { get { return _items.Values; } }

        #endregion public
    }
}