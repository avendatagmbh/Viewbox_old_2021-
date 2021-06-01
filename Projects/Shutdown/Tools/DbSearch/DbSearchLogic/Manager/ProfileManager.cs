// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-09-08
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------

using System;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using DbSearchBase.Interfaces;
using DbSearchLogic.Config;
using DbSearchLogic.Localisation;
using log4net;
using AV.Log;

namespace DbSearchLogic.Manager {

    /// <summary>
    /// This class provides functionality to persist and restore the profiles.
    /// </summary>
    public static class ProfileManager {

        internal static ILog _log = LogHelper.GetLogger();

        static ProfileManager() {
            //Profiles = DbSearchDatabase.Manager.DbProfileManager.Profiles;
            Profiles = new ObservableCollection<Profile>();
        }


        #region properties
        public static ObservableCollection<Profile> Profiles { get; private set; }
        #endregion properties


        #region methods
        public static Profile CreateNewProfile() {
            return new Profile(DbSearchDatabase.Manager.DbProfileManager.CreateNewProfile());
        }


        #region DeleteProfile
        public static void DeleteProfile(Profile profile) {
            Profiles.Remove(profile);
            //DbSearchDatabase.Manager.DbProfileManager.DeleteProfile(dbProfile);
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
                DbSearchDatabase.Manager.DbProfileManager.AddProfile(profile.DbProfile, loadOnStartup);
            } catch (Exception ex) {
                profile.FaultedOnLoad = ex.Message;
                Profiles.Add(profile);
                _log.Log(LogLevelEnum.Error, "Fehler beim laden des Profils: " + ex.Message);
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
