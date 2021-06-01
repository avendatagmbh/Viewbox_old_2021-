using DbAccess;
using eBalanceKitBusiness.Interfaces;

namespace eBalanceKitBusiness.Structures.DbMapping.ValueMappings {

    /// <summary>
    /// Each instance of this class represents a individual fact of a company entry in the GCD taxonomy.
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <since>2011-05-14</since>
    [DbTable("values_gcd_company", ForceInnoDb = true)]
    public class ValuesGCD_Company : ValueMappingBaseCompany, IValueMapping, IValueMappingCompany {

        #region Parent
        [DbColumn("parent_id", AllowDbNull = true, IsInverseMapping = true)]
        public ValuesGCD_Company Parent { get; set; }
        IValueMapping IValueMapping.Parent { get { return Parent; } }
        #endregion

        protected override ValueMappingBase GetParent() {
            return Parent;
        }

        protected override void SetParent(ValueMappingBase parent) {
            Parent = (parent as ValuesGCD_Company);
        }
    }
}
