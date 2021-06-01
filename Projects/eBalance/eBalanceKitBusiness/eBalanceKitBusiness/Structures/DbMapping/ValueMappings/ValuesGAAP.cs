using DbAccess;
using eBalanceKitBusiness.Interfaces;

namespace eBalanceKitBusiness.Structures.DbMapping.ValueMappings {

    /// <summary>
    /// Each instance of this class represents a individual fact of the GAAP taxonomy.
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <since>2011-02-09</since>
    [DbTable("values_gaap", ForceInnoDb = true)]
    public class ValuesGAAP : ValueMappingBaseDocument, IValueMappingDocument {

        #region Parent
        [DbColumn("parent_id", AllowDbNull = true, IsInverseMapping = true)]
        public ValuesGAAP Parent { get; set; }
        IValueMapping IValueMapping.Parent { get { return Parent; } }
        #endregion

        protected override ValueMappingBase GetParent() {
            return Parent;
        }

        protected override void SetParent(ValueMappingBase parent) {
            Parent = (parent as ValuesGAAP);
        }
    }
}
