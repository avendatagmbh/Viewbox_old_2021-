// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2012-02-29
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using DbAccess;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness.Logs.DbMapping {
    internal class DbLogBase : DbEntityBase<long> {

        #region TimeStamp
        [DbColumn("timestamp")]
        public DateTime TimeStamp { get; set; }
        #endregion

        #region UserId
        [DbColumn("user_id", AllowDbNull = false)]
        public int UserId { get; set; }
        #endregion
         
    }
}