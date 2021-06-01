// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2011-08-27
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Config.DbStructure;
using Config.Interfaces.DbStructure;
using DbAccess;
using Utils;

namespace Config.Manager {
    /// <summary>
    /// This class provides several profile management functions.
    /// </summary>
    public class ProfileManager : NotifyPropertyChangedBase {
        #region Constructor
        internal ProfileManager(IDatabase conn) {
            _profiles.Clear();
            foreach (Profile profile in conn.DbMapping.Load<Profile>()) {
                _profiles.Add(profile);
                profile.DoDbUpdate = true;
            }
        }
        #endregion Constructor

        #region private
        private readonly ObservableCollection<IProfile> _profiles = new ObservableCollection<IProfile>();

        #region VisibleProfiles
        private readonly ObservableCollection<IProfile> _visibleProfiles = new ObservableCollection<IProfile>();

        public IEnumerable<IProfile> VisibleProfiles { get { return _visibleProfiles; } }
        #endregion VisibleProfiles

        #endregion private

        #region internal

        #region InitVisibleProfiles
        internal void InitVisibleProfiles(IUser user) {
            // used for future profile security system
            // TODO: select all profiles, which are visible for the current user
            _visibleProfiles.Clear();
            foreach (IProfile profile in _profiles) {
                _visibleProfiles.Add(profile);
            }
        }
        #endregion InitVisibleProfiles

        #region ClearVisibleProfiles
        internal void ClearVisibleProfiles() { _visibleProfiles.Clear(); }
        #endregion ClearVisibleProfiles

        #endregion internal

        #region public

        #region LastProfile
        public IProfile LastProfile {
            get {
                int id;
                if (!int.TryParse(Info.LastProfile, out id)) return null;
                return _visibleProfiles.FirstOrDefault(profile => ((Profile) profile).Id == id);
            }
            set {
                if (!(value is Profile)) return;
                Info.LastProfile = (value as Profile).Id.ToString();
            }
        }
        #endregion LastProfile

        #region CreateProfile
        public IProfile CreateProfile() { return new Profile {DoDbUpdate = true}; }
        #endregion CreateProfile

        #region AddProfile
        public void AddProfile(IProfile profile) {
            _profiles.Add(profile);

            // TODO: add to visible profile only for allowed users
            _visibleProfiles.Add(profile);
        }
        #endregion AddProfile

        #region DeleteProfile
        public void DeleteProfile(IProfile profile) {
            _profiles.Remove(profile);
            _visibleProfiles.Remove(profile);
            ConfigDb.Delete(profile);
        }
        #endregion DeleteProfile

        #endregion public
    }
}