using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;
using ViewboxAdmin.Models.ProfileRelated;
using ViewboxAdminBusiness.Manager;
using ViewboxAdmin_ViewModel.Structures.Config;

namespace ViewboxAdmin.Manager
{
    public class ProfileModelRepository : IProfileModelRepository {
        private Dictionary<IProfile, IProfileModel> _profileModelRepository = new Dictionary<IProfile, IProfileModel>();

        public IProfileModel GetModel(IProfile profile) {
            if (profile == null) return null;
            if (!_profileModelRepository.ContainsKey(profile)) _profileModelRepository.Add(profile,this.CreateProfileModel(profile));
            return _profileModelRepository[profile];
        }

        public IProfileModel CreateProfileModel(IProfile profile) {
            IProgressCalculator progresscalculator = new ProgressCalculator();
            IProfilePartLoadingEnumHelper profileenumhelper = new ProfilePartLoadingEnumHelper();
            progresscalculator.SetWorkSteps(profileenumhelper.LengthOfEnum(),false);
            return new ProfileModel(profile,progresscalculator,profileenumhelper);
        }
    }
}
