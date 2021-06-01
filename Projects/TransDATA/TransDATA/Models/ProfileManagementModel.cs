// (C)opyright 2011 AvenDATA GmbH
// ----------------------------------------
// author: Mirko Dibbert
// since : 2011-08-30
using System.Collections.Generic;
using System.Windows;
using Business;
using Config.Interfaces.DbStructure;
using TransDATA.Windows;
using Utils;
using System;

namespace TransDATA.Models {
    internal class ProfileManagementModel : NotifyPropertyChangedBase
    {

        public event EventHandler Closed;

        internal ProfileManagementModel(Window owner) { _owner = owner; }

        private Window _owner;

        #region Items
        public IEnumerable<IProfile> Items {
            get { return AppController.ProfileManager.VisibleProfiles; }
        }
        #endregion Items

        #region SelectedItem
        private IProfile _selectedItem;

        public IProfile SelectedItem {
            get { return _selectedItem ?? AppController.ProfileManager.LastProfile; }
            set {
                //if (_selectedItem != null && value != null) {
                //    _selectedItem.Save();
                //    _selectedItem.InputConfig.Save();
                //    _selectedItem.OutputConfig.Save();
                //}

                _selectedItem = value;
                OnPropertyChanged("SelectedItem");
            }
        }
        #endregion SelectedItem

        #region AddItem
        public void AddItem() {
            //_owner.Visibility = Visibility.Collapsed;
            try {
                DlgEditProfile dlgEditProfile = new DlgEditProfile(){Owner=_owner.Owner};
                EditProfileModel model = new EditProfileModel(null, dlgEditProfile);
                dlgEditProfile.DataContext = model;
                dlgEditProfile.ShowDialog();
                if (model.Saved) {
                    AppController.ProfileManager.AddProfile(model.Profile);
                    SelectedItem = model.Profile;
                }
            } finally {
                //_owner.Visibility = Visibility.Visible;
            }
            //IProfile profile = AppController.ProfileManager.CreateProfile();
            //profile.Save();
            //AppController.ProfileManager.AddProfile(profile);
            //SelectedItem = profile;
        }
        #endregion AddProfile
         
        #region DeleteSelectedItem
        public void DeleteSelectedItem() {
            if (SelectedItem == null) return;
            AppController.ProfileManager.DeleteProfile(SelectedItem);
        }
        #endregion DeleteSelectedItem

        internal void Close() {
            if(Closed != null)
                Closed(this, new EventArgs());
        }

        public void CopySelectedProfile() {
            var profile = SelectedItem.Clone();
            profile.Name = SelectedItem.Name + " (Copy)";
            AppController.ProfileManager.AddProfile(profile);
            profile.DoDbUpdate = true;
            profile.Save(false);
            profile.DoDbUpdate = false;
        }
    }
}