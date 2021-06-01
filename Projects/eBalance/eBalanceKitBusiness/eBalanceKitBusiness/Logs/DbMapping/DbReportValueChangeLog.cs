// --------------------------------------------------------------------------------
// author: Benjamin Held / Mirko Dibbert
// since:  2011-07-01
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using DbAccess;

namespace eBalanceKitBusiness.Logs.DbMapping {
    [DbTable("log_report_1_value_change", ForceInnoDb = true)]
    internal class DbReportValueChangeLog : DbLogBase {
        #region Methods

        #region SetTableName
        public static void SetTableName(int id, IDatabase conn) { conn.DbMapping.SetTableName<DbReportValueChangeLog>(GetTableName(id)); }
        public static string GetTableName(int id) { return "log_report_" + id + "_value_change"; }
        #endregion

        #endregion

        #region Properties

        #region TaxonomyId
        [DbColumn("taxonomy_id", AllowDbNull = false)]
        public int TaxonomyId { get; set; }
        #endregion

        #region OldValue
        [DbColumn("old_value", AllowDbNull = true, Length = 100000)]
        public string OldValue { get; set; }
        #endregion

        #region NewValue
        [DbColumn("new_value", AllowDbNull = true, Length = 100000)]
        public string NewValue { get; set; }
        #endregion

        #region ReferenceId
        [DbColumn("reference_id", AllowDbNull = false)]
        public long ReferenceId { get; set; }
        #endregion

        #region ValueEnum
        public enum ValueTypes {
            Value = 0,
            OtherValue,
            ManualValue,
            IsManualValue,
            SendAccountBalances,
            AddToList,
            DeleteFromList
        }

        [DbColumn("value_type", AllowDbNull = false)]
        public ValueTypes ValueType { get; set; }
        #endregion

        //Saves the reference id which references one id in either of the three tables: values_gaap, values_gcd, values_gcd_company
        #endregion
    }
}