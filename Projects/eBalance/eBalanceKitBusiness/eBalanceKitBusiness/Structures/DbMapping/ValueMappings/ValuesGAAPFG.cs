// --------------------------------------------------------------------------------
// author: Solueman Hussain
// since: 2012-01-03
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using DbAccess;
using eBalanceKitBusiness.Interfaces;

namespace eBalanceKitBusiness.Structures.DbMapping.ValueMappings {
    [DbTable("values_gaap_fg", ForceInnoDb = true)]

    public class ValuesGAAPFG :ValueMappingBaseDocument, IValueMapping, IValueMappingDocument 
    {

        #region Parent
        [DbColumn("parent_id", AllowDbNull = true, IsInverseMapping = true)]
        public ValuesGAAPFG Parent { get; set; }
        IValueMapping IValueMapping.Parent { get { return Parent; } }
        #endregion
        
        protected override ValueMappingBase GetParent() { return Parent; }

        protected override void SetParent(ValueMappingBase parent) { Parent = parent as ValuesGAAPFG; }

    }
}