using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using TransDATABusiness.ConfigDb.Tables;
using DbAccess;
using DbAccess.Structures;
using System.Data.Common;

namespace TransDATABusiness.Manager {
    public static class ProfileManager {

         static ProfileManager() {
             ProfileNames = new ObservableCollection<string>();
        }

        /*********************************************************************/

        #region properties

        public static ObservableCollection<string> ProfileNames { get; private set; }

        #endregion properties
        /*********************************************************************/

        #region methods

        public static Profile GetProfile(string name) {
            using (IDatabase db = Global.AppConfig.ConfigDb.ConnectionManager.GetConnection()) {
                db.Open();
                return Profile.Load(db, name);
            }
        }

        public static void LoadProfiles() {
            ProfileNames.Clear();
            using(IDatabase db = Global.AppConfig.ConfigDb.ConnectionManager.GetConnection()) {
                db.Open();
                foreach(Profile prof in db.DbMapping.Load<Profile>()){
                    ProfileNames.Add(prof.Name);
                }
            }
        }

        #endregion methods

    }
}
