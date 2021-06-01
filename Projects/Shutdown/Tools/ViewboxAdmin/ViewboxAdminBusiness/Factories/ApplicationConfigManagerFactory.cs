using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ViewboxAdminBusiness.Manager;

namespace ViewboxAdminBusiness.Factories
{
    public static class ApplicationConfigManagerFactory
    {
        public static IApplicationManager Create() {
            //controling object creation pattern here
            IFileManager filemanager = new FileManager();
            IXmlConfigManager xmlconfigmanager = new XmlConfigManager();
            IApplicationManager appmanager = new ApplicationConfigManager(filemanager,xmlconfigmanager);
            appmanager.Load();
            return appmanager;
        }
    }
}
