using System.Collections.Generic;
using ViewValidator.Models.Profile;
using ViewValidatorLogic.Config;

namespace ViewValidator.Manager {
    public static class ProfileModelManager {
        private static Dictionary<ProfileConfig, ProfileModel> ProfileToModel = new Dictionary<ProfileConfig, ProfileModel>();

        public static ProfileModel GetModel(ProfileConfig profile) {
            if (profile == null) return null;
            if (!ProfileToModel.ContainsKey(profile)) ProfileToModel.Add(profile, new ProfileModel(profile));
            return ProfileToModel[profile];
        }
    }
}
