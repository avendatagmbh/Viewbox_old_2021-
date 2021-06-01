// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2012-01-22
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using eBalanceKitBusiness.HyperCubes.DbMapping;
using eBalanceKitBusiness.HyperCubes.Interfaces.Structure;
using eBalanceKitBusiness.HyperCubes.Structures;
using eBalanceKitBusiness.Structures;

namespace eBalanceKitBusiness.HyperCubes.Manager {
    
    /// <summary>
    /// Helper class, used by HyperCubeDimensionSet ctor. Ordinal numbers are stored in database to ensure
    /// that the ordinal number does not change if the order of the dimensions in the hypercube has been
    /// changed.
    /// </summary>
    internal static class HyperCubeOrdinalManager {

        internal static void SetDimensionOrdinals(HyperCube cube, IEnumerable<IHyperCubeDimension> dimensionItemEnum) {

            var dimensionItems = dimensionItemEnum.ToList();

            // create dimension dictionary
            var dimensionsById = new Dictionary<int, HyperCubeDimension>();
            var ordinalByDimensionObject = new Dictionary<HyperCubeDimension, int>();

            foreach (var dim in dimensionItems)
                dimensionsById[cube.TaxonomyIdManager.GetId(dim.DimensionRoot.Element.Id)] = (HyperCubeDimension) dim;

            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                // assign existing ordinals
                foreach (
                    var dimensionOrdinal in
                        conn.DbMapping.Load<DbEntityHyperCubeDimensionOrdinal>(
                        conn.Enquote("taxonomy_id") + " = " + cube.TaxonomyId + " AND " + conn.Enquote("cube_element_id") + "=" + cube.CubeElementId)) {

                    // check ordinal boundaries
                    if (dimensionOrdinal.Ordinal < 1 ||
                        dimensionOrdinal.Ordinal > dimensionItems.Count())
                        throw new Exception(
                            "Database error: invalid dimension ordinal number " +
                            "(table=hypercube_ordinal, id=" + dimensionOrdinal.Id + ")");

                    // check references
                    if (!dimensionsById.ContainsKey(dimensionOrdinal.DimensionElementId))
                        throw new Exception(
                            "Database error: could not find referenced taxonomy dimension element " +
                            "(table=hypercube_ordinal, id=" + dimensionOrdinal.Id + ")");

                    var dim = dimensionsById[dimensionOrdinal.DimensionElementId];

                    // check multiple occurences
                    if (ordinalByDimensionObject.ContainsKey(dim))
                        throw new Exception(
                            "Database error: multiple occurences of same ordinal number " +
                            "(table=hypercube_ordinal, id=" + dimensionOrdinal.Id + ")");

                    ordinalByDimensionObject[dim] = dimensionOrdinal.Ordinal;
                }

                // create new ordinals
                var newItems = new List<DbEntityHyperCubeDimensionOrdinal>();
                foreach (var pair in dimensionsById) {
                    var newItem = new DbEntityHyperCubeDimensionOrdinal {
                        TaxonomyId = cube.TaxonomyId,
                        CubeElementId = cube.CubeElementId,
                        DimensionElementId = pair.Key,
                        Ordinal = ordinalByDimensionObject.Count + 1
                    };

                    if (ordinalByDimensionObject.ContainsKey(pair.Value)) continue;
                    ordinalByDimensionObject[pair.Value] = newItem.Ordinal;
                    newItems.Add(newItem);
                }

                // save new ordinal items
                if (newItems.Count > 0) {
                    try {
                        conn.BeginTransaction();
                        conn.DbMapping.Save(typeof (DbEntityHyperCubeDimensionOrdinal), newItems);
                        conn.CommitTransaction();
                    } catch (Exception) {
                        conn.RollbackTransaction();
                        throw;
                    }
                }

                foreach (var pair in ordinalByDimensionObject) {
                    pair.Key.Ordinal = pair.Value;
                }
            }
        }
    }
}