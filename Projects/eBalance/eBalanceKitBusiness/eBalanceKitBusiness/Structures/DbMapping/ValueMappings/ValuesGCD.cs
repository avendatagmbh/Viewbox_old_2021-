using System;
using DbAccess;
using eBalanceKitBusiness.Interfaces;
using Taxonomy;
using Taxonomy.PresentationTree;

namespace eBalanceKitBusiness.Structures.DbMapping.ValueMappings {

    /// <summary>
    /// Each instance of this class represents a individual fact of the GCD taxonomy, whereas
    /// the company values and the report period entries are excluded.
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <since>2011-02-13</since>
    [DbTable("values_gcd", ForceInnoDb = true)]
    public class ValuesGCD : ValueMappingBaseDocument, IValueMappingDocument {

        #region Parent
        [DbColumn("parent_id", AllowDbNull = true, IsInverseMapping = true)]
        public ValuesGCD Parent { get; set; }
        IValueMapping IValueMapping.Parent { get { return Parent; } }
        #endregion

        protected override ValueMappingBase GetParent() {
            return Parent;
        }

        protected override void SetParent(ValueMappingBase parent) {
            Parent = (parent as ValuesGCD);
        }
    }
}
