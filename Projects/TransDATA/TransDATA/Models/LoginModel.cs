// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-09-26
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Business;
using Business.Structures;
using Config.Interfaces.DbStructure;
using TransDATA.Windows;

namespace TransDATA.Models {
    public class LoginModel {
        public LoginModel(Window owner) {
            Owner = owner;
            if (AppController.IsInitialized) {
                Users = new List<IUser>(AppController.UserManager.Users);

                // set last user if any exist
                bool isAutoLogin = false;
                foreach (String itm in Environment.GetCommandLineArgs().Where(itm => itm.ToLower() == "-autologin"))
                    isAutoLogin=true;

                if (isAutoLogin){
                    foreach (IUser user in AppController.UserManager.Users){
                        if (user.UserName == "admin"){
                            SelectedUser = user;
                            break;
                        }
                    }
                } else if (!string.IsNullOrEmpty(AppController.UserConfig.LastUserName)){
                    foreach (IUser user in AppController.UserManager.Users) {
                        if (user.UserName == AppController.UserConfig.LastUserName) {
                            SelectedUser = user;
                            break;
                        }
                    }
                } else {
                    // set initial focus to username
                    if (Users.Count() > 0) SelectedUser = Users.FirstOrDefault();
                }
            }
        }

        private Window Owner { get; set; }

        public IEnumerable<IUser> Users { get; private set; }
        public IUser SelectedUser { get; set; }

        public IEnumerable<Language> Languages { get { return AppController.Languages; } }

        public Language SelectedLanguage {
            get { return AppController.SelectedLanguage; }
            set {
                Config.ConfigDb.LastLanguage = value.Culture.Name;

                AppController.SelectedLanguage = value;
                new DlgLogin().Show();
                Owner.Close();
            }
        }
    }
}