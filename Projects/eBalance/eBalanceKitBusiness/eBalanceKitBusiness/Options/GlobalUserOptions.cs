// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-04-17
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Diagnostics;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness.Options {
    /// <summary>
    /// Static class to access the global settings or the methode for loading <see cref="AvailableOptions"/>.
    /// </summary>
    public static class GlobalUserOptions {

        #region UserOptions
        /// <summary>
        /// The Options of the current User.
        /// </summary>
        public static IOptions UserOptions {
            get {
                return UserManager.Instance.CurrentUser != null ?
                    UserManager.Instance.CurrentUser.Options :
                    new AvailableOptions(); // default user option object needed from designer
            }
        }
        #endregion UserOptions
        
        public static void Load() {
            Load(UserManager.Instance.CurrentUser);
        }

        public static void Load(User user) {
            IOptions result;
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                var entries = conn.DbMapping.Load<AvailableOptions>(conn.Enquote("user_id") + " = " + user.Id);
                foreach (var entry in entries)
                    entry.User = user;
                Debug.Assert(entries.Count == 0 || entries.Count == 1, "More than one dataset for the user with id = " + user.Id);

                result = entries.Count == 0 ? new AvailableOptions(user) : entries[0];
            }
            user.Options = result;
        }
    }
}