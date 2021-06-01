using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ViewboxAdminBusiness.Manager;

namespace ViewboxAdminBusiness.Factories
{
    public static class ProfileManagerFactory
    {
        public static IProfileManager Create() {
            IApplicationManager appmanager = ApplicationConfigManagerFactory.Create();
            IFileManager filemanager = new FileManager();
            IXmlProfileManager xmlprofilemanager = new XmlProfileManager();
            IProfileManager profmanage = new ProfileManager(appmanager, filemanager,
                                      xmlprofilemanager);
            profmanage.LoadProfiles();
            return profmanage;
        }
    }
}
