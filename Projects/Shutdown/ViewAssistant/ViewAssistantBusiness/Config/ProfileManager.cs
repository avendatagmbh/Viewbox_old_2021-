// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2011-08-27
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.ObjectModel;
using System.Linq;
using DbAccess;
using Utils;

namespace ViewAssistantBusiness.Config
{
    /// <summary>
    /// This class provides several profile management functions.
    /// </summary>
    public class ProfileManager : NotifyPropertyChangedBase
    {
        private ConnectionManager _manager;
        public delegate void ProfilesChangedDelegate();
        public event ProfilesChangedDelegate ProfilesChanged;

        #region Constructor
        internal ProfileManager(ConnectionManager manager)
        {
            _manager = manager;
            Profiles.Clear();
            using (var conn = _manager.GetConnection())
            {
                foreach (ProfileConfig profile in conn.DbMapping.Load<ProfileConfig>())
                {
                    Profiles.Add(new ProfileConfigModel(profile));
                }
            }
        }
        #endregion Constructor

        #region private

        public ObservableCollection<ProfileConfigModel> Profiles = new ObservableCollection<ProfileConfigModel>();

        #endregion private

        #region public

        #region AddProfile
        public void AddProfile(ProfileConfigModel profileModel)
        {
            using (var conn = _manager.GetConnection())
            {
                var profile = conn.DbMapping.Load<ProfileConfig>().FirstOrDefault(x => x.Id == profileModel.Id);

                if (profile != null)
                {
                    var temp = Profiles.FirstOrDefault(x => x.Id == profileModel.Id);
                    temp.SetProperties(profileModel);
                    profile.SetProfileConfigDatas(profileModel);
                    ConfigDb.Save(profile);
                }
                else
                {
                    Profiles.Add(profileModel);
                    profile = new ProfileConfig(profileModel);
                    ConfigDb.Save(profile);
                    profileModel.Id = profile.Id;
                }
                if (ProfilesChanged != null)
                    ProfilesChanged();
            }
        }
        #endregion AddProfile

        #region DeleteProfile
        public void DeleteProfile(ProfileConfigModel profileModel)
        {
            using (var conn = _manager.GetConnection())
            {
                var profile = conn.DbMapping.Load<ProfileConfig>().FirstOrDefault(x => x.Id == profileModel.Id);
                Profiles.Remove(profileModel);
                ConfigDb.Delete(profile);

                if (ProfilesChanged != null)
                    ProfilesChanged();
            }
        }
        #endregion DeleteProfile

        #endregion public
    }
}