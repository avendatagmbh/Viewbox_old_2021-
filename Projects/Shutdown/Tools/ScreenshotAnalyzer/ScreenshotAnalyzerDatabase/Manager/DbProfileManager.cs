// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-12-05
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DbAccess;
using DbSearchDatabase.Config;
using ScreenshotAnalyzerDatabase.Config;
using ScreenshotAnalyzerDatabase.Interfaces;
using ScreenshotAnalyzerDatabase.Resources;

namespace ScreenshotAnalyzerDatabase.Manager {
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
                        if (loadOnStartup)
                            throw new ArgumentException(string.Format(ResourcesDatabase.DbProfileManager_AddProfile_Error_DatabaseCorrupted, dbProfile.Name));
                        dbProfile.Save(true);
                    }
                    else dbProfile.Load(profiles[0]);
                }
            } catch (Exception ex) {
                throw new InvalidOperationException(ex.Message + Environment.NewLine + string.Format(ResourcesDatabase.DbProfileManager_AddProfile_Error_UnableToOpenProfile, iDbProfile.Name, iDbProfile.DbConfig.Hostname));
            }
            return true;
        }
        #endregion AddProfile

        #endregion
    }
}
