
using ViewboxAdmin_ViewModel.Structures.Config;

namespace ViewboxAdminBusiness.Manager
{
    public interface IXmlProfileManager {
        void LoadProfileFromFile(ref IProfile profile, string file);
        void SaveProfileToXml(IProfile profile, string path);
    }
}
