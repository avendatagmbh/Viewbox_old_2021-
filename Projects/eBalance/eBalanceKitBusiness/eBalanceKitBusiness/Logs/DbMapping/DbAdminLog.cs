// --------------------------------------------------------------------------------
// author: Benjamin Held / Mirko Dibbert
// since:  2011-07-01
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using DbAccess;
using eBalanceKitBusiness.Logs.Enums;

namespace eBalanceKitBusiness.Logs.DbMapping {
    [DbTable("log_admin", ForceInnoDb = true)]
    internal class DbAdminLog : DbLogBase {
        #region Properties

        #region ContentEnum
        private AdminLogContentTypes _contentType;

        [DbColumn("content_type", AllowDbNull = false)]
        public AdminLogContentTypes ContentType {
            get { return _contentType; }
            set {
                if (_contentType == value) return;
                _contentType = value;
            }
        }
        #endregion

        #region ActionEnum
        private ActionTypes _actionType;

        [DbColumn("action_type", AllowDbNull = false)]
        public ActionTypes ActionType {
            get { return _actionType; }
            set {
                if (_actionType == value) return;
                _actionType = value;
            }
        }
        #endregion

        #region ReferenceId
        private long _referenceId;

        [DbColumn("reference_id", AllowDbNull = false)]
        public long ReferenceId {
            get { return _referenceId; }
            set {
                if (_referenceId != value) {
                    _referenceId = value;
                }
            }
        }
        #endregion

        #region OldValue
        [DbColumn("old_value", AllowDbNull = true, Length = 100000)]
        public string OldValue { get; set; }
        #endregion

        #region NewValue
        [DbColumn("new_value", AllowDbNull = true, Length = 100000)]
        public string NewValue { get; set; }
        #endregion

        #region InfoValue
        [DbColumn("info", AllowDbNull = true, Length = 4096)]
        public string Info { get; set; }
        #endregion

        #endregion
    }
}