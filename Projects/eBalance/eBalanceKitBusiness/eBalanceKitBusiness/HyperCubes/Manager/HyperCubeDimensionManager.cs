// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2012-01-22
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using eBalanceKitBusiness.HyperCubes.DbMapping;
using eBalanceKitBusiness.HyperCubes.Interfaces.Structure;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures;

namespace eBalanceKitBusiness.HyperCubes.Manager {
    /// <summary>
    /// Helper class, used from HyperCubeDimensionKeyManager.
    /// </summary>
    internal class HyperCubeDimensionManager {

        private readonly Dictionary<long, DbEntityHyperCubeDimension> _keyToEntity =
            new Dictionary<long, DbEntityHyperCubeDimension>();

        public HyperCubeDimensionManager(IHyperCube cube, TaxonomyIdManager taxonomyIdManager, 
            IHyperCubeDimensionSet dimensions, int cubeElementId) {
            
            TaxonomyIdManager = taxonomyIdManager;

            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                HyperCubeDimensionEntities = conn.DbMapping.Load<DbEntityHyperCubeDimension>(
                    conn.Enquote("taxonomy_id") + " = " + cube.TaxonomyId + " AND " + conn.Enquote("cube_element_id") + "=" + cubeElementId).ToList();

                if (HyperCubeDimensionEntities.Count == 0) {
                    // create new dimension ids
                    CreateDimensionItems(cube.TaxonomyId, taxonomyIdManager, dimensions, cubeElementId);

                    // save new dimension ids
                    try {
                        conn.BeginTransaction();
                        conn.DbMapping.Save(typeof (DbEntityHyperCubeDimension), HyperCubeDimensionEntities);
                        conn.CommitTransaction();
                    } catch (Exception) {
                        conn.RollbackTransaction();
                        throw;
                    }
                }
            }

            foreach (var dimension in HyperCubeDimensionEntities) {
                _keyToEntity[dimension.Id] = dimension;
            }

        }

        private TaxonomyIdManager TaxonomyIdManager { get; set; }

        internal List<DbEntityHyperCubeDimension> HyperCubeDimensionEntities { get; private set; }

        private void CreateDimensionItems(
            int taxonomyId,
            TaxonomyIdManager taxonomyIdManager,
            IHyperCubeDimensionSet dimensions,
            int cubeElementId,
            int[] dimensionKeys = null,
            int index = 0) {

            if (dimensionKeys == null) 
                dimensionKeys = new int[dimensions.DimensionItems.Count()];

            if (index < dimensionKeys.Length) {
                foreach (var dimValue in dimensions.GetDimension(index).Values) {
                    dimensionKeys[index] = taxonomyIdManager.GetId(dimValue.Element.Id);
                    CreateDimensionItems(taxonomyId, taxonomyIdManager, dimensions, cubeElementId, dimensionKeys, index + 1);
                }
            } else {
                HyperCubeDimensionEntities.Add(new DbEntityHyperCubeDimension(taxonomyId, cubeElementId, dimensionKeys));
            }
        }

        public DbEntityHyperCubeDimension GetEntity(long dimensionId) {
            Debug.Assert(_keyToEntity.ContainsKey(dimensionId),
                         "HyperCubeDimensionKeyManager.GetEntity: dimension key not found.");
            return _keyToEntity[dimensionId];
        }
    }
}