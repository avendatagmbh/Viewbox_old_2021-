using ViewboxAdmin.Models.ProfileRelated;
using ViewboxAdminBusiness.Manager;
using ViewboxAdmin_ViewModel.Structures.Config;

namespace ViewboxAdmin.Manager {
    public interface IProfileModelRepository {
        IProfileModel GetModel(IProfile profile);
    }
}