// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-16
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using DbAccess;
using Utils;

namespace eBalanceKitBusiness.Structures.DbMapping {
    internal class DbEntityBase<TKeyType> {

        #region Id
        [DbColumn("id", AllowDbNull = false), DbPrimaryKey]
        public TKeyType Id { get; set; }
        #endregion
    }
}