using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ScreenshotAnalyzerBusiness.Structures.Config;
using Utils;

namespace ScreenshotAnalyzer.Models.ProfileRelated {
    public class ProfileModel {
        #region Constructor
        public ProfileModel(Profile profile) {
            Profile = profile;
        }
        #endregion

        #region Properties
        public Profile Profile { get; set; }
        #endregion
    }
}
