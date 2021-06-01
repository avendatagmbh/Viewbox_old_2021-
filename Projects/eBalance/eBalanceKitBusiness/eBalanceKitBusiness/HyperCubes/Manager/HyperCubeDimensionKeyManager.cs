// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2012-01-22
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Taxonomy;
using eBalanceKitBusiness.HyperCubes.DbMapping;
using eBalanceKitBusiness.HyperCubes.Interfaces.Structure;
using eBalanceKitBusiness.HyperCubes.Structures;
using eBalanceKitBusiness.Structures;

namespace eBalanceKitBusiness.HyperCubes.Manager {
    /// <summary>
    /// Internal helper class for mapping between hyper cube keys and HyperCubeDimensionValue objects. Used from HyperCubeItemCollection.
    /// </summary>
    internal class HyperCubeDimensionKeyManager {
        private readonly Dictionary<long, IEnumerable<IHyperCubeDimensionValue>> _dimKeyToValueEnumeration =
            new Dictionary<long, IEnumerable<IHyperCubeDimensionValue>>();

        private readonly Dictionary<string, long> _valueToDimKey = new Dictionary<string, long>();

        #region constructor
        internal HyperCubeDimensionKeyManager(HyperCube cube) {

            Debug.Assert(cube.Dimensions != null, "hypercube dimensions must be initialized!");

            int cubeElementId = cube.CubeElementId;
            var dimensionManager = new HyperCubeDimensionManager(cube, cube.TaxonomyIdManager, cube.Dimensions, cubeElementId);

            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                var items = conn.DbMapping.Load<DbEntityHyperCubeDimensionKey>(
                    conn.Enquote("taxonomy_id") + " = " + cube.TaxonomyId + " AND " + conn.Enquote("cube_element_id") + "=" + cubeElementId).ToList();

                if (items.Count == 0) {
                    // create new dimension key ids
                    items = (from dim in dimensionManager.HyperCubeDimensionEntities
                             from node in cube.Dimensions.Primary.Values
                             select new DbEntityHyperCubeDimensionKey {
                                 TaxonomytId = cube.TaxonomyId,
                                 CubeElementId = cubeElementId,
                                 PrimaryDimensionId = cube.TaxonomyIdManager.GetId(node.Element.Id),
                                 DimensionId = dim.Id,
                             }).ToList();

                    // save new dimension ids
                    try {
                        conn.BeginTransaction();
                        conn.DbMapping.Save(typeof (DbEntityHyperCubeDimensionKey), items);
                        conn.CommitTransaction();
                    } catch (Exception) {
                        conn.RollbackTransaction();
                        throw;
                    }
                }

                var elementIdToHyperCubeDimensionValue =
                    cube.Dimensions.AllDimensionItems.SelectMany(dimension => dimension.Values).
                        ToDictionary(dimensionValue => cube.TaxonomyIdManager.GetId(dimensionValue.Element.Id));

                foreach (DbEntityHyperCubeDimensionKey item in items) {
                    var dimensionEntity = dimensionManager.GetEntity(item.DimensionId);
                    var testSev = dimensionEntity.GetExplicitMemberValues().Union(new[] { item.PrimaryDimensionId });
                    var dimValues =
                        dimensionEntity.GetExplicitMemberValues().Union(new[] {item.PrimaryDimensionId}).Select(
                            id => elementIdToHyperCubeDimensionValue[id]).OrderBy(dimValue => dimValue.Dimension.Ordinal)
                            .ToArray();

                    var keys =
                        (from HyperCubeDimensionValue dimValue in dimValues
                         select dimValue.ElementId.ToString(CultureInfo.InvariantCulture)).ToList();

                    _valueToDimKey[string.Join("|", keys)] = item.Id;
                    _dimKeyToValueEnumeration[item.Id] = dimValues.ToList();
                }
            }
        }
        #endregion constructor

        #region GetDimensionKey
        public long GetDimensionKey(IEnumerable<IHyperCubeDimensionValue> dimensionKeys) {
            string key = string.Join("|", (from dimensionKey in dimensionKeys
                                           orderby dimensionKey.Dimension.Ordinal
                                           select
                                               ((HyperCubeDimensionValue) dimensionKey).ElementId.ToString(
                                                   CultureInfo.InvariantCulture)));
            Debug.Assert(_valueToDimKey.ContainsKey(key), "dimension key not found!");
            return _valueToDimKey[key];
        }
        #endregion GetDimensionKey

        // ToDo: Test by sev
        #region GetDimensionKey
        public long GetDimensionKey(long[] dimensionKeys) {
            string key = string.Join("|", dimensionKeys);
            Debug.Assert(_valueToDimKey.ContainsKey(key), "dimension key not found!");
            return _valueToDimKey[key];
        }
        #endregion GetDimensionKey

        #region GetDimensionValues
        public IEnumerable<IHyperCubeDimensionValue> GetDimensionValues(long key) { return _dimKeyToValueEnumeration[key]; }
        #endregion GetDimensionValues

        #region GetPrimaryDimensionElement
        public IElement GetPrimaryDimensionElement(long key) {
            IHyperCubeDimensionValue dimValue = GetDimensionValues(key).First();
            Debug.Assert(dimValue.Dimension.Ordinal == 0);
            return dimValue.Element;
        }

        #endregion GetPrimaryDimensionElement
    }
}