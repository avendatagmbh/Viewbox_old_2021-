using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using DbAccess;
using DbSearchBase.Interfaces;
using DbSearchDatabase.Config;
using DbSearchDatabase.Interfaces;

namespace DbSearchDatabase.Manager {
    public static class DbProfileManager {
        static DbProfileManager() {
            Profiles = new ObservableCollection<IDbProfile>();
        }

        #region Properties
        public static ObservableCollection<IDbProfile> Profiles { get; set; }
        #endregion

        #region Methods
        public static void LoadProfile(IDbProfile dbProfile) {
            ((DbProfile)dbProfile).IsLoaded = true;
        }

        public static IDbProfile CreateNewProfile() {
            return new DbProfile();
        }

        public static void DeleteProfile(IDbProfile iDbProfile) {
            Profiles.Remove(iDbProfile);
        }

        #region AddProfile
        public static bool AddProfile(IDbProfile iDbProfile, bool loadOnStartup) {
            DbProfile dbProfile = (DbProfile)iDbProfile;
            try {
                Profiles.Add(iDbProfile);
                using (IDatabase conn = dbProfile.GetOpenConnection()) {
                    if (!dbProfile.CheckDatabase(conn)) {
                        return false;
                    }
                    dbProfile.CreateTables(conn);
                    
                    List<DbProfile> profiles = conn.DbMapping.Load<DbProfile>();
                    if (profiles.Count != 1) {
                        dbProfile.Save();
                        //if (loadOnStartup) 
                        //    throw new ArgumentException("Konnte das Profil " + dbProfile.Name + " nicht laden, die Datenbank ist fehlerhaft.");
                    }
                    else dbProfile.Load(profiles[0]);
                }
            } catch (Exception ex) {
                throw new InvalidOperationException(ex.Message + Environment.NewLine + "Das Profil " + 
                    iDbProfile.Name + " (" + iDbProfile.DbConfigView.Hostname + "," + iDbProfile.DbConfigView.DbName + ") kann nicht geöffnet werden.");
            }
            return true;
        }
        #endregion AddProfile

        #endregion
    }
}
