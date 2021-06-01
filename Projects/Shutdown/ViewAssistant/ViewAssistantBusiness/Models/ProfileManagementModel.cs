using Utils;
using ViewAssistantBusiness.Config;
using System.Linq;
using System.Collections.Generic;

namespace ViewAssistantBusiness.Models
{
    public class ProfileManagementModel : NotifyPropertyChangedBase
    {
        public ProfileManagementModel()
        {
            ConfigDb.ProfileManager.ProfilesChanged += UpdateItems;
        }

        public IEnumerable<ProfileConfigModel> Items
        {
            get { return ConfigDb.ProfileManager.Profiles.OrderBy(p => p.Name); }
        }

        #region SelectedItem
        private ProfileConfigModel _selectedItem;

        public ProfileConfigModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                OnPropertyChanged("SelectedItem");
            }
        }
        #endregion SelectedItem

        #region AddItem
        public void AddItem(ProfileConfigModel profile)
        {
            ConfigDb.ProfileManager.AddProfile(profile);
            SelectedItem = profile;
        }
        #endregion AddItem

        #region EditItem
        public void EditItem(ProfileConfigModel profile)
        {
            ConfigDb.ProfileManager.AddProfile(profile);
        }
        #endregion EditItem

        #region DeleteSelectedItem
        public void DeleteSelectedItem()
        {
            ConfigDb.ProfileManager.DeleteProfile(SelectedItem);
        }
        #endregion DeleteSelectedItem

        #region CopySelectedProfile
        public void CopySelectedProfile()
        {
            var profile = SelectedItem.Clone();
            profile.Id = 0;
            profile.Name = SelectedItem.Name + " (Copy)";
            ConfigDb.ProfileManager.AddProfile(profile);
            SelectedItem = profile;
        }
        #endregion CopySelectedProfile

        private void UpdateItems()
        {
            OnPropertyChanged("Items");
        }
    }
}
