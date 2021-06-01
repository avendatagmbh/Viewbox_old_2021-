// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-09-08
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------

using System;
using System.Collections.ObjectModel;
using System.IO;
using AvdCommon.Logging;
using ScreenshotAnalyzerBusiness.Resources;
using ScreenshotAnalyzerBusiness.Structures.Config;
using ScreenshotAnalyzerDatabase.Manager;
using Utils;

namespace ScreenshotAnalyzerBusiness.Manager {

    /// <summary>
    /// This class provides functionality to persist and restore the profiles.
    /// </summary>
    public static class ProfileManager {

        static ProfileManager() {
            Profiles = new ObservableCollectionAsync<Profile>();
        }


        #region properties
        public static ObservableCollectionAsync<Profile> Profiles { get; private set; }
        #endregion properties


        #region methods
        public static Profile CreateNewProfile(string name) {
            return new Profile(DbProfileManager.CreateNewProfile(),name);
        }


        #region DeleteProfile
        public static void DeleteProfile(Profile profile) {
            //Delete SQLite database file
            new FileInfo(profile.DbConfig.Hostname).Delete();
            Profiles.Remove(profile);
        }
        #endregion

        #region ProfileExists
        public static bool ProfileExists(string profileName) {
            foreach(var profile in Profiles)
                if (profile.Name.ToLower() == profileName.ToLower())
                    return true;
            return false;

        }
        #endregion

        #region AddProfile
        public static void AddProfile(Profile profile, bool loadOnStartup = false) {
            try {
                DbProfileManager.AddProfile(profile.DbProfile, loadOnStartup);
            } catch (Exception ex) {
                profile.FaultedOnLoad = ex.Message;
                Profiles.Add(profile);
                LogManager.Error(ResourcesBusiness.ProfileManager_AddProfile_ErrorLoadingProfile + ex.Message);
                throw;
            }
            Profiles.Add(profile);
        }
        #endregion
        #endregion methods


        public static void Init() {
            //LoadProfiles();
        }

        public static Profile GetProfile(string name) {
            foreach (var profile in Profiles)
                if (profile.Name.ToLower() == name.ToLower())
                    return profile;
            return null;
        }
    }
}
