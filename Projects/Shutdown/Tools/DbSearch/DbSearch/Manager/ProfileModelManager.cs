﻿using System.Collections.Generic;
using DbSearch.Models.Profile;
using DbSearchBase.Interfaces;
using DbSearchLogic.Config;

namespace DbSearch.Manager {
    public static class ProfileModelManager {
        private static Dictionary<Profile, ProfileModel> ProfileToModel = new Dictionary<Profile, ProfileModel>();

        public static ProfileModel GetModel(Profile profile) {
            lock (ProfileToModel) {
                if (profile == null) return null;
                if (!ProfileToModel.ContainsKey(profile)) ProfileToModel.Add(profile, new ProfileModel(profile));
                return ProfileToModel[profile];
            }
        }
    }
}
