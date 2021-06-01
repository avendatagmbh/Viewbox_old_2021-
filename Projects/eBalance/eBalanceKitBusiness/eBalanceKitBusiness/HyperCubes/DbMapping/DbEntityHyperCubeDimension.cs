// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2012-01-22
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using DbAccess;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness.HyperCubes.DbMapping {
    /// <summary>
    /// Assignment of a combinations of hypercube dimension members to a unique id.
    /// </summary>
    [DbTable("hypercube_dimensions", ForceInnoDb = true)]    
    internal class DbEntityHyperCubeDimension : DbEntityBase<long> {
        public DbEntityHyperCubeDimension() { }
        public DbEntityHyperCubeDimension(int taxonomyId, int cubeElementId, int[] dimensionKeys) {

            TaxonomytId = taxonomyId;
            CubeElementId = cubeElementId;

            if (dimensionKeys.Length > 10)
                throw new Exception("Hyper cubes with more than 10 dimensions are not yet supported.");
            
            for (int index = 1; index <= dimensionKeys.Length; index++) {
                switch (index) {
                    case 1:
                        ExplicitMemberElementId1 = dimensionKeys[index - 1];
                        break;

                    case 2:
                        ExplicitMemberElementId2 = dimensionKeys[index - 1];
                        break;

                    case 3:
                        ExplicitMemberElementId3 = dimensionKeys[index - 1];
                        break;

                    case 4:
                        ExplicitMemberElementId4 = dimensionKeys[index - 1];
                        break;

                    case 5:
                        ExplicitMemberElementId5 = dimensionKeys[index - 1];
                        break;

                    case 6:
                        ExplicitMemberElementId6 = dimensionKeys[index - 1];
                        break;

                    case 7:
                        ExplicitMemberElementId7 = dimensionKeys[index - 1];
                        break;

                    case 8:
                        ExplicitMemberElementId8 = dimensionKeys[index - 1];
                        break;

                    case 9:
                        ExplicitMemberElementId9 = dimensionKeys[index - 1];
                        break;

                    case 10:
                        ExplicitMemberElementId10 = dimensionKeys[index - 1];
                        break;
                }
            }
        }

        [DbColumn("taxonomy_id", AllowDbNull = true)]
        [DbIndex("idx_hypercubes")]
        public int TaxonomytId { get; set; }

        [DbColumn("cube_element_id")]
        [DbIndex("idx_hypercubes")]
        public int CubeElementId { get; set; }

        [DbColumn("explicit_member_element_id_1")]
        public int ExplicitMemberElementId1 { get; set; }

        [DbColumn("explicit_member_element_id_2")]
        public int ExplicitMemberElementId2 { get; set; }

        [DbColumn("explicit_member_element_id_3")]
        public int ExplicitMemberElementId3 { get; set; }

        [DbColumn("explicit_member_element_id_4")]
        public int ExplicitMemberElementId4 { get; set; }

        [DbColumn("explicit_member_element_id_5")]
        public int ExplicitMemberElementId5 { get; set; }

        [DbColumn("explicit_member_element_id_6")]
        public int ExplicitMemberElementId6 { get; set; }

        [DbColumn("explicit_member_element_id_7")]
        public int ExplicitMemberElementId7 { get; set; }

        [DbColumn("explicit_member_element_id_8")]
        public int ExplicitMemberElementId8 { get; set; }

        [DbColumn("explicit_member_element_id_9")]
        public int ExplicitMemberElementId9 { get; set; }

        [DbColumn("explicit_member_element_id_10")]
        public int ExplicitMemberElementId10 { get; set; }

        public int GetExplicitMemberValue(int index) {
            Debug.Assert(index > 0 && index <= 10, "Member index must be between 1 and 10.");
            switch (index) {
                case 1:
                    return ExplicitMemberElementId1;
                
                case 2:
                    return ExplicitMemberElementId2;
                
                case 3:
                    return ExplicitMemberElementId3;
                
                case 4:
                    return ExplicitMemberElementId4;
                
                case 5:
                    return ExplicitMemberElementId5;
                
                case 6:
                    return ExplicitMemberElementId6;
                
                case 7:
                    return ExplicitMemberElementId7;
                
                case 8:
                    return ExplicitMemberElementId8;
                
                case 9:
                    return ExplicitMemberElementId9;
                
                case 10:
                    return ExplicitMemberElementId10;
            }

            throw new IndexOutOfRangeException();
        }

        public IEnumerable<int> GetExplicitMemberValues() {
            var result = new List<int>();
            if (ExplicitMemberElementId1 > 0) result.Add(ExplicitMemberElementId1);
            if (ExplicitMemberElementId2 > 0) result.Add(ExplicitMemberElementId2);
            if (ExplicitMemberElementId3 > 0) result.Add(ExplicitMemberElementId3);
            if (ExplicitMemberElementId4 > 0) result.Add(ExplicitMemberElementId4);
            if (ExplicitMemberElementId5 > 0) result.Add(ExplicitMemberElementId5);
            if (ExplicitMemberElementId6 > 0) result.Add(ExplicitMemberElementId6);
            if (ExplicitMemberElementId7 > 0) result.Add(ExplicitMemberElementId7);
            if (ExplicitMemberElementId8 > 0) result.Add(ExplicitMemberElementId8);
            if (ExplicitMemberElementId9 > 0) result.Add(ExplicitMemberElementId9);
            if (ExplicitMemberElementId10 > 0) result.Add(ExplicitMemberElementId10);
            return result;
        }
    }
}